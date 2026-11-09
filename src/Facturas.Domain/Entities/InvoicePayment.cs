namespace Facturas.Domain.Entities;

public class InvoicePayment
{
    public int Id { get; private set; }

    public int InvoiceId { get; private set; }

    public string? PaymentMethod { get; private set; }

    public DateOnly? PaymentDate { get; private set; }

    public bool IsPaid => PaymentDate.HasValue;

    private InvoicePayment()
    {
    }

    public InvoicePayment(string? paymentMethod, DateOnly? paymentDate)
    {
        PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? null : paymentMethod.Trim();
        PaymentDate = paymentDate;
    }
}
