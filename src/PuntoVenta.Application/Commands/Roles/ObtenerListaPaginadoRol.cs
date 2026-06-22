using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Roles;

public sealed record ObtenerListaPaginadoRolQuery(
    int Pagina = 1,
    int Tamano = 10,
    string? FiltroDinamico = null,
    bool? Activo = null) : IRequest<ErrorOr<PagedResult<RolDto>>>;

public sealed class ObtenerListaPaginadoRolHandler(IRolRepository rolRepository) : IRequestHandler<ObtenerListaPaginadoRolQuery, ErrorOr<PagedResult<RolDto>>>
{
    private readonly IRolRepository _rolRepository = rolRepository;

    public async ValueTask<ErrorOr<PagedResult<RolDto>>> Handle(ObtenerListaPaginadoRolQuery query, CancellationToken cancellationToken)
    {
        var (pagina, tamano) = Paginacion.Normalizar(query.Pagina, query.Tamano);

        var (items, total) = await _rolRepository.ObtenerListaPaginadoAsync(
            pagina,
            tamano,
            query.FiltroDinamico,
            query.Activo,
            cancellationToken);

        var dtos = items.Select(RolMapper.ToDto).ToList();

        return PagedResult<RolDto>.Crear(dtos, pagina, tamano, total);
    }
}
