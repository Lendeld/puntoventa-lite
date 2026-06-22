using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class ObtenerTicketDataEndpoint(IMediator mediator) : EndpointWithoutRequest<TicketDataDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/ventas/{id:guid}/ticket-data");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.FacturacionVer));
        Summary(s =>
        {
            s.Summary = "Obtener datos estructurados del ticket de una venta";
            s.Description = "Devuelve la información lista para renderizar en agente local o vista web.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        Guid? pagoId = null;
        var rawPagoId = Query<string>("pagoId", isRequired: false);
        if (!string.IsNullOrWhiteSpace(rawPagoId))
        {
            if (!Guid.TryParse(rawPagoId, out var parsed))
            {
                await Send.ResultAsync(new List<Error>
                {
                    Error.Validation("Ticket_PagoId", $"El valor '{rawPagoId}' no es un identificador válido."),
                }.ToProblem());
                return;
            }
            pagoId = parsed;
        }

        var result = await _mediator.Send(new ObtenerTicketDataQuery(id, pagoId), ct);
        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }
        await Send.OkAsync(result.Value, ct);
    }
}
