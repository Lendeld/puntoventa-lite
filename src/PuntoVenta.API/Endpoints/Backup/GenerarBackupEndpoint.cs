using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Backup.GenerarBackup;
using PuntoVenta.Application.DTOs.Backup;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Backup;

public sealed class GenerarBackupEndpoint(IMediator mediator) : Endpoint<GenerarBackupCommand, BackupGeneradoDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/backup/generar");
        Tags("Backup");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.BackupAdministrar));
        Summary(summary =>
        {
            summary.Summary = "Generar respaldo de la base de datos";
            summary.Description = "Genera una copia consistente del SQLite local usando VACUUM INTO. Requiere PIN del usuario y permiso backup:administrar.";
        });
    }

    public override async Task HandleAsync(GenerarBackupCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
