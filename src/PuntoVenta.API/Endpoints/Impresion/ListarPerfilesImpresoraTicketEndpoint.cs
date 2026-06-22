using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Impresion;
using PuntoVenta.Application.DTOs.Impresion;

namespace PuntoVenta.API.Endpoints.Impresion;

public sealed class ListarPerfilesImpresoraTicketEndpoint(IMediator mediator)
    : EndpointWithoutRequest<IReadOnlyList<PerfilImpresoraTicketDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/impresion/perfiles");
        Tags("Impresion");
        Options(b => b.RequireAuthorization());
        Summary(s =>
        {
            s.Summary = "Listar perfiles de impresora de tickets";
            s.Description = "Perfiles activos (ancho, chars por línea, codepage, gaveta, corte) para configurar la impresión local";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ListarPerfilesImpresoraTicketQuery(), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 200, ct);
    }
}
