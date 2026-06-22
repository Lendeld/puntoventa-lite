using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Vendedores;
using PuntoVenta.Application.DTOs.Vendedores;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Vendedores;

public sealed class ObtenerVendedoresActivosEndpoint(IMediator mediator) : EndpointWithoutRequest<IReadOnlyList<VendedorActivoDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/vendedores/activos");
        Tags("Vendedores");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VendedoresVer));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObtenerVendedoresActivosQuery(), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
