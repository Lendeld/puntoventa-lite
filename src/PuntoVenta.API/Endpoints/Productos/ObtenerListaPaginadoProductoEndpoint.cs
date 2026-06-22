using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Productos;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Productos;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Productos;

public sealed class ObtenerListaPaginadoProductoEndpoint(IMediator mediator) : Endpoint<ObtenerListaPaginadoProductoQuery, PagedResult<ProductoDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/productos");
        Tags("Productos");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProductosVer));
        Summary(s =>
        {
            s.Summary = "Obtener productos paginados";
            s.Description = "Filtros: TipoItem, CategoriaId, FiltroDinamico (Codigo, Nombre, CodigoBarras), NumeroPagina, TamanoPagina.";
        });
    }

    public override async Task HandleAsync(ObtenerListaPaginadoProductoQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
