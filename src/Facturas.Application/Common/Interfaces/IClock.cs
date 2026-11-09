namespace Facturas.Application.Common.Interfaces;

/// <summary>
/// Reloj abstraído para poder probar reglas dependientes de la fecha.
/// </summary>
public interface IClock
{
    DateOnly Today { get; }
}
