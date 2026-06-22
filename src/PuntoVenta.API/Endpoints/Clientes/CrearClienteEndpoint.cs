using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Clientes;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Clientes;

public sealed class CrearClienteEndpoint(IMediator mediator) : Endpoint<CrearClienteCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/clientes");
        Tags("Clientes");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ClientesCrear));
        Summary(s =>
        {
            s.Summary = "Crear cliente";
            s.Description = "Registra un nuevo cliente.";
        });
    }

    public override async Task HandleAsync(CrearClienteCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
