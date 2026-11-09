namespace Facturas.Infrastructure.Persistence;

/// <summary>
/// Vistas SQL usadas por la búsqueda y los reportes. Se crean al iniciar la
/// aplicación para que la base SQLite quede lista sin pasos manuales.
/// </summary>
public static class DatabaseViews
{
    public const string Sql = """
        CREATE VIEW IF NOT EXISTS vw_InvoiceSearch AS
        SELECT
            i.InvoiceNumber AS InvoiceNumber,
            i.InvoiceDate AS InvoiceDate,
            i.PaymentDueDate AS PaymentDueDate,
            i.TotalAmount AS TotalAmount,
            COALESCE(d.SubtotalSum, 0) AS SubtotalSum,
            COALESCE(c.CreditNoteSum, 0) AS CreditNoteSum,
            i.TotalAmount - COALESCE(c.CreditNoteSum, 0) AS PendingBalance,
            i.IsConsistent AS IsConsistent,
            CASE
                WHEN COALESCE(c.CreditNoteSum, 0) = 0 THEN 'Issued'
                WHEN COALESCE(c.CreditNoteSum, 0) >= i.TotalAmount THEN 'Cancelled'
                ELSE 'Partial'
            END AS InvoiceStatus,
            CASE
                WHEN p.PaymentDate IS NOT NULL THEN 'Paid'
                WHEN date(i.PaymentDueDate) < date('now') THEN 'Overdue'
                ELSE 'Pending'
            END AS PaymentStatus,
            cu.Run AS CustomerRun,
            cu.Name AS CustomerName,
            cu.Email AS CustomerEmail,
            p.PaymentMethod AS PaymentMethod,
            p.PaymentDate AS PaymentDate
        FROM Invoices i
        INNER JOIN Customers cu ON cu.Id = i.CustomerId
        LEFT JOIN (
            SELECT InvoiceId, SUM(Subtotal) AS SubtotalSum
            FROM InvoiceDetails
            GROUP BY InvoiceId
        ) d ON d.InvoiceId = i.Id
        LEFT JOIN (
            SELECT InvoiceId, SUM(CreditNoteAmount) AS CreditNoteSum
            FROM CreditNotes
            GROUP BY InvoiceId
        ) c ON c.InvoiceId = i.Id
        LEFT JOIN InvoicePayments p ON p.InvoiceId = i.Id;

        CREATE VIEW IF NOT EXISTS vw_PaymentSummary AS
        SELECT
            PaymentStatus AS PaymentStatus,
            COUNT(*) AS InvoiceCount,
            SUM(TotalAmount) AS TotalAmount
        FROM vw_InvoiceSearch
        WHERE IsConsistent = 1
        GROUP BY PaymentStatus;

        CREATE VIEW IF NOT EXISTS vw_OverdueWithoutPaymentOrCredit AS
        SELECT
            InvoiceNumber AS InvoiceNumber,
            CustomerName AS CustomerName,
            PaymentDueDate AS PaymentDueDate,
            CAST(julianday(date('now')) - julianday(date(PaymentDueDate)) AS INTEGER) AS DaysOverdue,
            TotalAmount AS TotalAmount,
            PendingBalance AS PendingBalance,
            InvoiceStatus AS InvoiceStatus
        FROM vw_InvoiceSearch
        WHERE IsConsistent = 1
          AND PaymentDate IS NULL
          AND CreditNoteSum = 0
          AND date(PaymentDueDate) < date('now', '-30 days');

        CREATE VIEW IF NOT EXISTS vw_InconsistentInvoices AS
        SELECT
            InvoiceNumber AS InvoiceNumber,
            CustomerName AS CustomerName,
            InvoiceDate AS InvoiceDate,
            TotalAmount AS TotalAmount,
            SubtotalSum AS SubtotalSum,
            SubtotalSum - TotalAmount AS Difference,
            PaymentStatus AS PaymentStatus
        FROM vw_InvoiceSearch
        WHERE IsConsistent = 0;
        """;
}
