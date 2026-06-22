using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Clientes;
using PuntoVenta.Application.DTOs.Clientes;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Clientes;

public sealed class ObtenerSaldoCreditoClienteEndpoint(IMediator mediator) : EndpointWithoutRequest<SaldoCreditoClienteDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/clientes/{id:guid}/saldo-credito");
        Tags("Clientes");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ClientesVer));
        Summary(s => s.Summary = "Obtener saldo y estado de crédito del cliente");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ObtenerSaldoCreditoClienteQuery(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
