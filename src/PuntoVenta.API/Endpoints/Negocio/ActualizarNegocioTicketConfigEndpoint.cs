using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Negocios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Negocio;

public sealed class ActualizarNegocioTicketConfigEndpoint(IMediator mediator) : Endpoint<ActualizarNegocioTicketConfigCommand>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/negocio/ticket-config");
        Tags("Negocio");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.NegocioEditar));
        Summary(s => s.Summary = "Actualizar configuración de ticket del negocio");
    }

    public override async Task HandleAsync(ActualizarNegocioTicketConfigCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }
        await Send.NoContentAsync(ct);
    }
}
