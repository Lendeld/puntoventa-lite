using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Cajas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Cajas;

public sealed class ToggleCajaEndpoint : Endpoint<ToggleCajaCommand, Success>
{
    private readonly IMediator _mediator;
    public ToggleCajaEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Patch("/cajas/{Id}/toggle");
        Tags("Cajas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CajasToggle));
    }

    public override async Task HandleAsync(ToggleCajaCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(ct);
    }
}
