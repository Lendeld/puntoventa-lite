using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Usuarios;

public sealed record CrearUsuarioCommand(
    string NombreUsuario,
    string Nombre,
    string Identificacion,
    string Password,
    Guid? RolId = null,
    string? Correo = null,
    string? Telefono = null) : IRequest<ErrorOr<Guid>>;

public sealed class CrearUsuarioHandler(
    IUsuarioRepository usuarioRepository,
    IRolRepository rolRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<CrearUsuarioCommand, ErrorOr<Guid>>
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IRolRepository _rolRepository = rolRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearUsuarioCommand command, CancellationToken cancellationToken)
    {
        if (command.RolId.HasValue)
        {
            var rol = await _rolRepository.GetByIdAsync(command.RolId.Value, cancellationToken);
            if (rol is null)
            {
                return RolErrors.NoEncontrado;
            }
        }

        if (await _usuarioRepository.ExisteNombreUsuarioAsync(command.NombreUsuario.Trim(), cancellationToken))
        {
            return UsuarioErrors.NombreUsuarioYaExiste;
        }

        // Identificación opcional: solo se valida unicidad cuando viene informada.
        if (!string.IsNullOrWhiteSpace(command.Identificacion)
            && await _usuarioRepository.ExisteIdentificacionAsync(command.Identificacion.Trim(), cancellationToken))
        {
            return UsuarioErrors.IdentificacionYaExiste;
        }

        var hash = _passwordHasher.Hash(command.Password);

        var resultado = Usuario.Crear(
            command.NombreUsuario,
            command.Nombre,
            command.Identificacion,
            hash,
            command.Correo,
            command.Telefono,
            command.RolId);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        resultado.Value.RequerirCambioPassword();
        var usuario = await _usuarioRepository.AddAsync(resultado.Value, cancellationToken);

        return usuario.Id;
    }
}
