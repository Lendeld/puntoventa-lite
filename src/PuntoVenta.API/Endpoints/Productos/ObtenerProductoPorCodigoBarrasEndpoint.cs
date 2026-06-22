using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Productos;
using PuntoVenta.Application.DTOs.Productos;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Productos;

public sealed class ObtenerProductoPorCodigoBarrasEndpoint(IMediator mediator) : EndpointWithoutRequest<ProductoDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/productos/codigo-barras/{codigo}");
        Tags("Productos");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProductosVer));
        Summary(s =>
        {
            s.Summary = "Obtener producto por código de barras";
            s.Description = "Busca un producto activo del negocio actual por su código de barras exacto. Usado por el flujo de pistola lectora en facturación.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var codigo = Route<string>("codigo") ?? string.Empty;
        var result = await _mediator.Send(new ObtenerProductoPorCodigoBarrasQuery(codigo), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
