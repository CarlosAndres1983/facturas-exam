using Facturas.Application.Common.Interfaces;
using Facturas.Domain.Entities;
using Facturas.Infrastructure.Persistence.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Facturas.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceDetail> InvoiceDetails => Set<InvoiceDetail>();

    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();

    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();

    // Read models respaldados por vistas SQL.
    public DbSet<InvoiceSearchView> InvoiceSearch => Set<InvoiceSearchView>();

    public DbSet<PaymentSummaryView> PaymentSummary => Set<PaymentSummaryView>();

    public DbSet<OverdueInvoiceView> OverdueInvoices => Set<OverdueInvoiceView>();

    public DbSet<InconsistentInvoiceView> InconsistentInvoices => Set<InconsistentInvoiceView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Run).IsRequired().HasMaxLength(30);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(x => x.Run).IsUnique();
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.InvoiceNumber).IsRequired();
            entity.HasIndex(x => x.InvoiceNumber).IsUnique();
            entity.Property(x => x.InvoiceDate).IsRequired();
            entity.Property(x => x.PaymentDueDate).IsRequired();
            entity.Property(x => x.TotalAmount).IsRequired();
            entity.Property(x => x.IsConsistent).IsRequired();

            entity.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Details)
                .WithOne()
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.CreditNotes)
                .WithOne()
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Payment)
                .WithOne()
                .HasForeignKey<InvoicePayment>(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(x => x.SubtotalSum);
            entity.Ignore(x => x.CreditNoteSum);
            entity.Ignore(x => x.PendingBalance);
            entity.Ignore(x => x.Status);
            entity.Ignore(x => x.IsActive);
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.ToTable("InvoiceDetails");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductName).IsRequired().HasMaxLength(300);
            entity.Property(x => x.UnitPrice).IsRequired();
            entity.Property(x => x.Quantity).IsRequired();
            entity.Property(x => x.Subtotal).IsRequired();
            entity.Ignore(x => x.CalculatedSubtotal);
            entity.Ignore(x => x.IsLineConsistent);
        });

        modelBuilder.Entity<CreditNote>(entity =>
        {
            entity.ToTable("CreditNotes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CreditNoteNumber).IsRequired();
            entity.HasIndex(x => x.CreditNoteNumber).IsUnique();
            entity.Property(x => x.CreditNoteDate).IsRequired();
            entity.Property(x => x.CreditNoteAmount).IsRequired();
        });

        modelBuilder.Entity<InvoicePayment>(entity =>
        {
            entity.ToTable("InvoicePayments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PaymentMethod).HasMaxLength(100);
            entity.Property(x => x.PaymentDate);
            entity.Ignore(x => x.IsPaid);
        });

        modelBuilder.Entity<InvoiceSearchView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_InvoiceSearch");
        });

        modelBuilder.Entity<PaymentSummaryView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_PaymentSummary");
        });

        modelBuilder.Entity<OverdueInvoiceView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_OverdueWithoutPaymentOrCredit");
        });

        modelBuilder.Entity<InconsistentInvoiceView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_InconsistentInvoices");
        });
    }
}
