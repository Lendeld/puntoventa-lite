using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerReporteVentasRangoQuery(
    DateTime FechaDesdeUtc,
    DateTime FechaHastaUtc,
    string? Consecutivo,
    bool Colonizar,
    bool Detallado) : IRequest<ErrorOr<ReporteVentasRangoResultadoDto>>;

public sealed class ObtenerReporteVentasRangoHandler(
    IDocumentoVentaRepository documentoRepository)
        : IRequestHandler<ObtenerReporteVentasRangoQuery, ErrorOr<ReporteVentasRangoResultadoDto>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;

    public async ValueTask<ErrorOr<ReporteVentasRangoResultadoDto>> Handle(
        ObtenerReporteVentasRangoQuery query,
        CancellationToken cancellationToken)
    {
        if (query.FechaHastaUtc < query.FechaDesdeUtc)
        {
            return Error.Validation(
                "ReporteVentasRango_RangoInvertido",
                "La fecha final no puede ser anterior a la fecha inicial.");
        }

        var proyeccion = await _documentoRepository.ObtenerVentasRangoProyectadoAsync(
            query.FechaDesdeUtc,
            query.FechaHastaUtc,
            string.IsNullOrWhiteSpace(query.Consecutivo) ? null : query.Consecutivo.Trim(),
            IDocumentoVentaRepository.MaxFilasReporteRango,
            cancellationToken);

        // Cap de filas: el repo lee a lo sumo MaxFilasReporteRango + 1. Si vino más
        // del tope, el reporte es demasiado grande para procesar/descargar. Como el
        // handler de Excel delega aquí, ambos quedan protegidos por el mismo cap.
        if (proyeccion.Count > IDocumentoVentaRepository.MaxFilasReporteRango)
        {
            return Error.Validation(
                "ReporteVentasRango_DemasiadasFilas",
                $"El reporte supera el máximo de {IDocumentoVentaRepository.MaxFilasReporteRango:N0} filas. " +
                "Reduce el rango o usa el modo Resumido.");
        }

        // Valores convertidos (tipo de cambio + signo NC) en PRECISIÓN COMPLETA
        // (5 dec), SIN redondear, en todo el camino de datos: filas, subtotales
        // por documento y totales generales. El redondeo a 2 dec es responsabilidad
        // de la capa de presentación (formato "#,##0.00" del Excel), no del dato.
        // Así Excel suma los valores reales de las celdas y el total coincide,
        // evitando el céntimo de basura de redondear cada línea por separado.
        var lineas = proyeccion
            .Select(p => ConstruirLinea(p, query.Colonizar))
            .ToList();

        // Totales generales: suma de la precisión completa de TODAS las líneas,
        // sin redondear. Igual para Detallado y Resumido.
        var totales = CalcularTotales(lineas);

        if (query.Detallado)
        {
            return new ReporteVentasRangoResultadoDto(
                Detallado: true,
                Colonizado: query.Colonizar,
                Filas: lineas.Select(l => l.Fila).ToList(),
                Resumen: [],
                TotalSubtotal: totales.Subtotal,
                TotalDescuento: totales.Descuento,
                TotalImpuesto: totales.Impuesto,
                TotalGeneral: totales.Total);
        }

        var resumen = AgruparPorDocumento(lineas);
        return new ReporteVentasRangoResultadoDto(
            Detallado: false,
            Colonizado: query.Colonizar,
            Filas: [],
            Resumen: resumen,
            TotalSubtotal: totales.Subtotal,
            TotalDescuento: totales.Descuento,
            TotalImpuesto: totales.Impuesto,
            TotalGeneral: totales.Total);
    }

    // Una fila de display más su documento de pertenencia. Los montos de la fila
    // ya vienen en precisión completa (5 dec, sin redondear): son la única fuente
    // para sumar subtotales por documento y totales generales.
    private sealed record LineaCalculo(
        Guid DocumentoId,
        ReporteVentasRangoFilaDto Fila);

    // Construye la fila Detallado con los montos convertidos (tipo de cambio +
    // signo NC) en PRECISIÓN COMPLETA, SIN redondear. El redondeo a 2 dec lo hace
    // la presentación (formato del Excel), no el dato.
    private static LineaCalculo ConstruirLinea(VentaRangoProyeccionDto p, bool colonizar)
    {
        var esCrc = string.Equals(p.MonedaCodigo, "CRC", StringComparison.OrdinalIgnoreCase);
        var aplicarTipoCambio = colonizar && !esCrc;
        var esNotaCredito = p.TipoDocumento == TipoDocumentoVentaProyeccion.NotaCredito;
        var signo = esNotaCredito ? -1m : 1m;

        // Conversión en precisión completa: tipo de cambio + signo, SIN redondear.
        decimal ConvertirSinRedondear(decimal monto)
            => (aplicarTipoCambio ? monto * p.TipoCambio : monto) * signo;

        var monedaResultante = colonizar && !esCrc ? "CRC" : p.MonedaCodigo;
        var medioPago = p.MediosPago.Count > 0 ? string.Join(", ", p.MediosPago) : string.Empty;

        var fila = new ReporteVentasRangoFilaDto(
            DocumentoId: p.DocumentoId,
            Consecutivo: p.Consecutivo,
            FechaFactura: p.FechaDocumento,
            ClienteIdentificacion: p.ClienteIdentificacion,
            ClienteNombre: p.ClienteNombre,
            MedioPago: medioPago,
            CondicionVenta: p.CondicionVentaDetalle,
            MonedaCodigo: monedaResultante,
            TipoCambio: p.TipoCambio,
            NumeroLinea: p.NumeroLinea,
            ProductoCodigo: p.ProductoCodigo,
            ProductoDetalle: p.ProductoDetalle,
            Cantidad: p.Cantidad * signo,
            PrecioUnitario: ConvertirSinRedondear(p.PrecioUnitario),
            Descuento: ConvertirSinRedondear(p.MontoDescuento),
            Subtotal: ConvertirSinRedondear(p.Subtotal),
            TarifaPorcentaje: p.TarifaPorcentaje,
            MontoImpuesto: ConvertirSinRedondear(p.MontoImpuesto),
            TotalLinea: ConvertirSinRedondear(p.TotalLinea),
            EsColonizado: aplicarTipoCambio,
            EsNotaCredito: esNotaCredito);

        return new LineaCalculo(DocumentoId: p.DocumentoId, Fila: fila);
    }

    // Agrupa por documento. Cada campo del documento es la suma en precisión
    // completa (5 dec, SIN redondear) de sus líneas. El redondeo a 2 dec lo hace
    // la presentación. El signo NC ya viene en cada valor convertido de la fila.
    private static List<ReporteVentasRangoResumenFilaDto> AgruparPorDocumento(
        IReadOnlyList<LineaCalculo> lineas)
    {
        // Agrupar por DocumentoId (Guid), no por Consecutivo: el consecutivo puede
        // venir vacío (documento sin consecutivo asignado) y dos documentos distintos
        // colapsarían en el mismo grupo. La cabecera mostrada (consecutivo, fecha,
        // cliente, etc.) se toma del primer registro del grupo.
        return lineas
            .GroupBy(l => l.DocumentoId)
            .Select(g =>
            {
                var cabecera = g.First().Fila;
                return new ReporteVentasRangoResumenFilaDto(
                    DocumentoId: g.Key,
                    Consecutivo: cabecera.Consecutivo,
                    FechaFactura: cabecera.FechaFactura,
                    ClienteIdentificacion: cabecera.ClienteIdentificacion,
                    ClienteNombre: cabecera.ClienteNombre,
                    MedioPago: cabecera.MedioPago,
                    CondicionVenta: cabecera.CondicionVenta,
                    MonedaCodigo: cabecera.MonedaCodigo,
                    TipoCambio: cabecera.TipoCambio,
                    Descuento: g.Sum(l => l.Fila.Descuento),
                    Subtotal: g.Sum(l => l.Fila.Subtotal),
                    MontoImpuesto: g.Sum(l => l.Fila.MontoImpuesto),
                    TotalDocumento: g.Sum(l => l.Fila.TotalLinea),
                    EsColonizado: cabecera.EsColonizado,
                    EsNotaCredito: cabecera.EsNotaCredito);
            })
            .ToList();
    }

    // Totales generales: suma de la precisión completa de TODAS las líneas, SIN
    // redondear. El redondeo a 2 dec es de la presentación (formato del Excel).
    private static (decimal Subtotal, decimal Descuento, decimal Impuesto, decimal Total) CalcularTotales(
        IReadOnlyList<LineaCalculo> lineas)
        => (
            lineas.Sum(l => l.Fila.Subtotal),
            lineas.Sum(l => l.Fila.Descuento),
            lineas.Sum(l => l.Fila.MontoImpuesto),
            lineas.Sum(l => l.Fila.TotalLinea));
}
