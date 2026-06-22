using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.Interfaces;

public interface IReporteMovimientosDineroPdfService
{
    Task<byte[]> GenerarAsync(
        ReporteMovimientosDineroPdfData data,
        CancellationToken cancellationToken = default);
}

public sealed record ReporteMovimientosDineroPdfData(
    Negocio Negocio,
    DateTime FechaDesdeUtc,
    DateTime FechaHastaUtc,
    string? CajaCodigo,
    string? CajaNombre,
    ReporteMovimientosDineroResultadoDto Reporte);
