using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Clientes;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Clientes;

public sealed class ActualizarClienteEndpoint(IMediator mediator) : Endpoint<ActualizarClienteCommand>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/clientes/{id}");
        Tags("Clientes");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ClientesEditar));
        Summary(s =>
        {
            s.Summary = "Actualizar cliente";
            s.Description = "Actualiza un cliente existente.";
        });
    }

    public override async Task HandleAsync(ActualizarClienteCommand req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = req with { Id = id };
        var result = await _mediator.Send(command, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.NoContentAsync(ct);
    }
}
