using Facturas.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Facturas.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseInitializer");

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(DatabaseViews.Sql, cancellationToken);

        var importer = scope.ServiceProvider.GetRequiredService<IInvoiceImportService>();
        var result = await importer.ImportAsync(cancellationToken);

        logger.LogInformation(
            "Base de datos lista. Facturas en archivo: {Total}, importadas: {Imported}, omitidas: {Skipped}, inconsistentes: {Inconsistent}.",
            result.TotalInFile,
            result.Imported,
            result.Skipped,
            result.Inconsistent);
    }
}
