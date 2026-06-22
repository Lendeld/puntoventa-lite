using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Productos;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Productos;

public sealed record ActualizarProductoRequest(
    string Codigo,
    string Nombre,
    TipoItem TipoItem,
    decimal PrecioUnitario,
    string? CodigoBarras = null,
    string? Descripcion = null,
    string? ImagenUrl = null,
    decimal? PrecioCosto = null,
    Guid? CategoriaId = null,
    string? TarifaIvaImpuestoCodigo = null,
    bool? NoAplicaExistencias = null,
    bool? PermiteModificarPrecioUnitario = null);

public sealed class ActualizarProductoEndpoint(IMediator mediator) : Endpoint<ActualizarProductoRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/productos/{id:guid}");
        Tags("Productos");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProductosEditar));
        Summary(s =>
        {
            s.Summary = "Actualizar producto";
            s.Description = "Actualiza los datos de un producto o servicio.";
        });
    }

    public override async Task HandleAsync(ActualizarProductoRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(
            new ActualizarProductoCommand(
                id, req.Codigo, req.Nombre, req.TipoItem, req.PrecioUnitario,
                req.CodigoBarras, req.Descripcion, req.ImagenUrl, req.PrecioCosto,
                req.CategoriaId, req.TarifaIvaImpuestoCodigo,
                req.NoAplicaExistencias, req.PermiteModificarPrecioUnitario), ct);

        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.NoContentAsync(ct);
    }
}
