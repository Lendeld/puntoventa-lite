namespace PuntoVenta.Application.Interfaces;

public interface IFechaActual
{
    DateTime Ahora { get; }
    DateTime AhoraUtc { get; }
    DateOnly Hoy { get; }
    DateOnly ALocal(DateTime utc);
}
