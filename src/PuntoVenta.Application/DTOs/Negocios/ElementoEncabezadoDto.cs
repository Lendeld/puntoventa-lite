using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.DTOs.Negocios;

public sealed record ElementoEncabezadoDto
{
    public ElementoEncabezadoTipo Tipo { get; init; }
    public int Orden { get; init; }
    public bool Visible { get; init; }
    public string? TextoLibre { get; init; }
}
