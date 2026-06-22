using ErrorOr;

namespace PuntoVenta.Application.Common.Errors;

public static class BackupErrors
{
    public static Error ArchivoInvalido =>
        Error.Validation("Backup_ArchivoInvalido", "El archivo no es un respaldo SQLite válido o no contiene historial de migraciones.");

    public static Error VersionIncompatible =>
        Error.Conflict("Backup_VersionIncompatible", "El respaldo fue creado con otra versión del sistema y no puede restaurarse.");

    public static Error FalloAlGenerar =>
        Error.Failure("Backup_FalloGenerar", "No se pudo generar el respaldo. Verifique la ruta de destino y vuelva a intentarlo.");

    public static Error RutaInvalida =>
        Error.Validation("Backup_RutaInvalida", "La ruta proporcionada no es válida. Debe ser una ruta absoluta.");

    public static Error TokenInvalido =>
        Error.Unauthorized("Backup_TokenInvalido", "Autorización de restauración inválida, expirada o ya utilizada. Vuelva a validar el respaldo.");
}
