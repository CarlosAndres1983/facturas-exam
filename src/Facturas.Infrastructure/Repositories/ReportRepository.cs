using Facturas.Application.Common.Interfaces;
using Facturas.Application.Reports.Dtos;
using Facturas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Facturas.Infrastructure.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private static readonly string[] PaymentStatusOrder = ["Paid", "Pending", "Overdue"];

    private readonly AppDbContext _dbContext;
    private readonly IClock _clock;

    public ReportRepository(AppDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<PaymentSummaryDto> GetPaymentSummaryAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.PaymentSummary
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalInvoices = rows.Sum(x => x.InvoiceCount);
        var totalAmount = rows.Sum(x => x.TotalAmount);

        var items = PaymentStatusOrder
            .Select(status =>
            {
                var row = rows.FirstOrDefault(x => x.PaymentStatus == status);
                var count = row?.InvoiceCount ?? 0;
                var amount = row?.TotalAmount ?? 0;
                var percentage = totalInvoices == 0
                    ? 0m
                    : Math.Round(count * 100m / totalInvoices, 2);

                return new PaymentSummaryItemDto(status, count, amount, percentage);
            })
            .ToList();

        return new PaymentSummaryDto(_clock.Today, totalInvoices, totalAmount, items);
    }

    public async Task<IReadOnlyList<OverdueInvoiceDto>> GetOverdueWithoutPaymentOrCreditAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.OverdueInvoices
            .AsNoTracking()
            .OrderByDescending(x => x.DaysOverdue)
            .ThenBy(x => x.InvoiceNumber)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new OverdueInvoiceDto(
                x.InvoiceNumber,
                x.CustomerName,
                x.PaymentDueDate,
                x.DaysOverdue,
                x.TotalAmount,
                x.PendingBalance,
                x.InvoiceStatus))
            .ToList();
    }

    public async Task<IReadOnlyList<InconsistentInvoiceDto>> GetInconsistentInvoicesAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.InconsistentInvoices
            .AsNoTracking()
            .OrderBy(x => x.InvoiceNumber)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new InconsistentInvoiceDto(
                x.InvoiceNumber,
                x.CustomerName,
                x.InvoiceDate,
                x.TotalAmount,
                x.SubtotalSum,
                x.Difference,
                x.PaymentStatus))
            .ToList();
    }
}
