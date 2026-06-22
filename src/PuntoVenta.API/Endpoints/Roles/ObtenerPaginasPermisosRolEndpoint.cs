using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Roles;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Roles;

public sealed class ObtenerPaginasPermisosRolEndpoint(IMediator mediator) : EndpointWithoutRequest<IReadOnlyList<PaginaPermisosRolTabDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/roles/{rolId:guid}/permisos/paginas");
        Tags("Roles");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.RolesPermisosAdministrar));
        Summary(s =>
        {
            s.Summary = "Obtener páginas con permisos configurables del rol";
            s.Description = "Retorna páginas activas con permisos activos para construir tabs por rol";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var rolId = Route<Guid>("rolId");

        var result = await _mediator.Send(
            new ObtenerPaginasPermisosRolQuery(rolId), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
