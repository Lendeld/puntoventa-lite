using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Usuarios;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Roles;


namespace PuntoVenta.Application.Commands.Usuarios;

public sealed record ObtenerListaPaginadoUsuarioQuery(
    int NumeroPagina = 1,
    int TamanoPagina = 10,
    string? FiltroDinamico = null,
    bool? Activo = null) : IRequest<ErrorOr<PagedResult<UsuarioDto>>>;

public sealed class ObtenerListaPaginadoUsuarioHandler(
    IUsuarioRepository usuarioRepository,
    IRolRepository rolRepository) : IRequestHandler<ObtenerListaPaginadoUsuarioQuery, ErrorOr<PagedResult<UsuarioDto>>>
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IRolRepository _rolRepository = rolRepository;

    public async ValueTask<ErrorOr<PagedResult<UsuarioDto>>> Handle(ObtenerListaPaginadoUsuarioQuery query, CancellationToken cancellationToken)
    {
        var (pagina, tamano) = Paginacion.Normalizar(query.NumeroPagina, query.TamanoPagina);

        var (items, total) = await _usuarioRepository.ObtenerListaPaginadoAsync(
            pagina,
            tamano,
            query.FiltroDinamico,
            query.Activo,
            cancellationToken);

        var rolesPorId = new Dictionary<Guid, Rol>();
        foreach (var rolId in items.Where(u => u.RolId.HasValue).Select(u => u.RolId!.Value).Distinct())
        {
            var rol = await _rolRepository.GetByIdAsync(rolId, cancellationToken);
            if (rol is not null) rolesPorId[rolId] = rol;
        }

        var dtos = items
            .Select(u => UsuarioMapper.ToDto(
                u,
                u.RolId.HasValue && rolesPorId.TryGetValue(u.RolId.Value, out var rol) ? rol : null))
            .ToList();

        return PagedResult<UsuarioDto>.Crear(dtos, pagina, tamano, total);
    }
}
