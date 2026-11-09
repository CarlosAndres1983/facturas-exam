namespace Facturas.Application.Reports.Dtos;

public sealed record PaymentSummaryItemDto(
    string PaymentStatus,
    int InvoiceCount,
    long TotalAmount,
    decimal Percentage);
