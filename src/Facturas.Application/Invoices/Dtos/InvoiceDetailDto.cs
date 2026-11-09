namespace Facturas.Application.Invoices.Dtos;

public sealed record InvoiceDetailDto(
    int InvoiceNumber,
    DateOnly InvoiceDate,
    DateOnly PaymentDueDate,
    long TotalAmount,
    long SubtotalSum,
    long CreditNoteSum,
    long PendingBalance,
    bool IsConsistent,
    string InvoiceStatus,
    string PaymentStatus,
    int DaysOverdue,
    CustomerDto Customer,
    InvoicePaymentDto? Payment,
    IReadOnlyList<InvoiceDetailLineDto> Details,
    IReadOnlyList<CreditNoteDto> CreditNotes);
