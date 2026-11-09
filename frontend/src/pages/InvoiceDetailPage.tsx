import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { Badge } from '../components/Badge'
import { ErrorState, Loading } from '../components/States'
import { useAsync } from '../hooks/useAsync'
import { formatCurrency, formatDate } from '../lib/format'

export function InvoiceDetailPage() {
  const params = useParams<{ invoiceNumber: string }>()
  const invoiceNumber = Number(params.invoiceNumber)

  const { data, loading, error, reload } = useAsync(
    () => api.getInvoice(invoiceNumber),
    [invoiceNumber],
  )

  const [amount, setAmount] = useState('')
  const [formError, setFormError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setFormError(null)
    setSuccess(null)

    const parsed = Number(amount)

    if (!amount.trim()) {
      setFormError('Ingresa un monto para la nota de crédito.')
      return
    }

    if (!Number.isInteger(parsed) || parsed <= 0) {
      setFormError('El monto debe ser un entero mayor que cero.')
      return
    }

    if (data && parsed > data.pendingBalance) {
      setFormError(
        `El monto no puede superar el saldo pendiente (${formatCurrency(data.pendingBalance)}).`,
      )
      return
    }

    setSubmitting(true)

    try {
      await api.addCreditNote(invoiceNumber, parsed)
      setAmount('')
      setSuccess('Nota de crédito registrada correctamente.')
      reload()
    } catch (requestError) {
      setFormError(
        requestError instanceof Error ? requestError.message : 'Error inesperado',
      )
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <div className="page">
        <Loading />
      </div>
    )
  }

  if (error || !data) {
    return (
      <div className="page">
        <Link className="back-link" to="/facturas">
          ← Volver a facturas
        </Link>
        <ErrorState message={error ?? 'Factura no encontrada'} onRetry={reload} />
      </div>
    )
  }

  const canAddCreditNote = data.isConsistent && data.pendingBalance > 0

  return (
    <div className="page">
      <Link className="back-link" to="/facturas">
        ← Volver a facturas
      </Link>

      <header className="page-header detail-header">
        <div>
          <p className="eyebrow">Factura</p>
          <h1>#{data.invoiceNumber}</h1>
          <div className="badge-row">
            <Badge value={data.invoiceStatus} />
            <Badge value={data.paymentStatus} />
            {!data.isConsistent && <span className="badge badge-cancelled">Inconsistente</span>}
          </div>
        </div>

        <div className="customer-card">
          <span>Cliente</span>
          <strong>{data.customer.name}</strong>
          <small>
            {data.customer.run} · {data.customer.email}
          </small>
        </div>
      </header>

      {!data.isConsistent && (
        <div className="alert alert-warning">
          <strong>Factura excluida del sistema activo.</strong>
          <p>
            La suma de subtotales ({formatCurrency(data.subtotalSum)}) no coincide
            con el total declarado ({formatCurrency(data.totalAmount)}). Se conserva
            en la base para el reporte de inconsistencias.
          </p>
        </div>
      )}

      <section className="summary-grid">
        <article className="metric">
          <span>Total factura</span>
          <strong>{formatCurrency(data.totalAmount)}</strong>
          <small>Emitida el {formatDate(data.invoiceDate)}</small>
        </article>

        <article className="metric">
          <span>Notas de crédito</span>
          <strong>{formatCurrency(data.creditNoteSum)}</strong>
          <small>{data.creditNotes.length} nota(s) registrada(s)</small>
        </article>

        <article className="metric metric-highlight">
          <span>Saldo pendiente</span>
          <strong>{formatCurrency(data.pendingBalance)}</strong>
          <small>Vence el {formatDate(data.paymentDueDate)}</small>
        </article>

        <article className="metric">
          <span>Estado de pago</span>
          <strong className="metric-status">
            <Badge value={data.paymentStatus} />
          </strong>
          <small>
            {data.payment?.paymentDate
              ? `Pagada el ${formatDate(data.payment.paymentDate)}`
              : data.daysOverdue > 0
                ? `${data.daysOverdue} días vencida`
                : 'Dentro del plazo'}
          </small>
        </article>
      </section>

      <div className="content-grid">
        <section className="panel">
          <div className="panel-header">
            <h2>Detalle de productos</h2>
            <span className="panel-meta">{data.details.length} línea(s)</span>
          </div>

          <div className="table-card table-card-flat">
            <table>
              <thead>
                <tr>
                  <th>Producto</th>
                  <th className="text-right">Precio unitario</th>
                  <th className="text-right">Cantidad</th>
                  <th className="text-right">Subtotal</th>
                </tr>
              </thead>
              <tbody>
                {data.details.map((line, index) => (
                  <tr key={`${line.productName}-${index}`}>
                    <td>
                      <div className="cell-main">{line.productName}</div>
                      {!line.isLineConsistent && (
                        <div className="cell-warning">
                          Subtotal declarado distinto de precio × cantidad (
                          {formatCurrency(line.calculatedSubtotal)})
                        </div>
                      )}
                    </td>
                    <td className="text-right">{formatCurrency(line.unitPrice)}</td>
                    <td className="text-right">{line.quantity}</td>
                    <td className="text-right strong">{formatCurrency(line.subtotal)}</td>
                  </tr>
                ))}
              </tbody>
              <tfoot>
                <tr>
                  <td colSpan={3}>Suma de subtotales</td>
                  <td className="text-right strong">{formatCurrency(data.subtotalSum)}</td>
                </tr>
              </tfoot>
            </table>
          </div>
        </section>

        <div className="stack">
          <section className="panel">
            <div className="panel-header">
              <h2>Pago</h2>
            </div>

            {data.payment?.paymentDate ? (
              <dl className="definition-list">
                <div>
                  <dt>Método</dt>
                  <dd>{data.payment.paymentMethod ?? 'No informado'}</dd>
                </div>
                <div>
                  <dt>Fecha</dt>
                  <dd>{formatDate(data.payment.paymentDate)}</dd>
                </div>
              </dl>
            ) : (
              <p className="muted">Sin pago registrado.</p>
            )}
          </section>

          <section className="panel">
            <div className="panel-header">
              <h2>Notas de crédito</h2>
              <span className="panel-meta">{data.creditNotes.length}</span>
            </div>

            {data.creditNotes.length === 0 ? (
              <p className="muted">La factura no tiene notas de crédito.</p>
            ) : (
              <ul className="note-list">
                {data.creditNotes.map((note) => (
                  <li key={note.creditNoteNumber}>
                    <div>
                      <strong>#{note.creditNoteNumber}</strong>
                      <small>{formatDate(note.creditNoteDate)}</small>
                    </div>
                    <span>{formatCurrency(note.creditNoteAmount)}</span>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section className="panel">
            <div className="panel-header">
              <h2>Agregar nota de crédito</h2>
            </div>

            {!data.isConsistent ? (
              <p className="muted">
                No se pueden agregar notas de crédito a una factura inconsistente.
              </p>
            ) : data.pendingBalance <= 0 ? (
              <p className="muted">La factura no tiene saldo pendiente.</p>
            ) : (
              <form className="credit-form" onSubmit={handleSubmit}>
                <label className="field">
                  <span>Monto</span>
                  <input
                    type="number"
                    min="1"
                    step="1"
                    placeholder={`Máximo ${formatCurrency(data.pendingBalance)}`}
                    value={amount}
                    onChange={(event) => setAmount(event.target.value)}
                    disabled={submitting}
                  />
                </label>

                <p className="form-hint">
                  La fecha de creación se asigna automáticamente. El monto no puede
                  superar el saldo pendiente.
                </p>

                {formError && <p className="form-error">{formError}</p>}
                {success && <p className="form-success">{success}</p>}

                <button
                  type="submit"
                  className="button button-primary button-block"
                  disabled={submitting || !canAddCreditNote}
                >
                  {submitting ? 'Registrando…' : 'Registrar nota de crédito'}
                </button>
              </form>
            )}
          </section>
        </div>
      </div>
    </div>
  )
}
