using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class ObtenerDocumentoVentaPdfEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/ventas/{id:guid}/pdf");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.FacturacionVer));
        Summary(s => s.Summary = "Obtener PDF de documento de venta");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(
            new ObtenerDocumentoVentaPdfQuery(id),
            ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        HttpContext.Response.ContentType = result.Value.ContentType;
        HttpContext.Response.Headers.ContentDisposition = $"inline; filename=\"{result.Value.FileName}\"";
        HttpContext.Response.Headers.CacheControl = "no-store, must-revalidate";
        HttpContext.Response.Headers.Pragma = "no-cache";
        await HttpContext.Response.Body.WriteAsync(result.Value.Content, ct);
    }
}
