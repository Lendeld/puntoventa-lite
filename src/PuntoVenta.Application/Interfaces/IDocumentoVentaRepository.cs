using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Queries.Dashboard;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Interfaces;

public interface IDocumentoVentaRepository : IRepository<DocumentoVenta>
{
    // Cap de filas (líneas de documento) del reporte de ventas por rango. Red de
    // seguridad para no cargar millones de filas en memoria. El repo lee a lo sumo
    // MaxFilasReporteRango + 1; el handler rechaza si la cantidad supera el tope.
    const int MaxFilasReporteRango = 100_000;

    // Reporte de ventas por rango: proyección liviana (no materializa entidades).
    // Una fila por línea de documento; filtra Factura+NC+ND emitidas en el rango
    // de fecha de factura, consecutivo opcional. Lee a lo sumo maxFilas + 1 filas
    // para detectar (sin cargar todo) cuando el reporte supera el cap.
    Task<IReadOnlyList<VentaRangoProyeccionDto>> ObtenerVentasRangoProyectadoAsync(
        DateTime desdeUtc,
        DateTime hastaUtc,
        string? consecutivo,
        int maxFilas,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<VentaRangoProyeccionDto>>([]);

    Task<VentasPeriodoDto> ObtenerResumenVentasAsync(
        DateTime desdeUtc,
        DateTime hastaUtc,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new VentasPeriodoDto(0m, 0));
    Task<IReadOnlyList<PuntoTendenciaDto>> ObtenerTendenciaVentasAsync(
        DateTime desdeUtc,
        DateTime hastaUtc,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PuntoTendenciaDto>>([]);
    Task<IReadOnlyList<MetodoPagoDto>> ObtenerVentasPorMetodoPagoAsync(
        DateTime desdeUtc,
        DateTime hastaUtc,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<MetodoPagoDto>>([]);
    Task<IReadOnlyList<TopProductoDto>> ObtenerTopProductosAsync(
        DateTime desdeUtc,
        DateTime hastaUtc,
        int top,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TopProductoDto>>([]);
    Task<CuentasPorCobrarDto> ObtenerCuentasPorCobrarVencidasAsync(
        DateTime hastaUtc,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new CuentasPorCobrarDto(0m, 0));
    Task<DocumentoVenta?> ObtenerDetalleAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Uso interno de workers/procesos de sistema. Bypassa el filtro de tenant del IUsuarioActual:
    /// el caller debe pasar el negocioId correcto (típicamente desde un registro persistido y confiable).
    /// No invocar desde endpoints o handlers con contexto de usuario.
    /// </summary>
    Task<DocumentoVenta?> ObtenerDetalleParaSistemaAsync(Guid negocioId, Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
    Task<IReadOnlyList<DocumentoVenta>> ObtenerDocumentosGeneradosAsync(Guid documentoOrigenId, CancellationToken cancellationToken = default);
    Task<decimal> ObtenerMontoNotasEmitidasAsync(Guid documentoOrigenId, TipoDocumentoVenta tipoNota, CancellationToken cancellationToken = default)
        => Task.FromResult(0m);
    Task<IReadOnlyDictionary<Guid, ConsumoNotaCreditoPorProductoDto>> ObtenerConsumoNotasCreditoPorProductoAsync(
        Guid documentoOrigenId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<Guid, ConsumoNotaCreditoPorProductoDto>>(
            new Dictionary<Guid, ConsumoNotaCreditoPorProductoDto>());
    Task<IReadOnlyDictionary<Guid, decimal>> ObtenerMontoNotasCreditoPorDocumentosAsync(
        IReadOnlyCollection<Guid> documentoOrigenIds,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<Guid, decimal>>(new Dictionary<Guid, decimal>());
    Task<IReadOnlyDictionary<Guid, decimal>> ObtenerMontoNotasDebitoPorDocumentosAsync(
        IReadOnlyCollection<Guid> documentoOrigenIds,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<Guid, decimal>>(new Dictionary<Guid, decimal>());
    /// <summary>
    /// Notas de débito emitidas contra el documento origen que aún tienen saldo
    /// vigente (Total − ΣNC emitidas contra esa ND &gt; 0). Usado para bloquear la
    /// anulación de la factura mientras existan cargos ND sin reversar.
    /// Retorna los consecutivos de las ND vigentes.
    /// </summary>
    Task<IReadOnlyList<string>> ObtenerNotasDebitoVigentesAsync(
        Guid documentoOrigenId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<string>>([]);
    Task<DocumentoVenta?> ObtenerEditableAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentoVenta>> ObtenerApartadosReservadosVencidosAsync(DateTime ahora, CancellationToken cancellationToken = default);
    Task RegistrarAbonoAsync(DocumentoVenta documento, DocumentoVentaPago pagoNuevo, CancellationToken cancellationToken = default);
    Task AnularAbonoAsync(DocumentoVenta documento, DocumentoVentaPago pagoAnulado, CancellationToken cancellationToken = default);
    Task<int> ObtenerMaxNumeroConsecutivoAsync(Guid negocioId, Guid cajaId, TipoDocumentoVenta tipoDocumento, CancellationToken cancellationToken = default)
        => Task.FromResult(0);
    Task<(decimal SaldoVigente, decimal SaldoVencido, int FacturasVencidas, int DiasAtrasoMax)> ObtenerSaldosCreditoClienteAsync(Guid clienteId, DateTime ahora, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentoVenta>> ObtenerFacturasCreditoClienteAsync(Guid clienteId, bool? soloConSaldo, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<DocumentoVenta> Items, int Total)> ObtenerListaCreditoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        Guid? clienteId,
        bool? soloVencidas,
        DateTime ahora,
        CancellationToken cancellationToken = default);
    Task<ReporteMovimientosDineroResultadoDto> ObtenerReporteMovimientosDineroAsync(
        DateTime fechaDesdeUtc,
        DateTime fechaHastaUtc,
        Guid? cajaId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new ReporteMovimientosDineroResultadoDto([], [], 0m, 0m, 0m));
    Task<(IReadOnlyList<DocumentoVenta> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        TipoDocumentoVenta? tipoDocumento,
        EstadoDocumentoVenta? estado,
        Guid? clienteId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        CancellationToken cancellationToken = default);
}

public sealed record ConsumoNotaCreditoPorProductoDto(
    decimal CantidadDevueltaInventario,
    decimal SubtotalAcumulado);

