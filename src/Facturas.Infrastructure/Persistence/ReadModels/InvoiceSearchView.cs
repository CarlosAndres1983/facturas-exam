namespace Facturas.Infrastructure.Persistence.ReadModels;

/// <summary>
/// Read model mapeado a la vista SQL vw_InvoiceSearch.
/// Los estados se calculan en la vista para que la búsqueda y los reportes
/// usen exactamente las mismas reglas.
/// </summary>
public class InvoiceSearchView
{
    public int InvoiceNumber { get; set; }

    public DateOnly InvoiceDate { get; set; }

    public DateOnly PaymentDueDate { get; set; }

    public long TotalAmount { get; set; }

    public long SubtotalSum { get; set; }

    public long CreditNoteSum { get; set; }

    public long PendingBalance { get; set; }

    public bool IsConsistent { get; set; }

    public string InvoiceStatus { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public string CustomerRun { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public string? PaymentMethod { get; set; }

    public DateOnly? PaymentDate { get; set; }
}
