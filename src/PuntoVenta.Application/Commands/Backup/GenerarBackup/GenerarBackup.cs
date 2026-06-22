using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common.Errors;
using PuntoVenta.Application.DTOs.Backup;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Commands.Backup.GenerarBackup;

public sealed record GenerarBackupCommand(string Pin, string RutaDestino)
    : IRequest<ErrorOr<BackupGeneradoDto>>;

public sealed class GenerarBackupHandler(
    IUsuarioActual usuarioActual,
    IPinValidator pinValidator,
    IBackupService backupService) : IRequestHandler<GenerarBackupCommand, ErrorOr<BackupGeneradoDto>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IPinValidator _pinValidator = pinValidator;
    private readonly IBackupService _backupService = backupService;

    public async ValueTask<ErrorOr<BackupGeneradoDto>> Handle(
        GenerarBackupCommand command,
        CancellationToken cancellationToken)
    {
        var validacion = await _pinValidator.ValidarAsync(_usuarioActual.UsuarioId, command.Pin, cancellationToken);
        if (validacion.IsError) return validacion.Errors;

        if (string.IsNullOrWhiteSpace(command.RutaDestino) || !Path.IsPathRooted(command.RutaDestino))
            return BackupErrors.RutaInvalida;

        var rutaCanonica = Path.GetFullPath(command.RutaDestino);

        return await _backupService.GenerarAsync(rutaCanonica, cancellationToken);
    }
}
