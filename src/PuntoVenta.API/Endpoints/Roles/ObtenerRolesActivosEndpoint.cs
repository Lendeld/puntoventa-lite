using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Roles;
using PuntoVenta.Application.DTOs.Roles;

namespace PuntoVenta.API.Endpoints.Roles;

public sealed class ObtenerRolesActivosEndpoint(IMediator mediator) : EndpointWithoutRequest<IReadOnlyList<RolDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/roles/activos");
        Tags("Roles");
        Options(b => b.RequireAuthorization());
        Summary(s =>
        {
            s.Summary = "Obtener roles activos";
            s.Description = "Retorna listado de roles activos del sistema";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObtenerRolesActivosQuery(), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
