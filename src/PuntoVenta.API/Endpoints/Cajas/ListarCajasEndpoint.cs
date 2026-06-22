using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Queries.Cajas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Cajas;

public sealed class ListarCajasEndpoint(IMediator mediator) : EndpointWithoutRequest<IReadOnlyList<CajaListadoItem>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/cajas");
        Tags("Cajas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CajasVer));
        Summary(s =>
        {
            s.Summary = "Listar cajas";
            s.Description = "Devuelve las cajas del negocio actual.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ListarCajasQuery(), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 200, ct);
    }
}
