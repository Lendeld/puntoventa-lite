using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Proveedores;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Proveedores;

public sealed class CrearProveedorEndpoint(IMediator mediator) : Endpoint<CrearProveedorCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/proveedores");
        Tags("Proveedores");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProveedoresCrear));
        Summary(s =>
        {
            s.Summary = "Crear proveedor";
            s.Description = "Registra un nuevo proveedor";
        });
    }

    public override async Task HandleAsync(CrearProveedorCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
