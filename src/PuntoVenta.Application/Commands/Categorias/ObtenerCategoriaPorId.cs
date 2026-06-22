using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Categorias;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Categorias;

namespace PuntoVenta.Application.Commands.Categorias;

public sealed record ObtenerCategoriaPorIdQuery(Guid Id) : IRequest<ErrorOr<CategoriaDto>>;

public sealed class ObtenerCategoriaPorIdHandler(ICategoriaRepository categoriaRepository) : IRequestHandler<ObtenerCategoriaPorIdQuery, ErrorOr<CategoriaDto>>
{
    private readonly ICategoriaRepository _categoriaRepository = categoriaRepository;

    public async ValueTask<ErrorOr<CategoriaDto>> Handle(ObtenerCategoriaPorIdQuery query, CancellationToken cancellationToken)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdConAuditoriaAsync(query.Id, cancellationToken);

        if (categoria is null)
        {
            return CategoriaErrors.NoEncontrado;
        }

        return CategoriaMapper.ToDto(categoria);
    }
}
