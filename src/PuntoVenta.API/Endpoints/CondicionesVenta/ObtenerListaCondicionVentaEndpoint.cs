using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.CondicionesVenta;
using PuntoVenta.Application.DTOs.CondicionesVenta;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.CondicionesVenta;

public sealed class ObtenerListaCondicionVentaEndpoint(IMediator mediator) : Endpoint<ObtenerListaCondicionVentaQuery, IReadOnlyList<CondicionVentaDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/condiciones-venta");
        Tags("CondicionesVenta");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CondicionesVentaVer));
        Summary(s =>
        {
            s.Summary = "Obtener condiciones de venta";
            s.Description = "Retorna listado de condiciones de venta. Filtro opcional: Activo";
        });
    }

    public override async Task HandleAsync(ObtenerListaCondicionVentaQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
