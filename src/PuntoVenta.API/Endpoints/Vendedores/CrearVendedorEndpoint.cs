using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Vendedores;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Vendedores;

public sealed class CrearVendedorEndpoint(IMediator mediator) : Endpoint<CrearVendedorCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/vendedores");
        Tags("Vendedores");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VendedoresCrear));
    }

    public override async Task HandleAsync(CrearVendedorCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
