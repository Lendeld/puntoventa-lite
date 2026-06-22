using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Application.Commands.Roles;

public sealed record ActualizarRolCommand(
    Guid Id,
    string Nombre,
    bool Activo,
    string? Descripcion = null) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarRolHandler(IRolRepository rolRepository) : IRequestHandler<ActualizarRolCommand, ErrorOr<Success>>
{
    private readonly IRolRepository _rolRepository = rolRepository;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarRolCommand command, CancellationToken cancellationToken)
    {
        var rol = await _rolRepository.GetByIdAsync(command.Id, cancellationToken);
        if (rol is null)
        {
            return RolErrors.NoEncontrado;
        }

        if (await _rolRepository.ExisteNombreExcluyendoAsync(command.Nombre.Trim(), command.Id, cancellationToken))
        {
            return RolErrors.NombreYaExiste;
        }

        var resultado = rol.Actualizar(command.Nombre, command.Descripcion);
        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        if (rol.IsPrincipal && command.Activo != rol.Activo)
        {
            return RolErrors.RolPrincipalNoPermiteCambiarEstado;
        }

        if (command.Activo)
        {
            rol.Activar();
        }
        else
        {
            rol.Desactivar();
        }

        await _rolRepository.UpdateAsync(rol, cancellationToken);
        return Result.Success;
    }
}
