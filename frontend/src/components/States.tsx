interface ErrorStateProps {
  message: string
  onRetry?: () => void
}

export function Loading({ label = 'Cargando información…' }: { label?: string }) {
  return (
    <div className="state">
      <span className="spinner" aria-hidden="true" />
      <p>{label}</p>
    </div>
  )
}

export function ErrorState({ message, onRetry }: ErrorStateProps) {
  return (
    <div className="state state-error">
      <strong>No se pudo cargar la información</strong>
      <p>{message}</p>
      {onRetry && (
        <button type="button" className="button button-secondary" onClick={onRetry}>
          Reintentar
        </button>
      )}
    </div>
  )
}

export function EmptyState({
  title = 'Sin resultados',
  description = 'Ajusta los filtros e inténtalo nuevamente.',
}: {
  title?: string
  description?: string
}) {
  return (
    <div className="state state-empty">
      <strong>{title}</strong>
      <p>{description}</p>
    </div>
  )
}
