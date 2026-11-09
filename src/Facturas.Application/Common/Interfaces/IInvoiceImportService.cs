using Facturas.Application.Import.Dtos;

namespace Facturas.Application.Common.Interfaces;

public interface IInvoiceImportService
{
    Task<ImportResultDto> ImportAsync(CancellationToken cancellationToken = default);
}
