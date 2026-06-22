using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Roles;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Roles;

public sealed class ObtenerListaPaginadoRolEndpoint(IMediator mediator) : Endpoint<ObtenerListaPaginadoRolQuery, PagedResult<RolDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/roles");
        Tags("Roles");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.RolesVer));
        Summary(s =>
        {
            s.Summary = "Obtener lista paginada de roles";
            s.Description = "Retorna los roles del sistema con paginación. Parámetros: pagina (default 1), tamano (default 10, máximo 100)";
        });
    }

    public override async Task HandleAsync(ObtenerListaPaginadoRolQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
