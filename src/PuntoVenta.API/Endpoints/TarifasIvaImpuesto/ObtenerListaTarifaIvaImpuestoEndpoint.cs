using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.TarifasIvaImpuesto;
using PuntoVenta.Application.DTOs.TarifasIvaImpuesto;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.TarifasIvaImpuesto;

public sealed class ObtenerListaTarifaIvaImpuestoEndpoint(IMediator mediator) : Endpoint<ObtenerListaTarifaIvaImpuestoQuery, IReadOnlyList<TarifaIvaImpuestoDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/tarifas-iva");
        Tags("TarifasIvaImpuesto");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.TarifasIvaVer));
        Summary(s =>
        {
            s.Summary = "Obtener tarifas IVA";
            s.Description = "Retorna listado de tarifas IVA. Filtro opcional: Activo";
        });
    }

    public override async Task HandleAsync(ObtenerListaTarifaIvaImpuestoQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
