using PuntoVenta.Application.DTOs.Inventarios;

namespace PuntoVenta.Application.Interfaces;

public interface IReporteInventarioExcelService
{
    // Serializa a .xlsx el resultado ya resuelto del handler de datos.
    // El servicio solo formatea, no calcula.
    byte[] Generar(ReporteInventarioResultadoDto resultado);
}
