# Sistema de gestión de facturas

Aplicación full-stack para importar facturas desde `bd_exam.json`, administrarlas
en una base SQLite y consultar reportes de cobranza e inconsistencias.

- **Backend:** .NET 8 · ASP.NET Core Web API · Entity Framework Core 8 · SQLite · Swagger
- **Frontend:** React 19 · TypeScript · Vite · React Router
- **Pruebas:** xUnit (16 pruebas)
- **Patrón:** Clean Architecture + Repository + Unit of Work

---

## Inicio rápido

Requisitos: [.NET SDK 8.0+](https://dotnet.microsoft.com/download/dotnet/8.0) y
[Node.js 20.19+ o 22.12+](https://nodejs.org/).

```bash
git clone https://github.com/CarlosAndres1983/facturas-exam.git
cd facturas-exam
dotnet run --project src/Facturas.Api
```

En otra terminal, desde la raíz del repositorio:

```bash
cd frontend
npm install
npm run dev
```

Abrir **http://localhost:5173**. La base SQLite se crea sola, las vistas se crean
solas y el archivo `bd_exam.json` se importa automáticamente. No hay pasos manuales
de base de datos.

- API: **http://localhost:5000**
- Swagger: **http://localhost:5000/swagger**

---

## 1. Arquitectura

El backend está separado en capas con dependencias hacia adentro:

```
Facturas.Api            → controladores, Swagger, CORS, middleware HTTP
Facturas.Infrastructure → EF Core, SQLite, repositorios, importación, vistas
Facturas.Application    → casos de uso, DTOs, interfaces, reglas de aplicación
Facturas.Domain         → entidades y reglas de negocio puras
```

### Patrón implementado

- **Clean Architecture:** el dominio no conoce EF Core, HTTP ni SQLite.
- **Repository:** `IInvoiceRepository` e `IReportRepository` abstraen el acceso a datos.
- **Unit of Work:** `IUnitOfWork` se implementa con `AppDbContext.SaveChangesAsync`.
- **Read models + vistas SQL:** la búsqueda y los reportes se resuelven con vistas
  (`vw_InvoiceSearch`, `vw_PaymentSummary`, `vw_OverdueWithoutPaymentOrCredit`,
  `vw_InconsistentInvoices`), tal como pide el enunciado.
- **Inyección de dependencias** en `DependencyInjection.cs` de cada capa.
- **Reloj inyectable** (`IClock`) para que las reglas de vencimiento sean testeables.

### Estructura

```
facturas-exam/
├── bd_exam.json                 # Copia de referencia del archivo de entrada
├── database/
│   └── schema.sql               # Script SQL de tablas + vistas (referencia)
├── src/
│   ├── Facturas.Domain/
│   ├── Facturas.Application/
│   ├── Facturas.Infrastructure/
│   └── Facturas.Api/
│       └── Data/bd_exam.json    # Archivo que lee la importación
├── tests/Facturas.Tests/
├── frontend/
└── Facturas.sln
```

---

## 2. Requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) o superior
  (el proyecto apunta a `net8.0`; el SDK 9 también compila).
- [Node.js 20.19+ o 22.12+](https://nodejs.org/) y npm (lo exige Vite 8).
- No se necesita instalar SQLite: el proveedor `Microsoft.Data.Sqlite` viene en el proyecto.

---

## 3. Ejecución

### Backend

Desde la raíz del repositorio:

```bash
dotnet restore
dotnet run --project src/Facturas.Api
```

La API queda en **http://localhost:5000** y Swagger en
**http://localhost:5000/swagger**.

Al arrancar:

1. Crea la base `facturas.db` si no existe (`EnsureCreated`).
2. Crea las vistas SQL.
3. Importa `src/Facturas.Api/Data/bd_exam.json` de forma idempotente.

> Para reiniciar los datos desde cero, detén la API, elimina
> `src/Facturas.Api/facturas.db` y vuelve a ejecutar.

### Frontend

En otra terminal:

```bash
cd frontend
npm install
npm run dev
```

Abre **http://localhost:5173**. Vite redirige `/api` a `http://localhost:5000`,
por lo que no hay problemas de CORS en desarrollo.

Si necesitas apuntar a otra URL de API:

```bash
# frontend/.env.local
VITE_API_URL=http://localhost:5000
```

### Pruebas

```bash
dotnet test
```

Las 16 pruebas cubren las reglas críticas y una importación real del JSON en
SQLite en memoria:

- Consistencia de montos (incluye los casos reales 41 y 48).
- Estado de factura (Issued / Partial / Cancelled, incluido el caso 30).
- Estado de pago (Paid / Pending / Overdue).
- Validación de notas de crédito (monto igual al saldo y monto que lo supera).
- Importación idempotente de las 50 facturas.
- Vistas y reportes (13 pagadas, 35 vencidas, 30 del reporte de 30 días, 2 inconsistentes).

---

## 4. Autenticación

**No se implementó autenticación.** El enunciado la marca como opcional y se
priorizó la estabilidad y la claridad del resto de la solución. Por lo tanto no
hay credenciales que informar.

---

## 5. Reglas de negocio

### 5.1 Coherencia de la factura

Una factura es **consistente** cuando:

```
suma(subtotales declarados de los productos) == total_amount
```

- Si no coincide, se marca `IsConsistent = false`.
- Se conserva en la base para el reporte de inconsistencias.
- Se excluye del sistema activo: no aparece en la búsqueda por defecto y no
  admite notas de crédito.

Se comparan **enteros exactos**. No se usa `float`, `double` ni tolerancias, porque
el archivo contiene diferencias deliberadamente pequeñas (factura 48: 6 pesos).

### 5.2 Estado de la factura (calculado)

| Condición | Estado |
|---|---|
| Sin notas de crédito | `Issued` |
| Suma NC **igual o mayor** al total | `Cancelled` |
| Suma NC menor al total | `Partial` |

No se usa el campo `invoice_status` del JSON: se recalcula siempre.

### 5.3 Estado de pago (calculado)

| Condición | Estado |
|---|---|
| `payment_date` informada | `Paid` |
| Sin pago y `payment_due_date < hoy` | `Overdue` |
| Sin pago y `payment_due_date >= hoy` | `Pending` |

El estado de pago es independiente del estado de la factura. Por ejemplo, una
factura puede estar `Paid` y `Partial` a la vez (facturas 2 y 32).

### 5.4 Notas de crédito

- La fecha se asigna automáticamente con el reloj del servidor.
- El número se genera automáticamente (`máximo + 1`).
- El monto debe ser entero, mayor que cero y **menor o igual** al saldo pendiente:
  `total_amount - suma(NC existentes)`.
- Se permite una NC exactamente igual al saldo (deja la factura en `Cancelled`).
- Se rechaza si la factura es inconsistente o si no tiene saldo pendiente.

### 5.5 Reportes

1. **Resumen por estado de pago:** facturas consistentes, con cantidad, monto y
   porcentaje para `Paid`, `Pending` y `Overdue`.
2. **Vencidas más de 30 días sin pago ni nota de crédito:** facturas consistentes,
   sin `payment_date`, sin NC y con `payment_due_date < hoy - 30 días`.
3. **Facturas inconsistentes:** total declarado vs suma de subtotales y diferencia.

### 5.6 Supuestos documentados

- La frase *"sin pago o nota de crédito"* se interpreta como **sin pago y sin nota
  de crédito**. Es la lectura conservadora para el reporte de cobranza.
- El resumen por estado de pago se calcula sobre las **facturas consistentes**
  (sistema activo). Las inconsistentes se reportan aparte.
- Las fechas se manejan como `DateOnly` (sin hora) y el "hoy" usa UTC. La regla está
  encapsulada en `IClock`.
- La importación es idempotente: si el número de factura ya existe, se omite para no
  duplicar ni pisar notas de crédito agregadas por el usuario.

---

## 6. API REST

Base: `http://localhost:5000`

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| GET | `/api/invoices?number=&invoiceStatus=&paymentStatus=&includeInconsistent=&page=&pageSize=` | Búsqueda paginada usando `vw_InvoiceSearch` | 200, 400 |
| GET | `/api/invoices/{invoiceNumber}` | Detalle con productos, pago y NC | 200, 404 |
| POST | `/api/invoices/{invoiceNumber}/credit-notes` | Agrega NC. Body: `{ "amount": 1000 }` | 201, 400, 404, 409 |
| POST | `/api/invoices/import` | Reejecuta la importación idempotente | 200 |
| GET | `/api/reports/payment-summary` | Resumen por estado de pago | 200 |
| GET | `/api/reports/overdue-without-payment-or-credit` | Vencidas > 30 días sin pago ni NC | 200 |
| GET | `/api/reports/inconsistent-invoices` | Facturas inconsistentes | 200 |
| GET | `/health` | Health check | 200 |

Los errores se devuelven como `application/problem+json` (`ProblemDetails`).

### Ejemplos

```bash
# Buscar facturas vencidas
curl "http://localhost:5000/api/invoices?paymentStatus=Overdue&pageSize=5"

# Ver la factura 41 (inconsistente)
curl "http://localhost:5000/api/invoices/41"

# Agregar una nota de crédito de 1.000 a la factura 1
curl -X POST "http://localhost:5000/api/invoices/1/credit-notes" \
  -H "Content-Type: application/json" \
  -d '{"amount":1000}'

# Reportes
curl "http://localhost:5000/api/reports/payment-summary"
curl "http://localhost:5000/api/reports/overdue-without-payment-or-credit"
curl "http://localhost:5000/api/reports/inconsistent-invoices"
```

---

## 7. Vistas SQL

| Vista | Uso |
|---|---|
| `vw_InvoiceSearch` | Búsqueda paginada y base de los reportes. Calcula estados y saldos. |
| `vw_PaymentSummary` | Agrupación por estado de pago. |
| `vw_OverdueWithoutPaymentOrCredit` | Reporte de vencidas > 30 días. |
| `vw_InconsistentInvoices` | Reporte de inconsistencias. |

El script completo está en `database/schema.sql`. La aplicación las crea
automáticamente al iniciar.

---

## 8. Hallazgos sobre los datos de `bd_exam.json`

El archivo contiene **trampas deliberadas**. La solución las maneja explícitamente:

| Hallazgo | Detalle | Tratamiento |
|---|---|---|
| Estados de factura incorrectos | Factura **30** dice `canceled` pero no tiene NC | Se recalcula a `Issued` |
| Estados de pago desactualizados | 36 facturas dicen `Pending` y ya están vencidas | Se recalcula con la fecha actual |
| `days_to_due` inconsistente | Factura 1: 75 vs 198 días reales; 3, 6, 8, 23, 25 y 30 también | Se ignora; se calcula con las fechas |
| Inconsistencia grande | Factura **41**: subtotales 828.336 vs total 691.171 | Se marca inconsistente y se excluye |
| Inconsistencia pequeña | Factura **48**: 560.418 vs 560.412 (6 pesos) | Se compara exacto; se marca inconsistente |
| RUN no numérico | `9G13WC4RU71`, `24753685-k`, etc. | Se guarda como `string` |
| NC igual al total | Factura **22**: NC 19.040 = total 19.040 | Se permite el borde y queda `Cancelled` |
| Pago + NC parcial | Facturas **2** y **32** | Los estados se calculan por separado |
| Subtotal de línea erróneo | Factura **41**, línea Lamb: 3.919 × 30 = 117.570, pero declara 254.735 | Se advierte en el detalle y además la factura queda inconsistente |

Resultados de referencia con los datos actuales:

- 50 facturas importadas, **2 inconsistentes** (41 y 48) → **48 activas**.
- Estados de factura: **Issued 43 · Partial 6 · Cancelled 1** (sobre las 50).
- Estados de pago (hoy): **Paid 14 · Overdue 36 · Pending 0** (sobre las 50).
- Sobre las 48 activas: **Paid 13 · Overdue 35 · Pending 0**.
- Reporte de > 30 días sin pago ni NC: **30 facturas**.

> Los conteos de pago dependen de la fecha de ejecución. Con las fechas del archivo
> (todas de 2025), a partir de 2026 no queda ninguna `Pending`.

---

## 9. Decisiones técnicas

- **Montos como `long`:** el archivo usa pesos enteros (CLP). Evita errores de
  redondeo de `double` y comparaciones exactas.
- **`DateOnly`:** evita desplazamientos por zona horaria.
- **`EnsureCreated` + script SQL:** simplifica la ejecución para el evaluador. En un
  proyecto productivo se usarían migraciones de EF Core.
- **Vistas para búsqueda/reportes:** cumple el requisito explícito y centraliza las
  reglas de estado en SQL.
- **Importación al arrancar:** no requiere un paso manual. También existe
  `POST /api/invoices/import` para reejecutarla.
- **Frontend sin librería de UI:** CSS propio, ligero y responsive; menos dependencias
  y más fácil de revisar.

## 10. Posibles mejoras

- Autenticación JWT y roles (el enunciado la deja como opcional).
- Migraciones de EF Core y seed por entorno.
- Concurrencia optimista al agregar NC simultáneas.
- Docker Compose para backend + frontend.
- Exportación de reportes a CSV/PDF.

