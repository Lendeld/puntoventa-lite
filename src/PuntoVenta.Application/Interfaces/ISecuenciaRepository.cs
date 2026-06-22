using PuntoVenta.Domain.Entities.Secuencias;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Interfaces;

public interface ISecuenciaRepository : IRepository<Secuencia>
{
    /// <summary>
    /// Devuelve la secuencia editable (tracked) para el tipo indicado.
    /// Si no existe, la crea y persiste en la misma transacción.
    /// </summary>
    Task<Secuencia> ObtenerOCrearEditableAsync(
        TipoDocumentoVenta tipoDocumento,
        CancellationToken cancellationToken = default);
}
