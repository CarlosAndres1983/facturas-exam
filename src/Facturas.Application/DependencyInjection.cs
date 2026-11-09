using Facturas.Application.Invoices;
using Facturas.Application.Reports;
using Microsoft.Extensions.DependencyInjection;

namespace Facturas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<InvoiceService>();
        services.AddScoped<ReportService>();
        return services;
    }
}
