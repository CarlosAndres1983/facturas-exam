using Facturas.Domain.Enums;
using Facturas.Domain.Exceptions;

namespace Facturas.Domain.Entities;

public class Invoice
{
    public int Id { get; private set; }

    public int InvoiceNumber { get; private set; }

    public DateOnly InvoiceDate { get; private set; }

    public DateOnly PaymentDueDate { get; private set; }

    public long TotalAmount { get; private set; }

    /// <summary>
    /// Una factura es consistente cuando la suma de los subtotales declarados
    /// coincide exactamente con el total declarado. Las inconsistentes se
    /// conservan en la base para reportes, pero quedan fuera del sistema activo.
    /// </summary>
    public bool IsConsistent { get; private set; }

    public int CustomerId { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public InvoicePayment? Payment { get; private set; }

    public ICollection<InvoiceDetail> Details { get; private set; } = new List<InvoiceDetail>();

    public ICollection<CreditNote> CreditNotes { get; private set; } = new List<CreditNote>();

    public long SubtotalSum => Details.Sum(d => d.Subtotal);

    public long CreditNoteSum => CreditNotes.Sum(c => c.CreditNoteAmount);

    /// <summary>
    /// Saldo pendiente de la factura. Nunca es negativo.
    /// </summary>
    public long PendingBalance => Math.Max(0, TotalAmount - CreditNoteSum);

    /// <summary>
    /// Estado calculado: sin NC = Issued, NC igual o mayor al total = Cancelled,
    /// NC menor al total = Partial.
    /// </summary>
    public InvoiceStatus Status =>
        CreditNoteSum == 0
            ? InvoiceStatus.Issued
            : CreditNoteSum >= TotalAmount
                ? InvoiceStatus.Cancelled
                : InvoiceStatus.Partial;

    public bool IsActive => IsConsistent;

    private Invoice()
    {
    }

    public Invoice(
        int invoiceNumber,
        DateOnly invoiceDate,
        DateOnly paymentDueDate,
        long totalAmount,
        Customer customer,
        IEnumerable<InvoiceDetail> details,
        InvoicePayment? payment = null,
        IEnumerable<CreditNote>? creditNotes = null)
    {
        if (invoiceNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(invoiceNumber), "El número de factura debe ser positivo.");
        }

        if (totalAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalAmount), "El total de la factura no puede ser negativo.");
        }

        InvoiceNumber = invoiceNumber;
        InvoiceDate = invoiceDate;
        PaymentDueDate = paymentDueDate;
        TotalAmount = totalAmount;
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        Payment = payment;

        foreach (var detail in details)
        {
            Details.Add(detail);
        }

        if (creditNotes is not null)
        {
            foreach (var creditNote in creditNotes)
            {
                CreditNotes.Add(creditNote);
            }
        }

        RecalculateConsistency();
    }

    public void RecalculateConsistency()
    {
        IsConsistent = SubtotalSum == TotalAmount;
    }

    /// <summary>
    /// Agrega una nota de crédito validando que no supere el saldo pendiente.
    /// Se permite el caso borde de una NC exactamente igual al saldo (deja la
    /// factura en estado Cancelled).
    /// </summary>
    public void AddCreditNote(CreditNote creditNote)
    {
        ArgumentNullException.ThrowIfNull(creditNote);

        if (!IsActive)
        {
            throw new DomainException(
                "La factura es inconsistente y no pertenece al sistema activo; no admite notas de crédito.");
        }

        if (creditNote.CreditNoteAmount > PendingBalance)
        {
            throw new DomainException(
                $"El monto de la nota de crédito ({creditNote.CreditNoteAmount:N0}) supera el saldo pendiente ({PendingBalance:N0}).");
        }

        CreditNotes.Add(creditNote);
    }

    /// <summary>
    /// Estado de pago a una fecha determinada. Se inyecta la fecha para que la
    /// regla sea testeable y no dependa del reloj del servidor.
    /// </summary>
    public PaymentStatus GetPaymentStatus(DateOnly today)
    {
        if (Payment?.PaymentDate is not null)
        {
            return PaymentStatus.Paid;
        }

        return PaymentDueDate < today
            ? PaymentStatus.Overdue
            : PaymentStatus.Pending;
    }

    public int GetDaysOverdue(DateOnly today)
    {
        if (GetPaymentStatus(today) != PaymentStatus.Overdue)
        {
            return 0;
        }

        return today.DayNumber - PaymentDueDate.DayNumber;
    }
}
