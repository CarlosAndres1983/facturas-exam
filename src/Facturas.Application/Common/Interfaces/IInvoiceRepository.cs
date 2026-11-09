using Facturas.Application.Invoices.Dtos;
using Facturas.Domain.Entities;

namespace Facturas.Application.Common.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByNumberAsync(int invoiceNumber, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int invoiceNumber, CancellationToken cancellationToken = default);

    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);

    Task<PagedResult<InvoiceListItemDto>> SearchAsync(
        InvoiceSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<int> GetNextCreditNoteNumberAsync(CancellationToken cancellationToken = default);
}
