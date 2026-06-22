using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.CodigosImpuesto;
using PuntoVenta.Application.DTOs.CodigosImpuesto;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.CodigosImpuesto;

public sealed class ObtenerListaCodigoImpuestoEndpoint(IMediator mediator) : Endpoint<ObtenerListaCodigoImpuestoQuery, IReadOnlyList<CodigoImpuestoDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/codigos-impuesto");
        Tags("CodigosImpuesto");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CodigosImpuestoVer));
        Summary(s =>
        {
            s.Summary = "Obtener códigos de impuesto";
            s.Description = "Retorna listado de códigos de impuesto. Filtro opcional: Activo";
        });
    }

    public override async Task HandleAsync(ObtenerListaCodigoImpuestoQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
