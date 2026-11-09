namespace Facturas.Infrastructure.Persistence.ReadModels;

public class PaymentSummaryView
{
    public string PaymentStatus { get; set; } = string.Empty;

    public int InvoiceCount { get; set; }

    public long TotalAmount { get; set; }
}
