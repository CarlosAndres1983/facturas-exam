namespace Facturas.Application.Reports.Dtos;

public sealed record PaymentSummaryDto(
    DateOnly AsOf,
    int TotalInvoices,
    long TotalAmount,
    IReadOnlyList<PaymentSummaryItemDto> Items);
