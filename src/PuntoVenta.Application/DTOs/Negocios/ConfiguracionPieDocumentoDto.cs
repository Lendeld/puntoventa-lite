using PuntoVenta.Domain.Entities.Negocios;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.DTOs.Negocios;

public sealed record ConfiguracionPieDocumentoDto
{
    public string Nombre { get; init; } = string.Empty;
    public DestinoLineaPie Destino { get; init; }
    public IReadOnlyList<TipoDocumentoVenta> TiposDocumento { get; init; } = [];
    public IReadOnlyList<LineaPieDocumentoDto> Lineas { get; init; } = [];
}
