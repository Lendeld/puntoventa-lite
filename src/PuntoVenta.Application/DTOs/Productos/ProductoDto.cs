using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.DTOs.Productos;

public sealed record ProductoDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string? CodigoBarras { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public TipoItem TipoItem { get; init; }
    public string? ImagenUrl { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal? PrecioCosto { get; init; }
    public Guid? CategoriaId { get; init; }
    public Guid? ProveedorId { get; init; }
    // Denormalizado solo en el detalle: muestra el nombre del proveedor aunque esté inactivo
    // (el Select del form solo ofrece activos; la FK puede apuntar a uno desactivado luego).
    public string? ProveedorNombre { get; init; }
    public string? TarifaIvaImpuestoCodigo { get; init; }
    public bool NoAplicaExistencias { get; init; }
    public bool PermiteModificarPrecioUnitario { get; init; }
    public decimal ExistenciaTotal { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
}
