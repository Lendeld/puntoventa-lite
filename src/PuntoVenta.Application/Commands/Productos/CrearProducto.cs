using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.MovimientosStock;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.Commands.Productos;

public sealed record CrearProductoCommand(
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
    bool PermiteModificarPrecioUnitario = false,
    decimal? ExistenciaInicial = null) : IRequest<ErrorOr<Guid>>;

public sealed class CrearProductoHandler(
    IUsuarioActual usuarioActual,
    IProductoRepository productoRepository,
    ICategoriaRepository categoriaRepository,
    IMovimientoStockRepository movimientoRepository,
    IFechaActual fechaActual,
    IPermisoCache permisoCache) : IRequestHandler<CrearProductoCommand, ErrorOr<Guid>>
{
    private const string PermisoProductosNoAplicaExistencias = "productos:no-aplica-existencias";

    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly ICategoriaRepository _categoriaRepository = categoriaRepository;
    private readonly IMovimientoStockRepository _movimientoRepository = movimientoRepository;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IPermisoCache _permisoCache = permisoCache;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearProductoCommand command, CancellationToken cancellationToken)
    {
        var noAplicaExistencias = command.NoAplicaExistencias ?? false;

        if (noAplicaExistencias)
        {
            var autorizacion = await ValidarPermisoNoAplicaExistenciasAsync(cancellationToken);
            if (autorizacion.IsError)
            {
                return autorizacion.Errors;
            }
        }

        if (await _productoRepository.ExisteCodigoAsync(command.Codigo.Trim(), cancellationToken))
        {
            return ProductoErrors.CodigoYaExiste;
        }

        var codigoBarrasNormalizado = command.CodigoBarras?.Trim();
        if (!string.IsNullOrEmpty(codigoBarrasNormalizado) &&
            await _productoRepository.ExisteCodigoBarrasAsync(codigoBarrasNormalizado, cancellationToken))
        {
            return ProductoErrors.CodigoBarrasYaExiste;
        }

        if (command.CategoriaId.HasValue)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(command.CategoriaId.Value, cancellationToken);
            if (categoria is null)
            {
                return Error.Validation("Producto_CategoriaId", "La categoría indicada no existe.");
            }
        }

        var resultado = Producto.Crear(
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
            command.PermiteModificarPrecioUnitario);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        var producto = resultado.Value;

        // Stock inicial opcional: evita el paso extra de crear y luego ajustar.
        // Solo aplica a bienes con existencias y cantidad positiva; se registra
        // como movimiento "Stock inicial" para mantener la trazabilidad.
        var existenciaInicial = command.ExistenciaInicial ?? 0m;
        var aplicaInicial = !noAplicaExistencias && existenciaInicial > 0;

        if (aplicaInicial)
        {
            producto.AplicarMovimientoStock(existenciaInicial);
        }

        await _productoRepository.AddAsync(producto, cancellationToken);

        if (aplicaInicial)
        {
            var movimiento = MovimientoStock.Crear(
                productoId: producto.Id,
                fechaUtc: _fechaActual.AhoraUtc,
                delta: existenciaInicial,
                saldoResultante: producto.Existencia,
                usuarioId: _usuarioActual.UsuarioId,
                razon: "Stock inicial");

            if (movimiento.IsError)
            {
                return movimiento.Errors;
            }

            await _movimientoRepository.AddAsync(movimiento.Value, cancellationToken);
        }

        return producto.Id;
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
