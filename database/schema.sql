-- Script de referencia para SQLite.
-- La aplicación crea automáticamente este esquema con EnsureCreated + DatabaseViews.
-- Uso manual: sqlite3 facturas.db < database/schema.sql

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS "Customers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Customers" PRIMARY KEY AUTOINCREMENT,
    "Run" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Email" TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Customers_Run" ON "Customers" ("Run");

CREATE TABLE IF NOT EXISTS "Invoices" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Invoices" PRIMARY KEY AUTOINCREMENT,
    "InvoiceNumber" INTEGER NOT NULL,
    "InvoiceDate" TEXT NOT NULL,
    "PaymentDueDate" TEXT NOT NULL,
    "TotalAmount" INTEGER NOT NULL,
    "IsConsistent" INTEGER NOT NULL,
    "CustomerId" INTEGER NOT NULL,
    CONSTRAINT "FK_Invoices_Customers_CustomerId"
        FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Invoices_InvoiceNumber" ON "Invoices" ("InvoiceNumber");
CREATE INDEX IF NOT EXISTS "IX_Invoices_CustomerId" ON "Invoices" ("CustomerId");

CREATE TABLE IF NOT EXISTS "InvoiceDetails" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InvoiceDetails" PRIMARY KEY AUTOINCREMENT,
    "InvoiceId" INTEGER NOT NULL,
    "ProductName" TEXT NOT NULL,
    "UnitPrice" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "Subtotal" INTEGER NOT NULL,
    CONSTRAINT "FK_InvoiceDetails_Invoices_InvoiceId"
        FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_InvoiceDetails_InvoiceId" ON "InvoiceDetails" ("InvoiceId");

CREATE TABLE IF NOT EXISTS "CreditNotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CreditNotes" PRIMARY KEY AUTOINCREMENT,
    "InvoiceId" INTEGER NOT NULL,
    "CreditNoteNumber" INTEGER NOT NULL,
    "CreditNoteDate" TEXT NOT NULL,
    "CreditNoteAmount" INTEGER NOT NULL,
    CONSTRAINT "FK_CreditNotes_Invoices_InvoiceId"
        FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_CreditNotes_CreditNoteNumber" ON "CreditNotes" ("CreditNoteNumber");
CREATE INDEX IF NOT EXISTS "IX_CreditNotes_InvoiceId" ON "CreditNotes" ("InvoiceId");

CREATE TABLE IF NOT EXISTS "InvoicePayments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InvoicePayments" PRIMARY KEY AUTOINCREMENT,
    "InvoiceId" INTEGER NOT NULL,
    "PaymentMethod" TEXT NULL,
    "PaymentDate" TEXT NULL,
    CONSTRAINT "FK_InvoicePayments_Invoices_InvoiceId"
        FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_InvoicePayments_InvoiceId" ON "InvoicePayments" ("InvoiceId");

-- Vistas usadas por la búsqueda y los reportes.

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
