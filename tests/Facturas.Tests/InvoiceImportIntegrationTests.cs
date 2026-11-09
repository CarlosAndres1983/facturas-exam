using Facturas.Application.Import.Dtos;
using Facturas.Infrastructure.Import;
using Facturas.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Facturas.Tests;

public class InvoiceImportIntegrationTests
{
    [Fact]
    public async Task Import_LoadsAllInvoices_AndDetectsTheTwoInconsistencies()
    {
        await using var fixture = await TestDatabase.CreateAsync();

        var result = await fixture.ImportAsync();

        Assert.Equal(50, result.TotalInFile);
        Assert.Equal(50, result.Imported);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(2, result.Inconsistent);
        Assert.Equal(50, await fixture.Db.Invoices.CountAsync());

        var inconsistent = await fixture.Db.Invoices
            .Where(x => !x.IsConsistent)
            .Select(x => x.InvoiceNumber)
            .OrderBy(x => x)
            .ToListAsync();

        Assert.Equal(new[] { 41, 48 }, inconsistent);
    }

    [Fact]
    public async Task Import_IsIdempotent()
    {
        await using var fixture = await TestDatabase.CreateAsync();

        await fixture.ImportAsync();
        var second = await fixture.ImportAsync();

        Assert.Equal(0, second.Imported);
        Assert.Equal(50, second.Skipped);
        Assert.Equal(50, await fixture.Db.Invoices.CountAsync());
    }

    [Fact]
    public async Task Views_ComputeInvoiceAndPaymentStatuses()
    {
        await using var fixture = await TestDatabase.CreateAsync();
        await fixture.ImportAsync();

        var invoice30 = await fixture.Db.InvoiceSearch.SingleAsync(x => x.InvoiceNumber == 30);
        Assert.Equal("Issued", invoice30.InvoiceStatus);

        var invoice22 = await fixture.Db.InvoiceSearch.SingleAsync(x => x.InvoiceNumber == 22);
        Assert.Equal("Cancelled", invoice22.InvoiceStatus);

        var invoice2 = await fixture.Db.InvoiceSearch.SingleAsync(x => x.InvoiceNumber == 2);
        Assert.Equal("Partial", invoice2.InvoiceStatus);
        Assert.Equal("Paid", invoice2.PaymentStatus);
    }

    [Fact]
    public async Task Views_ProduceTheExpectedReports()
    {
        await using var fixture = await TestDatabase.CreateAsync();
        await fixture.ImportAsync();

        // 48 facturas activas: 13 pagadas y 35 vencidas.
        var summary = await fixture.Db.PaymentSummary.ToListAsync();
        Assert.Equal(13, summary.Single(x => x.PaymentStatus == "Paid").InvoiceCount);
        Assert.Equal(35, summary.Single(x => x.PaymentStatus == "Overdue").InvoiceCount);
        Assert.DoesNotContain(summary, x => x.PaymentStatus == "Pending");

        // 30 facturas consistentes vencidas hace más de 30 días, sin pago ni NC.
        Assert.Equal(30, await fixture.Db.OverdueInvoices.CountAsync());

        // Solo las facturas 41 y 48 son inconsistentes.
        Assert.Equal(2, await fixture.Db.InconsistentInvoices.CountAsync());
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly IConfiguration _configuration;

        private TestDatabase(SqliteConnection connection, AppDbContext db, IConfiguration configuration)
        {
            _connection = connection;
            Db = db;
            _configuration = configuration;
        }

        public AppDbContext Db { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var db = new AppDbContext(options);
            await db.Database.EnsureCreatedAsync();
            await db.Database.ExecuteSqlRawAsync(DatabaseViews.Sql);

            var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "bd_exam.json");
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["DataFile:Path"] = dataPath
                })
                .Build();

            return new TestDatabase(connection, db, configuration);
        }

        public Task<ImportResultDto> ImportAsync()
        {
            var importer = new InvoiceImportService(
                Db,
                _configuration,
                NullLogger<InvoiceImportService>.Instance);

            return importer.ImportAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
