namespace Facturas.Domain.Enums;

/// <summary>
/// Estado de la factura calculado a partir de sus notas de crédito.
/// No se toma el valor del JSON de origen porque ese dato no es confiable.
/// </summary>
public enum InvoiceStatus
{
    Issued = 0,
    Partial = 1,
    Cancelled = 2
}
