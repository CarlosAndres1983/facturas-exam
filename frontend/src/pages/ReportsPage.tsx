import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { Badge } from '../components/Badge'
import { EmptyState, ErrorState, Loading } from '../components/States'
import { useAsync } from '../hooks/useAsync'
import { formatCurrency, formatDate, formatNumber } from '../lib/format'

export function ReportsPage() {
  const { data, loading, error, reload } = useAsync(
    () =>
      Promise.all([
        api.getPaymentSummary(),
        api.getOverdueInvoices(),
        api.getInconsistentInvoices(),
      ]),
    [],
  )

  if (loading) {
    return (
      <div className="page">
        <Loading label="Calculando reportes…" />
      </div>
    )
  }

  if (error || !data) {
    return (
      <div className="page">
        <ErrorState message={error ?? 'No se pudieron cargar los reportes'} onRetry={reload} />
      </div>
    )
  }

  const [summary, overdue, inconsistent] = data

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Análisis</p>
          <h1>Reportes</h1>
          <p className="page-subtitle">
            Resumen de pagos, facturas vencidas sin movimiento y control de
            inconsistencias.
          </p>
        </div>
        <div className="header-stat">
          <span>Fecha de corte</span>
          <strong>{formatDate(summary.asOf)}</strong>
        </div>
      </header>

      <section className="report-section">
        <div className="section-heading">
          <div>
            <p className="eyebrow">Reporte 1</p>
            <h2>Resumen por estado de pago</h2>
          </div>
          <span className="as-of">Sobre {formatNumber(summary.totalInvoices)} facturas activas</span>
        </div>

        <div className="summary-grid summary-grid-4">
          <article className="metric metric-highlight">
            <span>Total facturado</span>
            <strong>{formatCurrency(summary.totalAmount)}</strong>
            <small>{formatNumber(summary.totalInvoices)} facturas consistentes</small>
          </article>

          {summary.items.map((item) => (
            <article
              className={`metric summary-${item.paymentStatus.toLowerCase()}`}
              key={item.paymentStatus}
            >
              <span>
                <Badge value={item.paymentStatus} />
              </span>
              <strong>{formatNumber(item.invoiceCount)}</strong>
              <small>{formatCurrency(item.totalAmount)}</small>
              <div className="progress" aria-hidden="true">
                <div className="progress-fill" style={{ width: `${item.percentage}%` }} />
              </div>
              <small className="percentage">{item.percentage.toFixed(2)}%</small>
            </article>
          ))}
        </div>

        <p className="report-note">
          Los porcentajes se calculan sobre las facturas consistentes del sistema
          activo. Las inconsistentes se reportan por separado.
        </p>
      </section>

      <section className="report-section">
        <div className="section-heading">
          <div>
            <p className="eyebrow">Reporte 2</p>
            <h2>Vencidas más de 30 días, sin pago ni nota de crédito</h2>
          </div>
          <span className="as-of">{formatNumber(overdue.length)} factura(s)</span>
        </div>

        {overdue.length === 0 ? (
          <EmptyState
            title="Sin facturas en este reporte"
            description="No hay facturas consistentes con más de 30 días de vencimiento y sin pago ni nota de crédito."
          />
        ) : (
          <div className="table-card">
            <table>
              <thead>
                <tr>
                  <th>N°</th>
                  <th>Cliente</th>
                  <th>Vencimiento</th>
                  <th className="text-right">Días vencida</th>
                  <th className="text-right">Total</th>
                  <th className="text-right">Saldo</th>
                  <th>Factura</th>
                </tr>
              </thead>
              <tbody>
                {overdue.map((invoice) => (
                  <tr key={invoice.invoiceNumber}>
                    <td className="strong">
                      <Link to={`/facturas/${invoice.invoiceNumber}`}>
                        #{invoice.invoiceNumber}
                      </Link>
                    </td>
                    <td>{invoice.customerName}</td>
                    <td>{formatDate(invoice.paymentDueDate)}</td>
                    <td className="text-right">
                      <span className="days-overdue">{formatNumber(invoice.daysOverdue)}</span>
                    </td>
                    <td className="text-right">{formatCurrency(invoice.totalAmount)}</td>
                    <td className="text-right strong">
                      {formatCurrency(invoice.pendingBalance)}
                    </td>
                    <td>
                      <Badge value={invoice.invoiceStatus} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="report-section">
        <div className="section-heading">
          <div>
            <p className="eyebrow">Reporte 3</p>
            <h2>Facturas inconsistentes</h2>
          </div>
          <span className="as-of">{formatNumber(inconsistent.length)} factura(s)</span>
        </div>

        {inconsistent.length === 0 ? (
          <EmptyState
            title="Sin inconsistencias"
            description="Todas las facturas cuadran con la suma de sus subtotales."
          />
        ) : (
          <>
            <div className="alert alert-warning">
              <strong>Total declarado ≠ suma de subtotales.</strong>
              <p>
                Estas facturas se mantienen en la base para auditoría, pero están
                excluidas del sistema activo.
              </p>
            </div>

            <div className="table-card">
              <table>
                <thead>
                  <tr>
                    <th>N°</th>
                    <th>Cliente</th>
                    <th>Emisión</th>
                    <th className="text-right">Total declarado</th>
                    <th className="text-right">Suma subtotales</th>
                    <th className="text-right">Diferencia</th>
                    <th>Pago</th>
                  </tr>
                </thead>
                <tbody>
                  {inconsistent.map((invoice) => (
                    <tr key={invoice.invoiceNumber}>
                      <td className="strong">
                        <Link to={`/facturas/${invoice.invoiceNumber}`}>
                          #{invoice.invoiceNumber}
                        </Link>
                      </td>
                      <td>{invoice.customerName}</td>
                      <td>{formatDate(invoice.invoiceDate)}</td>
                      <td className="text-right">{formatCurrency(invoice.totalAmount)}</td>
                      <td className="text-right">{formatCurrency(invoice.subtotalSum)}</td>
                      <td className="text-right difference">
                        {formatCurrency(invoice.difference)}
                      </td>
                      <td>
                        <Badge value={invoice.paymentStatus} />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </>
        )}
      </section>
    </div>
  )
}
