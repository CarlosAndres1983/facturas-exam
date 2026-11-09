namespace Facturas.Domain.Entities;

public class CreditNote
{
    public int Id { get; private set; }

    public int InvoiceId { get; private set; }

    public int CreditNoteNumber { get; private set; }

    public DateOnly CreditNoteDate { get; private set; }

    public long CreditNoteAmount { get; private set; }

    private CreditNote()
    {
    }

    public CreditNote(int creditNoteNumber, DateOnly creditNoteDate, long creditNoteAmount)
    {
        if (creditNoteNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(creditNoteNumber), "El número de nota de crédito debe ser positivo.");
        }

        if (creditNoteAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(creditNoteAmount), "El monto de la nota de crédito debe ser mayor que cero.");
        }

        CreditNoteNumber = creditNoteNumber;
        CreditNoteDate = creditNoteDate;
        CreditNoteAmount = creditNoteAmount;
    }
}
