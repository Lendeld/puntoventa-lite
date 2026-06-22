using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class ObtenerListaFacturasCreditoEndpoint(IMediator mediator) : Endpoint<ObtenerListaFacturasCreditoRequest, PagedResult<FacturaCreditoResumenDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/ventas/credito");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VentasCreditoVer));
        Summary(s => s.Summary = "Listar facturas a crédito (cuentas por cobrar)");
    }

    public override async Task HandleAsync(ObtenerListaFacturasCreditoRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ObtenerListaFacturasCreditoQuery(req.Pagina, req.Tamano, req.Filtro, req.ClienteId, req.SoloVencidas),
            ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}

public sealed record ObtenerListaFacturasCreditoRequest(
    int Pagina = 1,
    int Tamano = 20,
    string? Filtro = null,
    Guid? ClienteId = null,
    bool? SoloVencidas = null);
