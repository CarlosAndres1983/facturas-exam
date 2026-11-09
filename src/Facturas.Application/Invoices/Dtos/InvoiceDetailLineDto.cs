namespace Facturas.Application.Invoices.Dtos;

public sealed record InvoiceDetailLineDto(
    string ProductName,
    long UnitPrice,
    int Quantity,
    long Subtotal,
    long CalculatedSubtotal,
    bool IsLineConsistent);
