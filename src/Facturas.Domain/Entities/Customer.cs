namespace Facturas.Domain.Entities;

public class Customer
{
    public int Id { get; private set; }

    /// <summary>
    /// RUN/RUT chileno o identificador extranjero. Se almacena como texto porque
    /// el archivo de origen contiene valores como "9G13WC4RU71" o "24753685-k".
    /// </summary>
    public string Run { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    private Customer()
    {
    }

    public Customer(string run, string name, string email)
    {
        if (string.IsNullOrWhiteSpace(run))
        {
            throw new ArgumentException("El RUN del cliente es obligatorio.", nameof(run));
        }

        Run = run.Trim();
        Name = name?.Trim() ?? string.Empty;
        Email = email?.Trim() ?? string.Empty;
    }
}
