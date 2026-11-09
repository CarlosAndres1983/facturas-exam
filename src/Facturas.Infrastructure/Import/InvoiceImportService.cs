using System.Globalization;
using System.Text.Json;
using Facturas.Application.Common.Interfaces;
using Facturas.Application.Import.Dtos;
using Facturas.Domain.Entities;
using Facturas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Facturas.Infrastructure.Import;

public sealed class InvoiceImportService : IInvoiceImportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InvoiceImportService> _logger;

    public InvoiceImportService(
        AppDbContext dbContext,
        IConfiguration configuration,
        ILogger<InvoiceImportService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ImportResultDto> ImportAsync(CancellationToken cancellationToken = default)
    {
        var path = ResolvePath();

        if (!File.Exists(path))
        {
            _logger.LogWarning("No se encontró el archivo de facturas en {Path}.", path);
            return new ImportResultDto(0, 0, 0, 0);
        }

        await using var stream = File.OpenRead(path);
        var file = await JsonSerializer.DeserializeAsync<InvoiceFileJson>(stream, JsonOptions, cancellationToken)
            ?? new InvoiceFileJson();

        // Importación idempotente: el número de factura es la clave natural.
        var existingNumbers = (await _dbContext.Invoices
                .Select(x => x.InvoiceNumber)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var customers = await _dbContext.Customers
            .ToDictionaryAsync(x => x.Run, cancellationToken);

        var imported = 0;
        var skipped = 0;
        var inconsistent = 0;

        foreach (var dto in file.Invoices)
        {
            if (existingNumbers.Contains(dto.InvoiceNumber))
            {
                skipped++;
                continue;
            }

            if (!customers.TryGetValue(dto.Customer.CustomerRun, out var customer))
            {
                customer = new Customer(
                    dto.Customer.CustomerRun,
                    dto.Customer.CustomerName,
                    dto.Customer.CustomerEmail);

                _dbContext.Customers.Add(customer);
                customers[dto.Customer.CustomerRun] = customer;
            }

            var details = dto.InvoiceDetail
                .Select(x => new InvoiceDetail(x.ProductName, x.UnitPrice, x.Quantity, x.Subtotal))
                .ToList();

            var payment = dto.InvoicePayment is null
                ? null
                : new InvoicePayment(
                    dto.InvoicePayment.PaymentMethod,
                    ParseNullableDate(dto.InvoicePayment.PaymentDate));

            var creditNotes = dto.InvoiceCreditNote
                .Select(x => new CreditNote(
                    x.CreditNoteNumber,
                    ParseDate(x.CreditNoteDate),
                    x.CreditNoteAmount))
                .ToList();

            var invoice = new Invoice(
                dto.InvoiceNumber,
                ParseDate(dto.InvoiceDate),
                ParseDate(dto.PaymentDueDate),
                dto.TotalAmount,
                customer,
                details,
                payment,
                creditNotes);

            _dbContext.Invoices.Add(invoice);
            imported++;

            if (!invoice.IsConsistent)
            {
                inconsistent++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Importación finalizada: {Imported} importadas, {Skipped} omitidas, {Inconsistent} inconsistentes de {Total}.",
            imported,
            skipped,
            inconsistent,
            file.Invoices.Count);

        return new ImportResultDto(file.Invoices.Count, imported, skipped, inconsistent);
    }

    private string ResolvePath()
    {
        var configured = _configuration["DataFile:Path"];

        if (string.IsNullOrWhiteSpace(configured))
        {
            return Path.Combine(AppContext.BaseDirectory, "Data", "bd_exam.json");
        }

        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);
    }

    private static DateOnly ParseDate(string value)
    {
        return DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static DateOnly? ParseNullableDate(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : ParseDate(value);
    }
}
