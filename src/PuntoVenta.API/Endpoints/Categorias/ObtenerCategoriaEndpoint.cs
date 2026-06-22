using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Categorias;
using PuntoVenta.Application.DTOs.Categorias;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Categorias;

public sealed class ObtenerCategoriaEndpoint(IMediator mediator) : EndpointWithoutRequest<CategoriaDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/categorias/{id:guid}");
        Tags("Categorias");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CategoriasVer));
        Summary(s =>
        {
            s.Summary = "Obtener categoría por Id";
            s.Description = "Retorna los datos de una categoría";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ObtenerCategoriaPorIdQuery(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
