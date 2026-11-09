using Facturas.Domain.Entities;
using Facturas.Domain.Enums;
using Facturas.Domain.Exceptions;

namespace Facturas.Tests;

public class InvoiceDomainTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    [Fact]
    public void Invoice_WithMatchingSubtotals_IsConsistent()
    {
        var invoice = CreateInvoice(total: 1_000, subtotals: [400, 600]);

        Assert.True(invoice.IsConsistent);
        Assert.True(invoice.IsActive);
    }

    [Fact]
    public void Invoice_WithSmallDifference_IsInconsistent()
    {
        // Caso real factura 48: 560.418 vs 560.412 (diferencia de solo 6).
        var invoice = CreateInvoice(total: 560_412, subtotals: [441_818, 118_600]);

        Assert.False(invoice.IsConsistent);
        Assert.False(invoice.IsActive);
    }

    [Fact]
    public void Invoice_41_IsInconsistent_UsingDeclaredSubtotals()
    {
        // Caso real factura 41: subtotales declarados 828.336 vs total 691.171.
        var invoice = CreateInvoice(total: 691_171, subtotals: [479_061, 94_540, 254_735]);

        Assert.False(invoice.IsConsistent);
        Assert.Equal(828_336, invoice.SubtotalSum);
    }

    [Fact]
    public void Status_WithoutCreditNotes_IsIssued()
    {
        // Caso real factura 30: el JSON dice "canceled" pero no tiene NC.
        var invoice = CreateInvoice(total: 1_530, subtotals: [1_530]);

        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
    }

    [Fact]
    public void Status_WithPartialCreditNote_IsPartial()
    {
        var invoice = CreateInvoice(total: 1_000, subtotals: [1_000], creditNotes: [400]);

        Assert.Equal(InvoiceStatus.Partial, invoice.Status);
        Assert.Equal(600, invoice.PendingBalance);
    }

    [Fact]
    public void Status_WithCreditNoteEqualToTotal_IsCancelled()
    {
        // Caso real factura 22: NC exactamente igual al total.
        var invoice = CreateInvoice(total: 19_040, subtotals: [19_040], creditNotes: [19_040]);

        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        Assert.Equal(0, invoice.PendingBalance);
    }

    [Fact]
    public void PaymentStatus_WithPaymentDate_IsPaid_EvenWhenInvoiceIsPartial()
    {
        // Caso real factura 2: pagada y con NC parcial al mismo tiempo.
        var invoice = CreateInvoice(
            total: 1_000,
            subtotals: [1_000],
            creditNotes: [400],
            paymentDate: new DateOnly(2025, 3, 5));

        Assert.Equal(PaymentStatus.Paid, invoice.GetPaymentStatus(Today));
        Assert.Equal(InvoiceStatus.Partial, invoice.Status);
    }

    [Fact]
    public void PaymentStatus_WithPastDueDateAndNoPayment_IsOverdue()
    {
        var invoice = CreateInvoice(total: 1_000, subtotals: [1_000], dueDate: new DateOnly(2025, 1, 1));

        Assert.Equal(PaymentStatus.Overdue, invoice.GetPaymentStatus(Today));
        Assert.True(invoice.GetDaysOverdue(Today) > 30);
    }

    [Fact]
    public void PaymentStatus_WithFutureDueDateAndNoPayment_IsPending()
    {
        var invoice = CreateInvoice(total: 1_000, subtotals: [1_000], dueDate: Today.AddDays(1));

        Assert.Equal(PaymentStatus.Pending, invoice.GetPaymentStatus(Today));
        Assert.Equal(0, invoice.GetDaysOverdue(Today));
    }

    [Fact]
    public void AddCreditNote_CannotExceedPendingBalance()
    {
        var invoice = CreateInvoice(total: 1_000, subtotals: [1_000], creditNotes: [700]);

        var exception = Assert.Throws<DomainException>(
            () => invoice.AddCreditNote(new CreditNote(10, Today, 301)));

        Assert.Contains("supera el saldo pendiente", exception.Message);
    }

    [Fact]
    public void AddCreditNote_CanEqualPendingBalance()
    {
        var invoice = CreateInvoice(total: 1_000, subtotals: [1_000], creditNotes: [700]);

        invoice.AddCreditNote(new CreditNote(10, Today, 300));

        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        Assert.Equal(0, invoice.PendingBalance);
    }

    [Fact]
    public void AddCreditNote_OnInconsistentInvoice_IsRejected()
    {
        var invoice = CreateInvoice(total: 1_000, subtotals: [900]);

        Assert.Throws<DomainException>(
            () => invoice.AddCreditNote(new CreditNote(10, Today, 100)));
    }

    private static Invoice CreateInvoice(
        long total,
        long[] subtotals,
        long[]? creditNotes = null,
        DateOnly? paymentDate = null,
        DateOnly? dueDate = null)
    {
        var customer = new Customer("13075795-2", "Cliente de prueba", "cliente@test.cl");

        var details = subtotals
            .Select((subtotal, index) => new InvoiceDetail($"Producto {index + 1}", subtotal, 1, subtotal))
            .ToList();

        var notes = (creditNotes ?? [])
            .Select((amount, index) => new CreditNote(index + 1, new DateOnly(2025, 1, 1), amount))
            .ToList();

        var payment = new InvoicePayment(paymentDate.HasValue ? "Credit Card" : null, paymentDate);

        return new Invoice(
            invoiceNumber: 1,
            invoiceDate: new DateOnly(2025, 1, 1),
            paymentDueDate: dueDate ?? new DateOnly(2025, 2, 1),
            totalAmount: total,
            customer: customer,
            details: details,
            payment: payment,
            creditNotes: notes);
    }
}
