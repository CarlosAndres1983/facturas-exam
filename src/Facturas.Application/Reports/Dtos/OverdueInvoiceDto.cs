namespace Facturas.Application.Reports.Dtos;

public sealed record OverdueInvoiceDto(
    int InvoiceNumber,
    string CustomerName,
    DateOnly PaymentDueDate,
    int DaysOverdue,
    long TotalAmount,
    long PendingBalance,
    string InvoiceStatus);
