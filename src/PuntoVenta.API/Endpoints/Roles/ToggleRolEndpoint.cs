using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Roles;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Roles;

public sealed class ToggleRolEndpoint(IMediator mediator) : EndpointWithoutRequest<bool>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/roles/{id:guid}/toggle");
        Tags("Roles");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.RolesToggle));
        Summary(s =>
        {
            s.Summary = "Activar / desactivar rol";
            s.Description = "Cambia el estado activo/inactivo de un rol";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ToggleRolCommand(id), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
