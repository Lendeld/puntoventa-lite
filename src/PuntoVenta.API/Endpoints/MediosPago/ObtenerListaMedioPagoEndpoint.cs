using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.MediosPago;
using PuntoVenta.Application.DTOs.MediosPago;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.MediosPago;

public sealed class ObtenerListaMedioPagoEndpoint(IMediator mediator) : Endpoint<ObtenerListaMedioPagoQuery, IReadOnlyList<MedioPagoDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/medios-pago");
        Tags("MediosPago");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.MediosPagoVer));
        Summary(s =>
        {
            s.Summary = "Obtener medios de pago";
            s.Description = "Retorna listado de medios de pago. Filtro opcional: Activo";
        });
    }

    public override async Task HandleAsync(ObtenerListaMedioPagoQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
