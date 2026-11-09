using Facturas.Application.Common.Interfaces;
using Facturas.Application.Invoices.Dtos;
using Facturas.Domain.Entities;
using Facturas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Facturas.Infrastructure.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _dbContext;

    public InvoiceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Invoice?> GetByNumberAsync(
        int invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Invoices
            .Include(x => x.Customer)
            .Include(x => x.Details)
            .Include(x => x.CreditNotes)
            .Include(x => x.Payment)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public Task<bool> ExistsAsync(int invoiceNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.Invoices.AnyAsync(x => x.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _dbContext.Invoices.Add(invoice);
        return Task.CompletedTask;
    }

    public async Task<PagedResult<InvoiceListItemDto>> SearchAsync(
        InvoiceSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.InvoiceSearch.AsNoTracking();

        if (!request.IncludeInconsistent)
        {
            query = query.Where(x => x.IsConsistent);
        }

        if (request.Number.HasValue)
        {
            query = query.Where(x => x.InvoiceNumber == request.Number.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.InvoiceStatus))
        {
            query = query.Where(x => x.InvoiceStatus == request.InvoiceStatus);
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
        {
            query = query.Where(x => x.PaymentStatus == request.PaymentStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.InvoiceNumber)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new InvoiceListItemDto(
                x.InvoiceNumber,
                x.InvoiceDate,
                x.PaymentDueDate,
                x.TotalAmount,
                x.CreditNoteSum,
                x.PendingBalance,
                x.IsConsistent,
                x.InvoiceStatus,
                x.PaymentStatus,
                x.CustomerName,
                x.CustomerRun))
            .ToListAsync(cancellationToken);

        return new PagedResult<InvoiceListItemDto>(items, request.Page, request.PageSize, totalCount);
    }

    public async Task<int> GetNextCreditNoteNumberAsync(CancellationToken cancellationToken = default)
    {
        var maxNumber = await _dbContext.CreditNotes
            .MaxAsync(x => (int?)x.CreditNoteNumber, cancellationToken);

        return (maxNumber ?? 0) + 1;
    }
}
