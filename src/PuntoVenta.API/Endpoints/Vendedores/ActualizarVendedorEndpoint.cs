using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Vendedores;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Vendedores;

public sealed record ActualizarVendedorRequest(
    string Nombre,
    bool IsPrincipal = false,
    bool Activo = true);

public sealed class ActualizarVendedorEndpoint(IMediator mediator) : Endpoint<ActualizarVendedorRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/vendedores/{id:guid}");
        Tags("Vendedores");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VendedoresEditar));
    }

    public override async Task HandleAsync(ActualizarVendedorRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(
            new ActualizarVendedorCommand(id, req.Nombre, req.IsPrincipal, req.Activo), ct);

        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.NoContentAsync(ct);
    }
}
