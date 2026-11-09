namespace Facturas.Application.Invoices.Dtos;

public sealed record InvoicePaymentDto(
    string? PaymentMethod,
    DateOnly? PaymentDate);
