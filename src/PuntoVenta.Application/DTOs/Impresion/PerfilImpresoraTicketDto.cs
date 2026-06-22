using PuntoVenta.Domain.Entities.Impresion;

namespace PuntoVenta.Application.DTOs.Impresion;

public sealed record PerfilImpresoraTicketDto
{
    public Guid Id { get; init; }
    public string Clave { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public int AnchoMm { get; init; }
    public int CharsPorLinea { get; init; }
    public string Codepage { get; init; } = string.Empty;
    public byte DrawerPin { get; init; }
    public ComandoCorteTicket ComandoCorte { get; init; }
    public byte Densidad { get; init; }
    public bool Activo { get; init; }
}
