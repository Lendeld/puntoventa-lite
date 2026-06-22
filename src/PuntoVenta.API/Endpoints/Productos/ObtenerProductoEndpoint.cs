using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Productos;
using PuntoVenta.Application.DTOs.Productos;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Productos;

public sealed class ObtenerProductoEndpoint(IMediator mediator) : EndpointWithoutRequest<ProductoDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/productos/{id:guid}");
        Tags("Productos");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProductosVer));
        Summary(s =>
        {
            s.Summary = "Obtener producto por Id";
            s.Description = "Retorna los datos de un producto o servicio.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ObtenerProductoPorIdQuery(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
