namespace Facturas.Domain.Entities;

public class InvoiceDetail
{
    public int Id { get; private set; }

    public int InvoiceId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public long UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    /// <summary>
    /// Subtotal declarado en el archivo de origen. Es el valor que exige el
    /// enunciado para validar la coherencia contra el total de la factura.
    /// </summary>
    public long Subtotal { get; private set; }

    /// <summary>
    /// Subtotal recalculado (precio unitario * cantidad). Sirve para detectar
    /// errores internos de una línea, como ocurre en la factura 41.
    /// </summary>
    public long CalculatedSubtotal => UnitPrice * Quantity;

    public bool IsLineConsistent => CalculatedSubtotal == Subtotal;

    private InvoiceDetail()
    {
    }

    public InvoiceDetail(string productName, long unitPrice, int quantity, long subtotal)
    {
        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "El precio unitario no puede ser negativo.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "La cantidad debe ser mayor que cero.");
        }

        if (subtotal < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(subtotal), "El subtotal no puede ser negativo.");
        }

        ProductName = productName?.Trim() ?? string.Empty;
        UnitPrice = unitPrice;
        Quantity = quantity;
        Subtotal = subtotal;
    }
}
