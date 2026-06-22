namespace PuntoVenta.Application.DTOs.Negocios;

public sealed record NegocioTicketConfigDto
{
    public Guid Id { get; init; }
    public string? MensajePie { get; init; }
    public bool MostrarLogo { get; init; }
    public bool AplicaCopiaClienteNegocio { get; init; }
    public bool MostrarCodigoBarras { get; init; }
    public IReadOnlyList<ConfiguracionPieDocumentoDto> Configuraciones { get; init; } = [];
    public IReadOnlyList<ElementoEncabezadoDto> ElementosEncabezado { get; init; } = [];
}
