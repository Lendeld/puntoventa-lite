using PuntoVenta.Application.DTOs.Ventas;

namespace PuntoVenta.Application.Interfaces;

public interface IReporteVentasExcelService
{
    // Serializa a .xlsx el resultado YA resuelto del handler de datos (colonización,
    // signo NC y agregación ya aplicados). El servicio solo formatea, no calcula.
    byte[] Generar(ReporteVentasRangoResultadoDto resultado, DateTime fechaDesdeUtc, DateTime fechaHastaUtc);
}
