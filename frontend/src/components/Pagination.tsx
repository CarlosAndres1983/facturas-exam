import { formatNumber } from '../lib/format'

interface PaginationProps {
  page: number
  totalPages: number
  totalCount: number
  onPageChange: (page: number) => void
}

export function Pagination({ page, totalPages, totalCount, onPageChange }: PaginationProps) {
  if (totalPages <= 1) {
    return null
  }

  return (
    <div className="pagination">
      <span>
        Página {formatNumber(page)} de {formatNumber(totalPages)} ·{' '}
        {formatNumber(totalCount)} registros
      </span>

      <div className="pagination-actions">
        <button
          type="button"
          className="button button-secondary"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          Anterior
        </button>
        <button
          type="button"
          className="button button-secondary"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Siguiente
        </button>
      </div>
    </div>
  )
}
