using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Usuarios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Usuarios;

public sealed class CrearUsuarioEndpoint(IMediator mediator) : Endpoint<CrearUsuarioCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/usuarios");
        Tags("Usuarios");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.UsuariosCrear));
        Summary(s =>
        {
            s.Summary = "Crear usuario";
            s.Description = "Crea un usuario con password temporal; debe cambiarla en el primer login";
        });
    }

    public override async Task HandleAsync(CrearUsuarioCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
