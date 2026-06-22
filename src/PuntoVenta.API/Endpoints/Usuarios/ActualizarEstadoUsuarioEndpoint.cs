using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Usuarios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Usuarios;

public sealed class ActualizarEstadoUsuarioEndpoint(IMediator mediator) : EndpointWithoutRequest<bool>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/usuarios/{id:guid}/estado");
        Tags("Usuarios");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.UsuariosToggle));
        Summary(s =>
        {
            s.Summary = "Activar / desactivar usuario";
            s.Description = "Cambia el estado activo/inactivo del usuario";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ActualizarEstadoUsuarioCommand(id), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
