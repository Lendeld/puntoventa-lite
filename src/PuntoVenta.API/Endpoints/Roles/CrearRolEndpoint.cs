using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Roles;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Roles;

public sealed class CrearRolEndpoint(IMediator mediator) : Endpoint<CrearRolCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/roles");
        Tags("Roles");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.RolesCrear));
        Summary(s =>
        {
            s.Summary = "Crear rol";
            s.Description = "Registra un nuevo rol en el sistema";
        });
    }

    public override async Task HandleAsync(CrearRolCommand req, CancellationToken ct)
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
