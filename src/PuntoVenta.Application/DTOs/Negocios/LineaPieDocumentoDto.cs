using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.DTOs.Negocios;

public sealed record LineaPieDocumentoDto
{
    public string Texto { get; init; } = string.Empty;
    public AlineacionLineaPie Alineacion { get; init; }
    public bool Negrita { get; init; }
    public int Orden { get; init; }
}
