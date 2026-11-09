namespace Facturas.Application.Invoices.Dtos;

public sealed record CustomerDto(
    string Run,
    string Name,
    string Email);
