namespace PuntoVenta.Application.DTOs.MediosPago;

public sealed record MedioPagoDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Detalle { get; init; } = string.Empty;
    public string? Comentario { get; init; }
    public bool Activo { get; init; }
    public string? ModificadoPor { get; init; }
    public DateTime? FechaModificacion { get; init; }
}
