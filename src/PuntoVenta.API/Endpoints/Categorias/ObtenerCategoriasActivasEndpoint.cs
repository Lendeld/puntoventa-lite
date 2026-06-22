using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Categorias;
using PuntoVenta.Application.DTOs.Categorias;

namespace PuntoVenta.API.Endpoints.Categorias;

public sealed class ObtenerCategoriasActivasEndpoint(IMediator mediator) : EndpointWithoutRequest<IReadOnlyList<CategoriaDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/categorias/activos");
        Tags("Categorias");
        Options(b => b.RequireAuthorization());
        Summary(s =>
        {
            s.Summary = "Obtener categorías activas";
            s.Description = "Retorna listado de categorías activas";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObtenerCategoriasActivasQuery(), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
