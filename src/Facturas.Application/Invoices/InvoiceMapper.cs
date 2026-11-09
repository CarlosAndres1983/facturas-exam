using Facturas.Application.Invoices.Dtos;
using Facturas.Domain.Entities;

namespace Facturas.Application.Invoices;

internal static class InvoiceMapper
{
    public static InvoiceDetailDto ToDetailDto(Invoice invoice, DateOnly today)
    {
        return new InvoiceDetailDto(
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.PaymentDueDate,
            invoice.TotalAmount,
            invoice.SubtotalSum,
            invoice.CreditNoteSum,
            invoice.PendingBalance,
            invoice.IsConsistent,
            invoice.Status.ToString(),
            invoice.GetPaymentStatus(today).ToString(),
            invoice.GetDaysOverdue(today),
            new CustomerDto(invoice.Customer.Run, invoice.Customer.Name, invoice.Customer.Email),
            invoice.Payment is null
                ? null
                : new InvoicePaymentDto(invoice.Payment.PaymentMethod, invoice.Payment.PaymentDate),
            invoice.Details
                .OrderBy(d => d.Id)
                .Select(d => new InvoiceDetailLineDto(
                    d.ProductName,
                    d.UnitPrice,
                    d.Quantity,
                    d.Subtotal,
                    d.CalculatedSubtotal,
                    d.IsLineConsistent))
                .ToList(),
            invoice.CreditNotes
                .OrderBy(c => c.CreditNoteNumber)
                .Select(ToCreditNoteDto)
                .ToList());
    }

    public static CreditNoteDto ToCreditNoteDto(CreditNote creditNote)
    {
        return new CreditNoteDto(
            creditNote.CreditNoteNumber,
            creditNote.CreditNoteDate,
            creditNote.CreditNoteAmount);
    }
}
