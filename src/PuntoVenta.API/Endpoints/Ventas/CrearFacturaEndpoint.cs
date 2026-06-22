using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class CrearFacturaEndpoint(IMediator mediator) : Endpoint<CrearFacturaCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/ventas/facturas");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.FacturacionVer));
        Summary(s =>
        {
            s.Summary = "Crear y emitir factura";
            s.Description = "Crea una factura completa y la emite en una sola operación.";
        });
    }

    public override async Task HandleAsync(CrearFacturaCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
