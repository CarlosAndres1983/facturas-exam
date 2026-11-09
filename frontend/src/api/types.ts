export type InvoiceStatus = 'Issued' | 'Partial' | 'Cancelled'
export type PaymentStatus = 'Pending' | 'Overdue' | 'Paid'

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface InvoiceListItem {
  invoiceNumber: number
  invoiceDate: string
  paymentDueDate: string
  totalAmount: number
  creditNoteSum: number
  pendingBalance: number
  isConsistent: boolean
  invoiceStatus: InvoiceStatus
  paymentStatus: PaymentStatus
  customerName: string
  customerRun: string
}

export interface Customer {
  run: string
  name: string
  email: string
}

export interface InvoicePayment {
  paymentMethod: string | null
  paymentDate: string | null
}

export interface InvoiceDetailLine {
  productName: string
  unitPrice: number
  quantity: number
  subtotal: number
  calculatedSubtotal: number
  isLineConsistent: boolean
}

export interface CreditNote {
  creditNoteNumber: number
  creditNoteDate: string
  creditNoteAmount: number
}

export interface InvoiceDetail {
  invoiceNumber: number
  invoiceDate: string
  paymentDueDate: string
  totalAmount: number
  subtotalSum: number
  creditNoteSum: number
  pendingBalance: number
  isConsistent: boolean
  invoiceStatus: InvoiceStatus
  paymentStatus: PaymentStatus
  daysOverdue: number
  customer: Customer
  payment: InvoicePayment | null
  details: InvoiceDetailLine[]
  creditNotes: CreditNote[]
}

export interface PaymentSummaryItem {
  paymentStatus: PaymentStatus
  invoiceCount: number
  totalAmount: number
  percentage: number
}

export interface PaymentSummary {
  asOf: string
  totalInvoices: number
  totalAmount: number
  items: PaymentSummaryItem[]
}

export interface OverdueInvoice {
  invoiceNumber: number
  customerName: string
  paymentDueDate: string
  daysOverdue: number
  totalAmount: number
  pendingBalance: number
  invoiceStatus: InvoiceStatus
}

export interface InconsistentInvoice {
  invoiceNumber: number
  customerName: string
  invoiceDate: string
  totalAmount: number
  subtotalSum: number
  difference: number
  paymentStatus: PaymentStatus
}

export interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
}
