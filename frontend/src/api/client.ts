import type {
  CreditNote,
  InconsistentInvoice,
  InvoiceDetail,
  InvoiceListItem,
  OverdueInvoice,
  PagedResult,
  PaymentSummary,
  ProblemDetails,
} from './types'

const BASE_URL = import.meta.env.VITE_API_URL ?? ''

export class ApiError extends Error {
  readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
  })

  if (!response.ok) {
    let problem: ProblemDetails | null = null
    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      problem = null
    }

    throw new ApiError(
      problem?.detail ?? problem?.title ?? `Error ${response.status}`,
      response.status,
    )
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export interface InvoiceFilters {
  number?: string
  invoiceStatus?: string
  paymentStatus?: string
  includeInconsistent?: boolean
  page?: number
  pageSize?: number
}

export const api = {
  searchInvoices(filters: InvoiceFilters) {
    const params = new URLSearchParams()

    if (filters.number?.trim()) params.set('number', filters.number.trim())
    if (filters.invoiceStatus) params.set('invoiceStatus', filters.invoiceStatus)
    if (filters.paymentStatus) params.set('paymentStatus', filters.paymentStatus)
    if (filters.includeInconsistent) params.set('includeInconsistent', 'true')
    params.set('page', String(filters.page ?? 1))
    params.set('pageSize', String(filters.pageSize ?? 10))

    return request<PagedResult<InvoiceListItem>>(`/api/invoices?${params.toString()}`)
  },

  getInvoice(invoiceNumber: number) {
    return request<InvoiceDetail>(`/api/invoices/${invoiceNumber}`)
  },

  addCreditNote(invoiceNumber: number, amount: number) {
    return request<CreditNote>(`/api/invoices/${invoiceNumber}/credit-notes`, {
      method: 'POST',
      body: JSON.stringify({ amount }),
    })
  },

  getPaymentSummary() {
    return request<PaymentSummary>('/api/reports/payment-summary')
  },

  getOverdueInvoices() {
    return request<OverdueInvoice[]>('/api/reports/overdue-without-payment-or-credit')
  },

  getInconsistentInvoices() {
    return request<InconsistentInvoice[]>('/api/reports/inconsistent-invoices')
  },
}
