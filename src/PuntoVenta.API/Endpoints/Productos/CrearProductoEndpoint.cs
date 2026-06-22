using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Productos;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Productos;

public sealed class CrearProductoEndpoint(IMediator mediator) : Endpoint<CrearProductoCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/productos");
        Tags("Productos");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProductosCrear));
        Summary(s =>
        {
            s.Summary = "Crear producto";
            s.Description = "Registra un nuevo producto o servicio en el catálogo.";
        });
    }

    public override async Task HandleAsync(CrearProductoCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
