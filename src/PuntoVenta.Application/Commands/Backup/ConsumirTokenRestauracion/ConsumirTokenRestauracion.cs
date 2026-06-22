using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common.Errors;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Commands.Backup.ConsumirTokenRestauracion;

public sealed record ConsumirTokenRestauracionCommand(string Token, string Ruta)
    : IRequest<ErrorOr<Success>>;

/// <summary>
/// Consume el token de capacidad de restauración. Lo invoca Electron main (sin JWT,
/// el token ES la credencial) justo antes de reemplazar la base de datos. El token
/// debe existir, no haber expirado y corresponder exactamente a la ruta indicada.
/// </summary>
public sealed class ConsumirTokenRestauracionHandler(
    IRestoreTokenService restoreTokenService,
    IBackupService backupService)
    : IRequestHandler<ConsumirTokenRestauracionCommand, ErrorOr<Success>>
{
    private readonly IRestoreTokenService _restoreTokenService = restoreTokenService;
    private readonly IBackupService _backupService = backupService;

    public async ValueTask<ErrorOr<Success>> Handle(
        ConsumirTokenRestauracionCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token)
            || string.IsNullOrWhiteSpace(command.Ruta)
            || !Path.IsPathRooted(command.Ruta))
        {
            return BackupErrors.TokenInvalido;
        }

        var rutaCanonica = Path.GetFullPath(command.Ruta);

        // Recomputa la huella del archivo en el momento del consumo: si cambió respecto
        // a lo validado (o el archivo ya no existe → huella vacía), el token no coincide.
        var huella = await _backupService.CalcularHuellaAsync(rutaCanonica, cancellationToken);

        return _restoreTokenService.Consumir(command.Token, rutaCanonica, huella)
            ? Result.Success
            : BackupErrors.TokenInvalido;
    }
}
