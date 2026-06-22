using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

internal static class DocumentoVentaDateTimeNormalizer
{
    public static DateTime? NormalizeToUtc(DateTime? value) =>
        value switch
        {
            null => null,
            { Kind: DateTimeKind.Utc } date => date,
            { Kind: DateTimeKind.Local } date => date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
}

public sealed class ObtenerListaPaginadaDocumentosVentaEndpoint(IMediator mediator) : Endpoint<ObtenerListaPaginadaDocumentosVentaQuery, PagedResult<DocumentoVentaResumenDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/ventas");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.FacturacionVer));
        Summary(s => s.Summary = "Obtener documentos de venta");
    }

    public override async Task HandleAsync(ObtenerListaPaginadaDocumentosVentaQuery req, CancellationToken ct)
    {
        var query = req with
        {
            FechaDesde = DocumentoVentaDateTimeNormalizer.NormalizeToUtc(req.FechaDesde),
            FechaHasta = DocumentoVentaDateTimeNormalizer.NormalizeToUtc(req.FechaHasta),
        };

        var result = await _mediator.Send(query, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
