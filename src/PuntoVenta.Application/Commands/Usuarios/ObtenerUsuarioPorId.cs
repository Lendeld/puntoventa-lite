using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Usuarios;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Usuarios;


namespace PuntoVenta.Application.Commands.Usuarios;

public sealed record ObtenerUsuarioPorIdQuery(Guid Id) : IRequest<ErrorOr<UsuarioDto>>;

public sealed class ObtenerUsuarioPorIdHandler(
    IUsuarioRepository usuarioRepository,
    IRolRepository rolRepository) : IRequestHandler<ObtenerUsuarioPorIdQuery, ErrorOr<UsuarioDto>>
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IRolRepository _rolRepository = rolRepository;

    public async ValueTask<ErrorOr<UsuarioDto>> Handle(ObtenerUsuarioPorIdQuery query, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdConAuditoriaAsync(query.Id, cancellationToken);

        if (usuario is null)
        {
            return UsuarioErrors.NoEncontrado;
        }

        var rol = usuario.RolId.HasValue
            ? await _rolRepository.GetByIdAsync(usuario.RolId.Value, cancellationToken)
            : null;

        return UsuarioMapper.ToDto(usuario, rol);
    }
}
