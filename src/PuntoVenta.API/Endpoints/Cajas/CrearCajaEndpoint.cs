using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Cajas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Cajas;

public sealed class CrearCajaEndpoint(IMediator mediator) : Endpoint<CrearCajaCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/cajas");
        Tags("Cajas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CajasCrear));
        Summary(s =>
        {
            s.Summary = "Crear caja";
            s.Description = "Registra una nueva caja para el negocio actual.";
        });
    }

    public override async Task HandleAsync(CrearCajaCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
