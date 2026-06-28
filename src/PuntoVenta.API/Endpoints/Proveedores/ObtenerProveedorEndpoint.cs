using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Proveedores;
using PuntoVenta.Application.DTOs.Proveedores;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Proveedores;

public sealed class ObtenerProveedorEndpoint(IMediator mediator) : EndpointWithoutRequest<ProveedorDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/proveedores/{id:guid}");
        Tags("Proveedores");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProveedoresVer));
        Summary(s =>
        {
            s.Summary = "Obtener proveedor por Id";
            s.Description = "Retorna los datos de un proveedor";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ObtenerProveedorPorIdQuery(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
