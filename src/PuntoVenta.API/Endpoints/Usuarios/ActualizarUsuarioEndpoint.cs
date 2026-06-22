using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Usuarios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Usuarios;

public sealed class ActualizarUsuarioEndpoint(IMediator mediator) : Endpoint<ActualizarUsuarioCommand>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/usuarios/{id:guid}");
        Tags("Usuarios");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.UsuariosEditar));
        Summary(s =>
        {
            s.Summary = "Actualizar acceso del usuario";
            s.Description = "Modifica el rol y estado del usuario dentro del negocio activo";
        });
    }

    public override async Task HandleAsync(ActualizarUsuarioCommand req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = req with { Id = id };

        var result = await _mediator.Send(command, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
