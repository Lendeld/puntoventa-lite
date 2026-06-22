using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Cajas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Cajas;

public sealed class ActualizarCajaEndpoint : Endpoint<ActualizarCajaCommand, Success>
{
    private readonly IMediator _mediator;
    public ActualizarCajaEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/cajas/{Id}");
        Tags("Cajas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CajasEditar));
    }

    public override async Task HandleAsync(ActualizarCajaCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(ct);
    }
}
