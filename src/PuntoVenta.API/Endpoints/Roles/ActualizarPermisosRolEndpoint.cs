using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Roles;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Roles;

public sealed record ActualizarPermisosRolRequest(Guid PaginaId, IReadOnlyList<Guid> PermisosIds);

public sealed class ActualizarPermisosRolEndpoint(IMediator mediator) : Endpoint<ActualizarPermisosRolRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/roles/{rolId:guid}/permisos");
        Tags("Roles");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.RolesPermisosAdministrar));
        Summary(s =>
        {
            s.Summary = "Actualizar permisos del rol por página";
            s.Description = "Reemplaza los permisos asignados al rol para la página indicada";
        });
    }

    public override async Task HandleAsync(ActualizarPermisosRolRequest req, CancellationToken ct)
    {
        var rolId = Route<Guid>("rolId");

        var command = new ActualizarPermisosRolCommand(rolId, req.PaginaId, req.PermisosIds);
        var result  = await _mediator.Send(command, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.NoContentAsync();
    }
}
