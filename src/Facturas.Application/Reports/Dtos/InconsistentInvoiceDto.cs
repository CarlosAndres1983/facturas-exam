namespace Facturas.Application.Reports.Dtos;

public sealed record InconsistentInvoiceDto(
    int InvoiceNumber,
    string CustomerName,
    DateOnly InvoiceDate,
    long TotalAmount,
    long SubtotalSum,
    long Difference,
    string PaymentStatus);
