using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Inventarios;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Common;

namespace PuntoVenta.Application.Commands.Inventarios;

public sealed record ObtenerReporteInventarioQuery(
    string? Codigo,
    Guid? CategoriaId,
    Guid? ProveedorId) : IRequest<ErrorOr<ReporteInventarioResultadoDto>>;

public sealed class ObtenerReporteInventarioHandler(
    IProductoRepository productoRepository)
        : IRequestHandler<ObtenerReporteInventarioQuery, ErrorOr<ReporteInventarioResultadoDto>>
{
    private readonly IProductoRepository _productoRepository = productoRepository;

    public async ValueTask<ErrorOr<ReporteInventarioResultadoDto>> Handle(
        ObtenerReporteInventarioQuery query,
        CancellationToken cancellationToken)
    {
        var codigoTrim = string.IsNullOrWhiteSpace(query.Codigo) ? null : query.Codigo.Trim();

        var proyeccion = await _productoRepository.ObtenerReporteInventarioProyectadoAsync(
            codigoTrim,
            query.CategoriaId,
            query.ProveedorId,
            IProductoRepository.MaxFilasReporteInventario,
            cancellationToken);

        // Cap de filas: el repo lee a lo sumo MaxFilasReporteInventario + 1.
        // El handler de Excel delega aquí, por lo que ambos quedan protegidos.
        if (proyeccion.Count > IProductoRepository.MaxFilasReporteInventario)
        {
            return Error.Validation(
                "ReporteInventario_DemasiadasFilas",
                $"El reporte supera el máximo de {IProductoRepository.MaxFilasReporteInventario:N0} filas. " +
                "Aplica un filtro para reducir el resultado.");
        }

        var filas = proyeccion.Select(ConstruirFila).ToList();

        var totalExistencia = filas.Sum(f => f.Existencia);
        var totalValorCosto = filas.Sum(f => f.ValorCosto);
        var totalValorImpuesto = filas.Sum(f => Dinero.Redondear(f.Existencia * f.MontoImpuesto));
        var totalValorVenta = filas.Sum(f => f.ValorVenta);

        return new ReporteInventarioResultadoDto(
            Filas: filas,
            TotalExistencia: totalExistencia,
            TotalValorCosto: totalValorCosto,
            TotalValorImpuesto: totalValorImpuesto,
            TotalValorVenta: totalValorVenta);
    }

    // PrecioUnitario es NETO (sin IVA): el impuesto se suma encima, igual que
    // DocumentoVentaLinea.CalcularMontos. Ver Decisiones del feature.
    private static ReporteInventarioFilaDto ConstruirFila(InventarioReporteProyeccionDto p)
    {
        var precioNeto = Dinero.Redondear(p.PrecioUnitario);
        var montoImpuesto = Dinero.Redondear(precioNeto * p.TarifaPorcentaje / 100m);
        var precioVenta = precioNeto + montoImpuesto;
        var precioCosto = p.PrecioCosto ?? 0m;
        var valorCosto = Dinero.Redondear(p.Existencia * precioCosto);
        var valorVenta = Dinero.Redondear(p.Existencia * precioVenta);

        return new ReporteInventarioFilaDto(
            ProductoId: p.ProductoId,
            Codigo: p.Codigo,
            Nombre: p.Nombre,
            Descripcion: p.Descripcion ?? string.Empty,
            Categoria: p.Categoria ?? string.Empty,   // null -> "" -> Excel muestra "-"
            Proveedor: p.Proveedor ?? string.Empty,  // null -> "" -> Excel muestra "-"
            FechaCreacion: p.FechaCreacion,
            Existencia: p.Existencia,
            PrecioCosto: precioCosto,
            PrecioNeto: precioNeto,
            TarifaPorcentaje: p.TarifaPorcentaje,
            MontoImpuesto: montoImpuesto,
            PrecioVenta: precioVenta,
            ValorCosto: valorCosto,
            ValorVenta: valorVenta);
    }
}
