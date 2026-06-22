using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Backup.ValidarBackup;
using PuntoVenta.Application.DTOs.Backup;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Backup;

public sealed class ValidarBackupEndpoint(IMediator mediator) : Endpoint<ValidarBackupCommand, BackupValidacionDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/backup/validar");
        Tags("Backup");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.BackupAdministrar));
        Summary(summary =>
        {
            summary.Summary = "Validar archivo de respaldo";
            summary.Description = "Valida que el archivo .db indicado sea un SQLite válido compatible con la versión de esquema actual. Requiere PIN del usuario.";
        });
    }

    public override async Task HandleAsync(ValidarBackupCommand req, CancellationToken ct)
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
