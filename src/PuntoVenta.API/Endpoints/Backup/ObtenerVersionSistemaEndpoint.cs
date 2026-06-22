using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Queries.Backup.ObtenerVersionSistema;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Backup;

public sealed class ObtenerVersionSistemaEndpoint(IMediator mediator) : EndpointWithoutRequest<string>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/backup/version-sistema");
        Tags("Backup");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.BackupAdministrar));
        Summary(summary =>
        {
            summary.Summary = "Obtener versión del sistema";
            summary.Description = "Devuelve la versión del sistema (legible) para mostrar en la pantalla de respaldo.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObtenerVersionSistemaQuery(), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
