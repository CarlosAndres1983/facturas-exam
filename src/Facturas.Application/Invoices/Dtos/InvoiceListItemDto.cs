namespace Facturas.Application.Invoices.Dtos;

public sealed record InvoiceListItemDto(
    int InvoiceNumber,
    DateOnly InvoiceDate,
    DateOnly PaymentDueDate,
    long TotalAmount,
    long CreditNoteSum,
    long PendingBalance,
    bool IsConsistent,
    string InvoiceStatus,
    string PaymentStatus,
    string CustomerName,
    string CustomerRun);
