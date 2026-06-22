using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Vendedores;
using PuntoVenta.Application.DTOs.Vendedores;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Vendedores;

public sealed class ObtenerVendedorEndpoint(IMediator mediator) : EndpointWithoutRequest<VendedorDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/vendedores/{id:guid}");
        Tags("Vendedores");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VendedoresVer));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ObtenerVendedorPorIdQuery(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
