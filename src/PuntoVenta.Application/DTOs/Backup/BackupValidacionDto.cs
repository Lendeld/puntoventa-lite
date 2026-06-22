namespace PuntoVenta.Application.DTOs.Backup;

public sealed record BackupValidacionDto
{
    public bool EsCompatible { get; init; }
    public string VersionBackup { get; init; } = string.Empty;
    public string VersionApp { get; init; } = string.Empty;

    /// <summary>
    /// Token de capacidad de un solo uso, acuñado solo cuando el respaldo es compatible.
    /// Electron main debe presentarlo (atado a la ruta exacta) en POST /backup/consumir-token
    /// antes de reemplazar la base de datos. Cierra el bypass del swap nativo sin validación.
    /// </summary>
    public string TokenRestauracion { get; init; } = string.Empty;
}
