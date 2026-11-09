using Facturas.Application.Common.Exceptions;
using Facturas.Application.Common.Interfaces;
using Facturas.Application.Invoices.Dtos;
using Facturas.Domain.Entities;

namespace Facturas.Application.Invoices;

public sealed class InvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public Task<PagedResult<InvoiceListItemDto>> SearchAsync(
        InvoiceSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = request with
        {
            InvoiceStatus = NormalizeInvoiceStatus(request.InvoiceStatus),
            PaymentStatus = NormalizePaymentStatus(request.PaymentStatus),
            Page = request.Page < 1 ? 1 : request.Page,
            PageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize
        };

        return _invoiceRepository.SearchAsync(normalized, cancellationToken);
    }

    public async Task<InvoiceDetailDto> GetByNumberAsync(
        int invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByNumberAsync(invoiceNumber, cancellationToken)
            ?? throw new NotFoundException($"No existe la factura {invoiceNumber}.");

        return InvoiceMapper.ToDetailDto(invoice, _clock.Today);
    }

    public async Task<CreditNoteDto> AddCreditNoteAsync(
        int invoiceNumber,
        long amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new ValidationException("El monto de la nota de crédito debe ser mayor que cero.");
        }

        var invoice = await _invoiceRepository.GetByNumberAsync(invoiceNumber, cancellationToken)
            ?? throw new NotFoundException($"No existe la factura {invoiceNumber}.");

        if (!invoice.IsActive)
        {
            throw new ConflictException(
                "La factura es inconsistente y está excluida del sistema activo; no admite notas de crédito.");
        }

        if (amount > invoice.PendingBalance)
        {
            throw new ConflictException(
                $"El monto informado ({amount:N0}) supera el saldo pendiente de la factura ({invoice.PendingBalance:N0}).");
        }

        var nextNumber = await _invoiceRepository.GetNextCreditNoteNumberAsync(cancellationToken);
        var creditNote = new CreditNote(nextNumber, _clock.Today, amount);

        invoice.AddCreditNote(creditNote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InvoiceMapper.ToCreditNoteDto(creditNote);
    }

    private static string? NormalizeInvoiceStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "issued" => "Issued",
            "partial" => "Partial",
            "cancelled" or "canceled" => "Cancelled",
            _ => throw new ValidationException("Estado de factura inválido. Use Issued, Partial o Cancelled.")
        };
    }

    private static string? NormalizePaymentStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "pending" => "Pending",
            "overdue" => "Overdue",
            "paid" => "Paid",
            _ => throw new ValidationException("Estado de pago inválido. Use Pending, Overdue o Paid.")
        };
    }
}
