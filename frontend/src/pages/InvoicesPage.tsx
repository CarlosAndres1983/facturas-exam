import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import type { InvoiceFilters } from '../api/client'
import { Badge } from '../components/Badge'
import { Pagination } from '../components/Pagination'
import { EmptyState, ErrorState, Loading } from '../components/States'
import { useAsync } from '../hooks/useAsync'
import { formatCurrency, formatDate, formatNumber } from '../lib/format'

const PAGE_SIZE = 10

export function InvoicesPage() {
  const navigate = useNavigate()

  const [number, setNumber] = useState('')
  const [invoiceStatus, setInvoiceStatus] = useState('')
  const [paymentStatus, setPaymentStatus] = useState('')
  const [includeInconsistent, setIncludeInconsistent] = useState(false)
  const [filters, setFilters] = useState<InvoiceFilters>({ page: 1, pageSize: PAGE_SIZE })

  const { data, loading, error, reload } = useAsync(
    () => api.searchInvoices(filters),
    [filters],
  )

  function handleSubmit(event: FormEvent) {
    event.preventDefault()

    setFilters({
      number,
      invoiceStatus,
      paymentStatus,
      includeInconsistent,
      page: 1,
      pageSize: PAGE_SIZE,
    })
  }

  function handleClear() {
    setNumber('')
    setInvoiceStatus('')
    setPaymentStatus('')
    setIncludeInconsistent(false)
    setFilters({ page: 1, pageSize: PAGE_SIZE })
  }

  function handlePageChange(page: number) {
    setFilters((current) => ({ ...current, page }))
  }

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Sistema activo</p>
          <h1>Facturas</h1>
          <p className="page-subtitle">
            Busca por número, estado de factura o estado de pago. Las facturas
            inconsistentes se excluyen por defecto.
          </p>
        </div>

        <div className="header-stat">
          <span>Facturas encontradas</span>
          <strong>{data ? formatNumber(data.totalCount) : '—'}</strong>
        </div>
      </header>

      <form className="filters" onSubmit={handleSubmit}>
        <label className="field">
          <span>Número</span>
          <input
            type="number"
            min="1"
            placeholder="Ej. 41"
            value={number}
            onChange={(event) => setNumber(event.target.value)}
          />
        </label>

        <label className="field">
          <span>Estado factura</span>
          <select
            value={invoiceStatus}
            onChange={(event) => setInvoiceStatus(event.target.value)}
          >
            <option value="">Todos</option>
            <option value="Issued">Emitida</option>
            <option value="Partial">Parcial</option>
            <option value="Cancelled">Anulada</option>
          </select>
        </label>

        <label className="field">
          <span>Estado pago</span>
          <select
            value={paymentStatus}
            onChange={(event) => setPaymentStatus(event.target.value)}
          >
            <option value="">Todos</option>
            <option value="Paid">Pagada</option>
            <option value="Pending">Pendiente</option>
            <option value="Overdue">Vencida</option>
          </select>
        </label>

        <label className="field field-check">
          <input
            type="checkbox"
            checked={includeInconsistent}
            onChange={(event) => setIncludeInconsistent(event.target.checked)}
          />
          <span>Incluir inconsistentes</span>
        </label>

        <div className="filters-actions">
          <button type="submit" className="button button-primary">
            Buscar
          </button>
          <button type="button" className="button button-ghost" onClick={handleClear}>
            Limpiar
          </button>
        </div>
      </form>

      {loading && <Loading />}
      {!loading && error && <ErrorState message={error} onRetry={reload} />}
      {!loading && !error && data && data.items.length === 0 && <EmptyState />}

      {!loading && !error && data && data.items.length > 0 && (
        <>
          <div className="table-card">
            <table>
              <thead>
                <tr>
                  <th>N°</th>
                  <th>Cliente</th>
                  <th>Emisión</th>
                  <th>Vencimiento</th>
                  <th className="text-right">Total</th>
                  <th className="text-right">NC</th>
                  <th className="text-right">Saldo</th>
                  <th>Factura</th>
                  <th>Pago</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((invoice) => (
                  <tr
                    key={invoice.invoiceNumber}
                    className="clickable"
                    tabIndex={0}
                    onClick={() => navigate(`/facturas/${invoice.invoiceNumber}`)}
                    onKeyDown={(event) => {
                      if (event.key === 'Enter') {
                        navigate(`/facturas/${invoice.invoiceNumber}`)
                      }
                    }}
                  >
                    <td className="strong">#{invoice.invoiceNumber}</td>
                    <td>
                      <div className="cell-main">{invoice.customerName}</div>
                      <div className="cell-sub">{invoice.customerRun}</div>
                    </td>
                    <td>{formatDate(invoice.invoiceDate)}</td>
                    <td>{formatDate(invoice.paymentDueDate)}</td>
                    <td className="text-right">{formatCurrency(invoice.totalAmount)}</td>
                    <td className="text-right">
                      {invoice.creditNoteSum ? formatCurrency(invoice.creditNoteSum) : '—'}
                    </td>
                    <td className="text-right strong">
                      {formatCurrency(invoice.pendingBalance)}
                    </td>
                    <td>
                      <Badge value={invoice.invoiceStatus} />
                    </td>
                    <td>
                      <Badge value={invoice.paymentStatus} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            totalCount={data.totalCount}
            onPageChange={handlePageChange}
          />
        </>
      )}
    </div>
  )
}
