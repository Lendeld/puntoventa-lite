using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Categorias;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Categorias;

public sealed record ObtenerCategoriasActivasQuery : IRequest<ErrorOr<IReadOnlyList<CategoriaDto>>>;

public sealed class ObtenerCategoriasActivasHandler(ICategoriaRepository categoriaRepository) : IRequestHandler<ObtenerCategoriasActivasQuery, ErrorOr<IReadOnlyList<CategoriaDto>>>
{
    private readonly ICategoriaRepository _categoriaRepository = categoriaRepository;

    public async ValueTask<ErrorOr<IReadOnlyList<CategoriaDto>>> Handle(ObtenerCategoriasActivasQuery query, CancellationToken cancellationToken)
    {
        var items = await _categoriaRepository.ObtenerActivosAsync(cancellationToken);
        return items.Select(CategoriaMapper.ToDto).ToList();
    }
}
