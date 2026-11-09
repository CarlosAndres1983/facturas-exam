using Facturas.Application.Common.Interfaces;
using Facturas.Infrastructure.Common;
using Facturas.Infrastructure.Import;
using Facturas.Infrastructure.Persistence;
using Facturas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Facturas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=facturas.db";

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IInvoiceImportService, InvoiceImportService>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
