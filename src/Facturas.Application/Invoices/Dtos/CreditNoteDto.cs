namespace Facturas.Application.Invoices.Dtos;

public sealed record CreditNoteDto(
    int CreditNoteNumber,
    DateOnly CreditNoteDate,
    long CreditNoteAmount);
