using Facturas.Application.Common.Interfaces;
using Facturas.Application.Invoices;
using Facturas.Application.Invoices.Dtos;
using Facturas.Application.Import.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Facturas.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Produces("application/json")]
public sealed class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;
    private readonly IInvoiceImportService _importService;

    public InvoicesController(InvoiceService invoiceService, IInvoiceImportService importService)
    {
        _invoiceService = invoiceService;
        _importService = importService;
    }

    /// <summary>
    /// Busca facturas por número y por estado de factura / estado de pago.
    /// Usa la vista SQL vw_InvoiceSearch. Por defecto excluye las inconsistentes.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InvoiceListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<InvoiceListItemDto>>> Search(
        [FromQuery] int? number,
        [FromQuery] string? invoiceStatus,
        [FromQuery] string? paymentStatus,
        [FromQuery] bool includeInconsistent = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _invoiceService.SearchAsync(
            new InvoiceSearchRequest
            {
                Number = number,
                InvoiceStatus = invoiceStatus,
                PaymentStatus = paymentStatus,
                IncludeInconsistent = includeInconsistent,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle de una factura, incluyendo productos, pago y notas de crédito.
    /// </summary>
    [HttpGet("{invoiceNumber:int}")]
    [ProducesResponseType(typeof(InvoiceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceDetailDto>> GetByNumber(
        int invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceService.GetByNumberAsync(invoiceNumber, cancellationToken);
        return Ok(invoice);
    }

    /// <summary>
    /// Agrega una nota de crédito con fecha automática. Valida que el monto no
    /// supere el saldo pendiente y que la factura pertenezca al sistema activo.
    /// </summary>
    [HttpPost("{invoiceNumber:int}/credit-notes")]
    [ProducesResponseType(typeof(CreditNoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreditNoteDto>> AddCreditNote(
        int invoiceNumber,
        [FromBody] AddCreditNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var created = await _invoiceService.AddCreditNoteAsync(invoiceNumber, request.Amount, cancellationToken);

        return CreatedAtAction(
            nameof(GetByNumber),
            new { invoiceNumber },
            created);
    }

    /// <summary>
    /// Vuelve a ejecutar la importación del archivo JSON. Es idempotente:
    /// las facturas ya existentes se omiten.
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ImportResultDto>> Import(CancellationToken cancellationToken = default)
    {
        var result = await _importService.ImportAsync(cancellationToken);
        return Ok(result);
    }
}
