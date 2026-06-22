using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Categorias;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Categorias;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Categorias;

public sealed class ObtenerListaPaginadoCategoriaEndpoint(IMediator mediator) : Endpoint<ObtenerListaPaginadoCategoriaQuery, PagedResult<CategoriaDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/categorias");
        Tags("Categorias");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CategoriasVer));
        Summary(s =>
        {
            s.Summary = "Obtener categorías paginadas";
            s.Description = "Filtros: Activo (nullable), FiltroDinamico, NumeroPagina, TamanoPagina.";
        });
    }

    public override async Task HandleAsync(ObtenerListaPaginadoCategoriaQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
