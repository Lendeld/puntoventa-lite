using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.Commands.Productos;

public sealed record ActualizarProductoCommand(
    Guid Id,
    string Codigo,
    string Nombre,
    TipoItem TipoItem,
    decimal PrecioUnitario,
    string? CodigoBarras = null,
    string? Descripcion = null,
    string? ImagenUrl = null,
    decimal? PrecioCosto = null,
    Guid? CategoriaId = null,
    string? TarifaIvaImpuestoCodigo = null,
    bool? NoAplicaExistencias = null,
    bool? PermiteModificarPrecioUnitario = null) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarProductoHandler(
    IUsuarioActual usuarioActual,
    IProductoRepository productoRepository,
    ICategoriaRepository categoriaRepository,
    IPermisoCache permisoCache) : IRequestHandler<ActualizarProductoCommand, ErrorOr<Success>>
{
    private const string PermisoProductosNoAplicaExistencias = "productos:no-aplica-existencias";

    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly ICategoriaRepository _categoriaRepository = categoriaRepository;
    private readonly IPermisoCache _permisoCache = permisoCache;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarProductoCommand command, CancellationToken cancellationToken)
    {
        var producto = await _productoRepository.ObtenerPorIdConAuditoriaAsync(command.Id, cancellationToken);

        if (producto is null)
        {
            return ProductoErrors.NoEncontrado;
        }

        if (await _productoRepository.ExisteCodigoExcluyendoAsync(command.Codigo.Trim(), command.Id, cancellationToken))
        {
            return ProductoErrors.CodigoYaExiste;
        }

        var codigoBarrasNormalizado = command.CodigoBarras?.Trim();
        if (!string.IsNullOrEmpty(codigoBarrasNormalizado) &&
            await _productoRepository.ExisteCodigoBarrasExcluyendoAsync(codigoBarrasNormalizado, command.Id, cancellationToken))
        {
            return ProductoErrors.CodigoBarrasYaExiste;
        }

        var noAplicaExistencias = command.NoAplicaExistencias ?? producto.NoAplicaExistencias;
        var permiteModificarPrecioUnitario = command.PermiteModificarPrecioUnitario ?? producto.PermiteModificarPrecioUnitario;

        var intentaModificarNoAplicaExistencias =
            command.NoAplicaExistencias.HasValue &&
            command.NoAplicaExistencias.Value != producto.NoAplicaExistencias;

        if (intentaModificarNoAplicaExistencias)
        {
            var autorizacion = await ValidarPermisoNoAplicaExistenciasAsync(cancellationToken);
            if (autorizacion.IsError)
            {
                return autorizacion.Errors;
            }
        }

        if (command.CategoriaId.HasValue)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(command.CategoriaId.Value, cancellationToken);
            if (categoria is null)
            {
                return Error.Validation("Producto_CategoriaId", "La categoría indicada no existe.");
            }
        }

        var resultado = producto.Actualizar(
            command.Codigo,
            command.Nombre,
            command.TipoItem,
            command.PrecioUnitario,
            command.CodigoBarras,
            command.Descripcion,
            command.ImagenUrl,
            command.PrecioCosto,
            command.CategoriaId,
            command.TarifaIvaImpuestoCodigo,
            noAplicaExistencias,
            permiteModificarPrecioUnitario);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _productoRepository.UpdateAsync(producto, cancellationToken);

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ValidarPermisoNoAplicaExistenciasAsync(CancellationToken cancellationToken)
    {
        var permisos = await _permisoCache.ObtenerPermisosAsync(
            _usuarioActual.UsuarioId,
            cancellationToken);

        return permisos.Contains(PermisoProductosNoAplicaExistencias)
            ? Result.Success
            : Error.Forbidden(
                "Producto_NoAplicaExistencias_Permiso",
                "No tienes permiso para modificar la opción No aplica existencias.");
    }
}
