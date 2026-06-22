using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Categorias;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Categorias;

public sealed record ActualizarCategoriaRequest(string Nombre, string? Descripcion = null, bool Activo = true);

public sealed class ActualizarCategoriaEndpoint(IMediator mediator) : Endpoint<ActualizarCategoriaRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/categorias/{id:guid}");
        Tags("Categorias");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CategoriasEditar));
        Summary(s =>
        {
            s.Summary = "Actualizar categoría";
            s.Description = "Actualiza los datos de una categoría";
        });
    }

    public override async Task HandleAsync(ActualizarCategoriaRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(
            new ActualizarCategoriaCommand(id, req.Nombre, req.Descripcion, req.Activo), ct);

        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.NoContentAsync(ct);
    }
}
