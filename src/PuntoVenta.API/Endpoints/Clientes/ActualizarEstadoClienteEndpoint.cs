using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Clientes;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Clientes;

public sealed class ActualizarEstadoClienteEndpoint(IMediator mediator) : EndpointWithoutRequest<bool>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/clientes/{id}/estado");
        Tags("Clientes");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ClientesToggle));
        Summary(s =>
        {
            s.Summary = "Actualizar estado del cliente";
            s.Description = "Activa o desactiva un cliente.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ActualizarEstadoClienteCommand(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
