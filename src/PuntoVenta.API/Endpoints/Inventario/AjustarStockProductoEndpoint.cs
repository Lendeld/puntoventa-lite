using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Inventario;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Inventario;

public sealed class AjustarStockProductoEndpoint(IMediator mediator) : Endpoint<AjustarStockProductoCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/inventario/ajuste-stock");
        Tags("Inventario");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProductosAjustarStock));
        Summary(s => s.Summary = "Registrar ajuste manual de stock de un producto");
    }

    public override async Task HandleAsync(AjustarStockProductoCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
