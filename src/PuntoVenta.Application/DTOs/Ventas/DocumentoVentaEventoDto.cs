namespace PuntoVenta.Application.DTOs.Ventas;

public sealed record DocumentoVentaEventoDto
{
    public Guid Id { get; init; }
    public Guid DocumentoVentaId { get; init; }
    public string TipoCodigo { get; init; } = string.Empty;
    public string TipoNombre { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public string? IconoSugerido { get; init; }
    public string? ColorSugerido { get; init; }
    public DateTime OcurridoEn { get; init; }
    public Guid? UsuarioId { get; init; }
    public string? UsuarioNombre { get; init; }
    public string Resumen { get; init; } = string.Empty;
    public Guid? CorrelacionId { get; init; }
}

public sealed record DocumentoVentaEventoListaDto
{
    public IReadOnlyList<DocumentoVentaEventoDto> Items { get; init; } = [];
    public int Total { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
}
