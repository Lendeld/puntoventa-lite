using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Clientes;
using PuntoVenta.Application.DTOs.Clientes;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Clientes;

public sealed class ObtenerClienteEndpoint(IMediator mediator) : EndpointWithoutRequest<ClienteDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/clientes/{id}");
        Tags("Clientes");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ClientesVer));
        Summary(s =>
        {
            s.Summary = "Obtener cliente";
            s.Description = "Obtiene el detalle de un cliente.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ObtenerClientePorIdQuery(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
