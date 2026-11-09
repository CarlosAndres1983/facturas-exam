namespace Facturas.Infrastructure.Persistence.ReadModels;

public class InconsistentInvoiceView
{
    public int InvoiceNumber { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateOnly InvoiceDate { get; set; }

    public long TotalAmount { get; set; }

    public long SubtotalSum { get; set; }

    public long Difference { get; set; }

    public string PaymentStatus { get; set; } = string.Empty;
}
