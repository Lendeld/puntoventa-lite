using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Proveedores;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Proveedores;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Proveedores;

public sealed class ObtenerListaPaginadoProveedorEndpoint(IMediator mediator) : Endpoint<ObtenerListaPaginadoProveedorQuery, PagedResult<ProveedorDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/proveedores");
        Tags("Proveedores");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProveedoresVer));
        Summary(s =>
        {
            s.Summary = "Obtener proveedores paginados";
            s.Description = "Filtros: Activo (nullable), FiltroDinamico, NumeroPagina, TamanoPagina.";
        });
    }

    public override async Task HandleAsync(ObtenerListaPaginadoProveedorQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
