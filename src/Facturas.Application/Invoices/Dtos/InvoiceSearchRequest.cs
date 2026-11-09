namespace Facturas.Application.Invoices.Dtos;

public sealed record InvoiceSearchRequest
{
    public int? Number { get; init; }

    /// <summary>Issued | Partial | Cancelled</summary>
    public string? InvoiceStatus { get; init; }

    /// <summary>Pending | Overdue | Paid</summary>
    public string? PaymentStatus { get; init; }

    /// <summary>
    /// Por defecto el sistema activo excluye las facturas inconsistentes.
    /// El reporte de inconsistencias las consulta aparte.
    /// </summary>
    public bool IncludeInconsistent { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
