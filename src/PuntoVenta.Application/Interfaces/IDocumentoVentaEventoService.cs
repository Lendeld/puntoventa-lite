using ErrorOr;

namespace PuntoVenta.Application.Interfaces;

public interface IDocumentoVentaEventoService
{
    /// <summary>
    /// Encola un evento de historia para el documento. NO persiste — el SaveChanges
    /// del handler caller commit-ea el evento junto con el cambio de negocio.
    /// </summary>
    Task<ErrorOr<Success>> RegistrarAsync(
        Guid documentoVentaId,
        string tipoEventoCodigo,
        string resumen,
        object? payload = null,
        Guid? correlacionId = null,
        DateTime? ocurridoEn = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Variante para procesos de sistema (workers/jobs) sin contexto de usuario.
    /// El caller pasa negocioId explícito. NO persiste — el caller commit-ea
    /// en su propio SaveChanges/transacción.
    /// </summary>
    Task<ErrorOr<Success>> RegistrarSistemaAsync(
        Guid negocioId,
        Guid documentoVentaId,
        string tipoEventoCodigo,
        string resumen,
        object? payload = null,
        Guid? correlacionId = null,
        DateTime? ocurridoEn = null,
        CancellationToken cancellationToken = default);
}
