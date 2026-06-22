namespace PuntoVenta.Application.DTOs.Negocios;

public sealed record NegocioDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? NombreComercial { get; init; }
    public string Direccion { get; init; } = string.Empty;
    public string TipoIdentificacionCodigo { get; init; } = string.Empty;
    public string Identificacion { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public bool AplicaVendedores { get; init; }
    public bool AplicaCajas { get; init; }
    public decimal TipoCambioPredeterminado { get; init; }
    public string? LogoUrl { get; init; }
    public bool Activo { get; init; }
    public string? ModificadoPor { get; init; }
    public DateTime? FechaModificacion { get; init; }

}
