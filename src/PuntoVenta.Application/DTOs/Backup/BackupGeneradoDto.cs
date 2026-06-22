namespace PuntoVenta.Application.DTOs.Backup;

public sealed record BackupGeneradoDto
{
    public string RutaArchivo { get; init; } = string.Empty;
    public string VersionEsquema { get; init; } = string.Empty;
    public DateTime FechaUtc { get; init; }
    public string? AppVersion { get; init; }
}
