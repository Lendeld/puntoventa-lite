using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Categorias;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Categorias;

public sealed class ActualizarEstadoCategoriaEndpoint(IMediator mediator) : EndpointWithoutRequest<bool>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/categorias/{id:guid}/estado");
        Tags("Categorias");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CategoriasToggle));
        Summary(s =>
        {
            s.Summary = "Activar / desactivar categoría";
            s.Description = "Cambia el estado activo/inactivo de la categoría";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ActualizarEstadoCategoriaCommand(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
