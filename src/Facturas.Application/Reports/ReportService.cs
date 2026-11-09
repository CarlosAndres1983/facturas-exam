using Facturas.Application.Common.Interfaces;
using Facturas.Application.Reports.Dtos;

namespace Facturas.Application.Reports;

public sealed class ReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public Task<PaymentSummaryDto> GetPaymentSummaryAsync(CancellationToken cancellationToken = default)
        => _reportRepository.GetPaymentSummaryAsync(cancellationToken);

    public Task<IReadOnlyList<OverdueInvoiceDto>> GetOverdueWithoutPaymentOrCreditAsync(
        CancellationToken cancellationToken = default)
        => _reportRepository.GetOverdueWithoutPaymentOrCreditAsync(cancellationToken);

    public Task<IReadOnlyList<InconsistentInvoiceDto>> GetInconsistentInvoicesAsync(
        CancellationToken cancellationToken = default)
        => _reportRepository.GetInconsistentInvoicesAsync(cancellationToken);
}
