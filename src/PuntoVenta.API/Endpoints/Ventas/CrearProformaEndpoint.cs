using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class CrearProformaEndpoint(IMediator mediator) : Endpoint<CrearProformaCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/ventas/proformas");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.FacturacionVer));
        Summary(s =>
        {
            s.Summary = "Crear proforma";
            s.Description = "Crea una proforma editable sin pagos y con consecutivo propio.";
        });
    }

    public override async Task HandleAsync(CrearProformaCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
