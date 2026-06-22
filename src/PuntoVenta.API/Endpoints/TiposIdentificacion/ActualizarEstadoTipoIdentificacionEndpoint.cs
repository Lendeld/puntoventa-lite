using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.TiposIdentificacion;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.TiposIdentificacion;

public sealed class ActualizarEstadoTipoIdentificacionEndpoint(IMediator mediator) : EndpointWithoutRequest<bool>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/tipos-identificacion/{id:guid}/estado");
        Tags("TiposIdentificacion");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.TiposIdentificacionToggle));
        Summary(s =>
        {
            s.Summary = "Activar / desactivar tipo de identificación";
            s.Description = "Cambia estado activo/inactivo del tipo de identificación";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ActualizarEstadoTipoIdentificacionCommand(id), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
