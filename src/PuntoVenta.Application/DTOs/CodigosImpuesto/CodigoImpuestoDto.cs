namespace PuntoVenta.Application.DTOs.CodigosImpuesto;

public sealed record CodigoImpuestoDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Detalle { get; init; } = string.Empty;
    public string? Comentario { get; init; }
    public bool Activo { get; init; }
    public string? ModificadoPor { get; init; }
    public DateTime? FechaModificacion { get; init; }
}
