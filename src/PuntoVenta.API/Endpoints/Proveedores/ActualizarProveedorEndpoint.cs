using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Proveedores;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Proveedores;

public sealed record ActualizarProveedorRequest(
    string Nombre,
    string? Correo = null,
    string? Telefono = null,
    string? Observacion = null,
    bool Activo = true);

public sealed class ActualizarProveedorEndpoint(IMediator mediator) : Endpoint<ActualizarProveedorRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/proveedores/{Id:guid}");
        Tags("Proveedores");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProveedoresEditar));
        Summary(s =>
        {
            s.Summary = "Actualizar proveedor";
            s.Description = "Actualiza los datos de un proveedor. Activo:false = desactivar (borrado lógico).";
        });
    }

    public override async Task HandleAsync(ActualizarProveedorRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("Id");
        var result = await _mediator.Send(
            new ActualizarProveedorCommand(id, req.Nombre, req.Correo, req.Telefono, req.Observacion, req.Activo), ct);

        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.NoContentAsync(ct);
    }
}
