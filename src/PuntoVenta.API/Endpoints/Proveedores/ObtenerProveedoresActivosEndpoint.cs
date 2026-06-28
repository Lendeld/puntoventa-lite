using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Proveedores;
using PuntoVenta.Application.DTOs.Proveedores;

namespace PuntoVenta.API.Endpoints.Proveedores;

public sealed class ObtenerProveedoresActivosEndpoint(IMediator mediator) : EndpointWithoutRequest<IReadOnlyList<ProveedorDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/proveedores/activos");
        Tags("Proveedores");
        Options(b => b.RequireAuthorization());
        Summary(s =>
        {
            s.Summary = "Obtener proveedores activos";
            s.Description = "Retorna listado de proveedores activos (para Selects)";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObtenerProveedoresActivosQuery(), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
