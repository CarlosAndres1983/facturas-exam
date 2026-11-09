namespace Facturas.Domain.Enums;

/// <summary>
/// Estado de pago calculado a partir de la fecha de pago y del vencimiento.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Overdue = 1,
    Paid = 2
}
