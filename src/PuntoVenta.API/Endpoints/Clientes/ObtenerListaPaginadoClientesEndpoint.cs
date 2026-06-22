using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Clientes;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Clientes;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Clientes;

public sealed class ObtenerListaPaginadoClientesEndpoint(IMediator mediator) : Endpoint<ObtenerListaPaginadoClientesQuery, PagedResult<ClienteListaDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/clientes");
        Tags("Clientes");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ClientesVer));
        Summary(s =>
        {
            s.Summary = "Obtener clientes";
            s.Description = "Obtiene el listado paginado de clientes.";
        });
    }

    public override async Task HandleAsync(ObtenerListaPaginadoClientesQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
