using Facturas.Application.Reports.Dtos;

namespace Facturas.Application.Common.Interfaces;

public interface IReportRepository
{
    Task<PaymentSummaryDto> GetPaymentSummaryAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OverdueInvoiceDto>> GetOverdueWithoutPaymentOrCreditAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InconsistentInvoiceDto>> GetInconsistentInvoicesAsync(
        CancellationToken cancellationToken = default);
}
