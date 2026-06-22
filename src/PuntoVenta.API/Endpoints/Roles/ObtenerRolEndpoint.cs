using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Roles;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Roles;

public sealed class ObtenerRolEndpoint(IMediator mediator) : EndpointWithoutRequest<RolDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/roles/{id:guid}");
        Tags("Roles");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.RolesVer));
        Summary(s =>
        {
            s.Summary = "Obtener rol por Id";
            s.Description = "Retorna los datos de un rol específico";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ObtenerRolQuery(id), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
