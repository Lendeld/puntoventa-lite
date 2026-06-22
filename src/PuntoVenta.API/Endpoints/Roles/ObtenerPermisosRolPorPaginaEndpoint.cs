using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Roles;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Roles;

public sealed record ObtenerPermisosRolPorPaginaRequest(Guid PaginaId);

public sealed class ObtenerPermisosRolPorPaginaEndpoint(IMediator mediator) : Endpoint<ObtenerPermisosRolPorPaginaRequest, PermisosRolPorPaginaDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/roles/{rolId:guid}/permisos");
        Tags("Roles");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.RolesPermisosAdministrar));
        Summary(s =>
        {
            s.Summary = "Obtener permisos del rol por página";
            s.Description = "Retorna los permisos de una página con flag de asignación para el rol";
        });
    }

    public override async Task HandleAsync(ObtenerPermisosRolPorPaginaRequest req, CancellationToken ct)
    {
        var rolId = Route<Guid>("rolId");

        var result = await _mediator.Send(
            new ObtenerPermisosRolPorPaginaQuery(rolId, req.PaginaId), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
