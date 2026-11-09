using Facturas.Application.Common.Interfaces;

namespace Facturas.Infrastructure.Common;

public sealed class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
