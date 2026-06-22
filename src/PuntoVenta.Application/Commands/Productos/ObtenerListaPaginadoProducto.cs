using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Productos;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.Commands.Productos;

public sealed record ObtenerListaPaginadoProductoQuery(
    int NumeroPagina = 1,
    int TamanoPagina = 10,
    string? FiltroDinamico = null,
    TipoItem? TipoItem = null,
    Guid? CategoriaId = null) : IRequest<ErrorOr<PagedResult<ProductoDto>>>;

public sealed class ObtenerListaPaginadoProductoHandler(
    IProductoRepository productoRepository) : IRequestHandler<ObtenerListaPaginadoProductoQuery, ErrorOr<PagedResult<ProductoDto>>>
{
    private readonly IProductoRepository _productoRepository = productoRepository;

    public async ValueTask<ErrorOr<PagedResult<ProductoDto>>> Handle(ObtenerListaPaginadoProductoQuery query, CancellationToken cancellationToken)
    {
        var (pagina, tamano) = Paginacion.Normalizar(query.NumeroPagina, query.TamanoPagina);

        var (items, total) = await _productoRepository.ObtenerListaPaginadoAsync(
            pagina,
            tamano,
            query.FiltroDinamico,
            query.TipoItem,
            query.CategoriaId,
            cancellationToken);

        var dtos = items.Select(p => ProductoMapper.ToDto(p)).ToList();

        return PagedResult<ProductoDto>.Crear(dtos, pagina, tamano, total);
    }
}
