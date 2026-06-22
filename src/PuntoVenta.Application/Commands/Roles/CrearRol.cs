using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Application.Commands.Roles;

public sealed record CrearRolCommand(
    string Nombre,
    string? Descripcion = null) : IRequest<ErrorOr<Guid>>;

public sealed class CrearRolHandler(IRolRepository rolRepository) : IRequestHandler<CrearRolCommand, ErrorOr<Guid>>
{
    private readonly IRolRepository _rolRepository = rolRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearRolCommand command, CancellationToken cancellationToken)
    {
        if (await _rolRepository.ExisteNombreAsync(command.Nombre.Trim(), cancellationToken))
        {
            return RolErrors.NombreYaExiste;
        }

        var resultado = Rol.Crear(command.Nombre, command.Descripcion);
        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _rolRepository.AddAsync(resultado.Value, cancellationToken);
        return resultado.Value.Id;
    }
}
