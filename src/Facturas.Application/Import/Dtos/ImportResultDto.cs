namespace Facturas.Application.Import.Dtos;

public sealed record ImportResultDto(
    int TotalInFile,
    int Imported,
    int Skipped,
    int Inconsistent);
