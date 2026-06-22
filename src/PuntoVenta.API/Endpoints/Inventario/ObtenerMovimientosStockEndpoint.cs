using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Queries.Inventario;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Inventario;

public sealed class ObtenerMovimientosStockEndpoint(IMediator mediator) : EndpointWithoutRequest<ObtenerMovimientosStockResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/inventario/movimientos-stock");
        Tags("Inventario");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProductosMovimientosVer));
        Summary(s => s.Summary = "Bandeja de movimientos de stock");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var productoId = Query<Guid?>("productoId", isRequired: false);
        var pagina = Query<int>("pagina", isRequired: false);
        var tamano = Query<int>("tamano", isRequired: false);

        var query = new ObtenerMovimientosStockQuery(
            productoId,
            pagina > 0 ? pagina : 1,
            tamano > 0 ? tamano : 20);

        var result = await _mediator.Send(query, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 200, ct);
    }
}
