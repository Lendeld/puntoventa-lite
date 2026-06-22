using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Backup.ConsumirTokenRestauracion;

namespace PuntoVenta.API.Endpoints.Backup;

/// <summary>
/// Consume el token de capacidad de restauración. AllowAnonymous a propósito: lo llama
/// Electron main, que no tiene JWT de usuario. El token (256 bits, un solo uso, atado a
/// ruta, expira pronto) es la credencial; solo se acuña tras una validación autenticada
/// con PIN + permiso en /backup/validar.
/// </summary>
public sealed class ConsumirTokenRestauracionEndpoint(IMediator mediator)
    : Endpoint<ConsumirTokenRestauracionCommand>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/backup/consumir-token");
        Tags("Backup");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Consumir token de restauración";
            summary.Description = "Valida y consume el token de capacidad acuñado en /backup/validar. Lo invoca Electron main antes del swap del archivo SQLite.";
        });
    }

    public override async Task HandleAsync(ConsumirTokenRestauracionCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
