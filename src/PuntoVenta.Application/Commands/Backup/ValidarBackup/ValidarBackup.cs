using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common.Errors;
using PuntoVenta.Application.DTOs.Backup;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Commands.Backup.ValidarBackup;

public sealed record ValidarBackupCommand(string RutaBackup, string Pin)
    : IRequest<ErrorOr<BackupValidacionDto>>;

public sealed class ValidarBackupHandler(
    IUsuarioActual usuarioActual,
    IPinValidator pinValidator,
    IBackupService backupService,
    IRestoreTokenService restoreTokenService) : IRequestHandler<ValidarBackupCommand, ErrorOr<BackupValidacionDto>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IPinValidator _pinValidator = pinValidator;
    private readonly IBackupService _backupService = backupService;
    private readonly IRestoreTokenService _restoreTokenService = restoreTokenService;

    public async ValueTask<ErrorOr<BackupValidacionDto>> Handle(
        ValidarBackupCommand command,
        CancellationToken cancellationToken)
    {
        var validacion = await _pinValidator.ValidarAsync(_usuarioActual.UsuarioId, command.Pin, cancellationToken);
        if (validacion.IsError) return validacion.Errors;

        if (string.IsNullOrWhiteSpace(command.RutaBackup) || !Path.IsPathRooted(command.RutaBackup))
            return BackupErrors.RutaInvalida;

        var rutaCanonica = Path.GetFullPath(command.RutaBackup);

        var resultado = await _backupService.ValidarAsync(rutaCanonica, cancellationToken);
        if (resultado.IsError) return resultado.Errors;

        // Solo si el respaldo es válido y compatible se acuña el token de capacidad,
        // atado a la ruta canónica y a la huella del archivo validado, que Electron main
        // consumirá antes del swap. Si el archivo cambia luego, el token deja de servir.
        var huella = await _backupService.CalcularHuellaAsync(rutaCanonica, cancellationToken);
        var token = _restoreTokenService.Generar(rutaCanonica, huella);
        return resultado.Value with { TokenRestauracion = token };
    }
}
