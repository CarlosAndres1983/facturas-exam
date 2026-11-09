namespace Facturas.Infrastructure.Import;

internal sealed class InvoiceFileJson
{
    public List<InvoiceJson> Invoices { get; set; } = [];
}

internal sealed class InvoiceJson
{
    public int InvoiceNumber { get; set; }

    public string InvoiceDate { get; set; } = string.Empty;

    public string InvoiceStatus { get; set; } = string.Empty;

    public long TotalAmount { get; set; }

    public int DaysToDue { get; set; }

    public string PaymentDueDate { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public List<InvoiceDetailJson> InvoiceDetail { get; set; } = [];

    public InvoicePaymentJson? InvoicePayment { get; set; }

    public List<CreditNoteJson> InvoiceCreditNote { get; set; } = [];

    public CustomerJson Customer { get; set; } = new();
}

internal sealed class InvoiceDetailJson
{
    public string ProductName { get; set; } = string.Empty;

    public long UnitPrice { get; set; }

    public int Quantity { get; set; }

    public long Subtotal { get; set; }
}

internal sealed class InvoicePaymentJson
{
    public string? PaymentMethod { get; set; }

    public string? PaymentDate { get; set; }
}

internal sealed class CreditNoteJson
{
    public int CreditNoteNumber { get; set; }

    public string CreditNoteDate { get; set; } = string.Empty;

    public long CreditNoteAmount { get; set; }
}

internal sealed class CustomerJson
{
    public string CustomerRun { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;
}
