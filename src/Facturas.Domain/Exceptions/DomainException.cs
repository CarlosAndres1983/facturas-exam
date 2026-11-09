namespace Facturas.Domain.Exceptions;

/// <summary>
/// Violación de una regla de negocio del dominio.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
