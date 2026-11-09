# Frontend — Gestión de facturas

Interfaz React + TypeScript + Vite para la API de facturas.

## Requisitos

- Node.js 20 o superior.
- El backend debe estar disponible en `http://localhost:5000`.

## Ejecución

```bash
npm install
npm run dev
```

Abre `http://localhost:5173`. Vite redirige `/api` al backend
(`vite.config.ts`), por lo que no se necesita configurar CORS en desarrollo.

Para apuntar a otra URL de API:

```bash
# .env.local
VITE_API_URL=http://localhost:5000
```

## Build de producción

```bash
npm run build
npm run preview
```

El resultado queda en `dist/`. Para servirlo con la API, publica esa carpeta
en cualquier servidor estático y configura la URL de la API con
`VITE_API_URL`.

## Estructura

```
src/
├── api/          # Cliente HTTP y tipos compartidos
├── components/   # Layout, badges, paginación y estados de carga
├── hooks/        # useAsync
├── lib/          # Formato de moneda y fechas
└── pages/        # Listado, detalle y reportes
```

La documentación completa del sistema está en el `README.md` de la raíz.
