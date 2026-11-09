using Facturas.Application.Reports;
using Facturas.Application.Reports.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Facturas.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Produces("application/json")]
public sealed class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Resumen de facturas consistentes por estado de pago, con totales y porcentajes.
    /// </summary>
    [HttpGet("payment-summary")]
    [ProducesResponseType(typeof(PaymentSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentSummaryDto>> GetPaymentSummary(CancellationToken cancellationToken = default)
    {
        var summary = await _reportService.GetPaymentSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Facturas consistentes vencidas hace más de 30 días, sin pago y sin notas de crédito.
    /// </summary>
    [HttpGet("overdue-without-payment-or-credit")]
    [ProducesResponseType(typeof(IReadOnlyList<OverdueInvoiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OverdueInvoiceDto>>> GetOverdueWithoutPaymentOrCredit(
        CancellationToken cancellationToken = default)
    {
        var invoices = await _reportService.GetOverdueWithoutPaymentOrCreditAsync(cancellationToken);
        return Ok(invoices);
    }

    /// <summary>
    /// Facturas inconsistentes: el total declarado no coincide con la suma de subtotales.
    /// </summary>
    [HttpGet("inconsistent-invoices")]
    [ProducesResponseType(typeof(IReadOnlyList<InconsistentInvoiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InconsistentInvoiceDto>>> GetInconsistentInvoices(
        CancellationToken cancellationToken = default)
    {
        var invoices = await _reportService.GetInconsistentInvoicesAsync(cancellationToken);
        return Ok(invoices);
    }
}
