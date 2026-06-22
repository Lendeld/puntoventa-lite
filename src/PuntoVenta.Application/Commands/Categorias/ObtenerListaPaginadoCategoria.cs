using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Categorias;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Categorias;

public sealed record ObtenerListaPaginadoCategoriaQuery(
    int NumeroPagina = 1,
    int TamanoPagina = 10,
    string? FiltroDinamico = null,
    bool? Activo = null) : IRequest<ErrorOr<PagedResult<CategoriaDto>>>;

public sealed class ObtenerListaPaginadoCategoriaHandler(ICategoriaRepository categoriaRepository) : IRequestHandler<ObtenerListaPaginadoCategoriaQuery, ErrorOr<PagedResult<CategoriaDto>>>
{
    private readonly ICategoriaRepository _categoriaRepository = categoriaRepository;

    public async ValueTask<ErrorOr<PagedResult<CategoriaDto>>> Handle(ObtenerListaPaginadoCategoriaQuery query, CancellationToken cancellationToken)
    {
        var (pagina, tamano) = Paginacion.Normalizar(query.NumeroPagina, query.TamanoPagina);

        var (items, total) = await _categoriaRepository.ObtenerListaPaginadoAsync(
            pagina,
            tamano,
            query.FiltroDinamico,
            query.Activo,
            cancellationToken);

        return PagedResult<CategoriaDto>.Crear([.. items.Select(CategoriaMapper.ToDto)], pagina, tamano, total);
    }
}
