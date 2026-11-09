const labels: Record<string, string> = {
  Issued: 'Emitida',
  Partial: 'Parcial',
  Cancelled: 'Anulada',
  Paid: 'Pagada',
  Pending: 'Pendiente',
  Overdue: 'Vencida',
}

export function Badge({ value }: { value: string }) {
  return (
    <span className={`badge badge-${value.toLowerCase()}`} title={value}>
      {labels[value] ?? value}
    </span>
  )
}
