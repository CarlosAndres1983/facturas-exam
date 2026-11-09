namespace Facturas.Infrastructure.Persistence.ReadModels;

public class OverdueInvoiceView
{
    public int InvoiceNumber { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateOnly PaymentDueDate { get; set; }

    public int DaysOverdue { get; set; }

    public long TotalAmount { get; set; }

    public long PendingBalance { get; set; }

    public string InvoiceStatus { get; set; } = string.Empty;
}
