using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class ObtenerReciboPagoPdfEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/ventas/{id:guid}/abonos/{pagoId:guid}/pdf");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.FacturacionVer));
        Summary(s => s.Summary = "Obtener PDF de recibo o anulación de abono");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var pagoId = Route<Guid>("pagoId");
        var result = await _mediator.Send(new ObtenerReciboPagoPdfQuery(id, pagoId), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }

        var dto = result.Value;
        HttpContext.Response.ContentType = dto.ContentType;
        HttpContext.Response.Headers.ContentDisposition = $"inline; filename=\"{dto.FileName}\"";
        HttpContext.Response.Headers.CacheControl = "no-store, must-revalidate";
        HttpContext.Response.Headers.Pragma = "no-cache";
        await HttpContext.Response.Body.WriteAsync(dto.Content, ct);
    }
}
