using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Services;

public sealed class FechaActual : IFechaActual, ISingletonService
{
    // América Central: UTC-6 (no aplica horario de verano)
    private static readonly TimeZoneInfo ZonaCentroamerica =
        TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    public DateTime AhoraUtc => DateTime.UtcNow;

    public DateTime Ahora =>
        TimeZoneInfo.ConvertTimeFromUtc(AhoraUtc, ZonaCentroamerica);

    public DateOnly Hoy => DateOnly.FromDateTime(Ahora);

    public DateOnly ALocal(DateTime utc)
    {
        var utcKind = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcKind, ZonaCentroamerica));
    }
}
