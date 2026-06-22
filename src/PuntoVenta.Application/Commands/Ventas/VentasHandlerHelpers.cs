using ErrorOr;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.CondicionesVenta;
using PuntoVenta.Domain.Entities.MediosPago;
using PuntoVenta.Domain.Entities.MovimientosStock;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

internal static partial class VentasHandlerHelpers
{
    public static DateTime NormalizarFechaUtc(DateTime? fechaDocumento)
    {
        if (!fechaDocumento.HasValue)
        {
            return DateTime.UtcNow;
        }

        return fechaDocumento.Value.Kind switch
        {
            DateTimeKind.Utc => fechaDocumento.Value,
            DateTimeKind.Local => fechaDocumento.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(fechaDocumento.Value, DateTimeKind.Utc)
        };
    }

    public static async Task<ErrorOr<CondicionVenta>> ObtenerCondicionVentaAsync(
        string codigo,
        ICondicionVentaRepository repository,
        CancellationToken cancellationToken)
    {
        var condicion = (await repository.ObtenerListaAsync(true, cancellationToken))
            .FirstOrDefault(c => c.Codigo == codigo.Trim());

        return condicion is null
            ? Error.Validation("DocumentoVenta_CondicionVentaCodigo", "La condición de venta indicada no existe o está inactiva.")
            : condicion;
    }

    public static async Task<ErrorOr<IReadOnlyList<(DocumentoVentaPagoCommand Pago, MedioPago MedioPago)>>> PrepararPagosAsync(
        IReadOnlyList<DocumentoVentaPagoCommand> pagos,
        IMedioPagoRepository medioPagoRepository,
        CancellationToken cancellationToken)
    {
        var medios = await medioPagoRepository.ObtenerListaAsync(true, cancellationToken);
        var resultado = new List<(DocumentoVentaPagoCommand Pago, MedioPago MedioPago)>();

        foreach (var pago in pagos)
        {
            var medio = medios.FirstOrDefault(m => m.Codigo == pago.MedioPagoCodigo.Trim());
            if (medio is null)
            {
                return Error.Validation("DocumentoVentaPago_MedioPagoCodigo", $"El medio de pago {pago.MedioPagoCodigo} no existe o está inactivo.");
            }

            resultado.Add((pago, medio));
        }

        return resultado;
    }

    public static async Task<ErrorOr<IReadOnlyList<LineaPreparada>>> PrepararLineasAsync(
        IReadOnlyList<DocumentoVentaLineaCommand> lineas,
        IProductoRepository productoRepository,
        ITarifaIvaImpuestoRepository tarifaRepository,
        bool usarPrecioInventario,
        string monedaCodigoDocumento,
        decimal tipoCambioDocumento,
        CancellationToken cancellationToken,
        bool permitirPrecioLibre = false)
    {
        var tarifas = await tarifaRepository.ObtenerListaAsync(true, cancellationToken);
        var resultado = new List<LineaPreparada>();

        foreach (var linea in lineas)
        {
            var producto = await productoRepository.GetByIdAsync(linea.ProductoId, cancellationToken);
            if (producto is null)
            {
                return ProductoErrors.NoEncontrado;
            }

            var precioCatalogoDocMoneda = ConvertirDeCrcADocMoneda(
                producto.PrecioUnitario, monedaCodigoDocumento, tipoCambioDocumento);

            var tarifa = ObtenerTarifaProducto(producto, tarifas);

            if (!permitirPrecioLibre &&
                usarPrecioInventario &&
                !producto.PermiteModificarPrecioUnitario &&
                linea.PrecioUnitario.HasValue &&
                decimal.Round(linea.PrecioUnitario.Value, 5, MidpointRounding.AwayFromZero)
                    != decimal.Round(precioCatalogoDocMoneda, 5, MidpointRounding.AwayFromZero))
            {
                return Error.Validation(
                    "DocumentoVenta_Lineas",
                    "El precio del producto cambió en el catálogo. Elimina la línea y agrégala de nuevo.");
            }

            if (!permitirPrecioLibre &&
                !producto.PermiteModificarPrecioUnitario &&
                !usarPrecioInventario &&
                linea.PrecioUnitario.HasValue &&
                decimal.Round(linea.PrecioUnitario.Value, 5, MidpointRounding.AwayFromZero)
                    != decimal.Round(precioCatalogoDocMoneda, 5, MidpointRounding.AwayFromZero))
            {
                return Error.Validation(
                    "DocumentoVenta_Lineas",
                    "Este producto no permite modificar el precio unitario.");
            }

            var precioUnitario = (usarPrecioInventario && !producto.PermiteModificarPrecioUnitario && !permitirPrecioLibre)
                ? precioCatalogoDocMoneda
                : linea.PrecioUnitario ?? precioCatalogoDocMoneda;

            var descripcion = string.IsNullOrWhiteSpace(linea.Descripcion)
                ? producto.Nombre
                : linea.Descripcion!.Trim();

            resultado.Add(new LineaPreparada(
                producto.Id,
                producto.TipoItem,
                producto.Codigo,
                descripcion,
                "Unid",
                linea.Cantidad,
                precioUnitario,
                linea.MontoDescuento,
                producto.TarifaIvaImpuestoCodigo,
                tarifa?.Porcentaje ?? 0,
                linea.DevuelveInventario,
                producto.NoAplicaExistencias,
                producto.PermiteModificarPrecioUnitario));
        }

        return resultado;
    }

    public static IReadOnlyList<LineaPreparada> PrepararLineasDesdeSnapshot(DocumentoVenta documento)
    {
        return documento.Lineas.Select(l => new LineaPreparada(
            l.ProductoId ?? Guid.Empty,
            l.TipoItem,
            l.Codigo,
            l.Descripcion,
            l.UnidadMedidaCodigo,
            l.Cantidad,
            l.PrecioUnitario,
            l.MontoDescuento,
            l.TarifaIvaImpuestoCodigo,
            l.PorcentajeImpuesto,
            false,
            l.NoAplicaExistencias,
            l.PermiteModificarPrecioUnitario)).ToList();
    }

    public static async Task<ErrorOr<string>> SiguienteConsecutivoAsync(
        TipoDocumentoVenta tipoDocumento,
        ISecuenciaRepository secuenciaRepository,
        CancellationToken cancellationToken)
    {
        var secuencia = await secuenciaRepository.ObtenerOCrearEditableAsync(tipoDocumento, cancellationToken);
        var consecutivo = secuencia.Siguiente();
        await secuenciaRepository.UpdateAsync(secuencia, cancellationToken);
        return consecutivo;
    }

    public static void AgregarPagos(
        DocumentoVenta documento,
        IReadOnlyList<(DocumentoVentaPagoCommand Pago, MedioPago MedioPago)> pagos)
    {
        foreach (var (pago, medioPago) in pagos)
        {
            documento.AgregarPago(
                pago.MonedaCodigo,
                pago.TipoCambioAplicado,
                medioPago.Codigo,
                medioPago.Detalle,
                pago.MontoEntregado,
                pago.MontoAplicadoMonedaPago,
                pago.MontoAplicadoDocumento,
                pago.MontoVueltoMonedaPago,
                pago.MontoVueltoDocumento,
                pago.Referencia,
                pago.Observacion,
                DateTime.UtcNow,
                null);
        }
    }

    public static void AgregarLineas(
        DocumentoVenta documento,
        IReadOnlyList<LineaPreparada> lineas)
    {
        foreach (var linea in lineas)
        {
            documento.AgregarLinea(
                linea.ProductoId == Guid.Empty ? null : linea.ProductoId,
                linea.TipoItem,
                linea.Codigo,
                linea.Descripcion,
                linea.UnidadMedidaCodigo,
                linea.Cantidad,
                linea.PrecioUnitario,
                linea.MontoDescuento,
                linea.TarifaIvaImpuestoCodigo,
                linea.PorcentajeImpuesto,
                linea.DevuelveInventario,
                linea.NoAplicaExistencias,
                linea.PermiteModificarPrecioUnitario);
        }
    }

    public static void AgregarPagos(
        DocumentoVenta documento,
        IReadOnlyList<(DocumentoVentaPagoCommand Pago, Domain.Entities.MediosPago.MedioPago MedioPago)> pagos,
        DateTime fechaPago,
        Guid usuarioId,
        DateTime fechaRegistroUtc)
    {
        foreach (var (pago, medioPago) in pagos)
        {
            documento.AgregarPago(
                pago.MonedaCodigo,
                pago.TipoCambioAplicado,
                medioPago.Codigo,
                medioPago.Detalle,
                pago.MontoEntregado,
                pago.MontoAplicadoMonedaPago,
                pago.MontoAplicadoDocumento,
                pago.MontoVueltoMonedaPago,
                pago.MontoVueltoDocumento,
                pago.Referencia,
                pago.Observacion,
                fechaPago,
                usuarioId,
                fechaRegistroUtc);
        }
    }

    /// <summary>
    /// Aplica movimientos de stock para las líneas del documento.
    /// Para ventas (deltaEsNegativo=true) descuenta stock; para NC con DevuelveInventario=true lo reintegra.
    /// Ignora líneas sin ProductoId o con NoAplicaExistencias.
    /// No bloquea la operación si el stock queda negativo.
    /// </summary>
    public static async Task AplicarMovimientosStockAsync(
        IReadOnlyCollection<DocumentoVentaLinea> lineas,
        DocumentoVenta documento,
        bool deltaEsNegativo,
        IProductoRepository productoRepository,
        IMovimientoStockRepository movimientoRepository,
        DateTime fechaUtc,
        Guid? usuarioId,
        CancellationToken cancellationToken)
    {
        var idsConStock = lineas
            .Where(l => l.ProductoId.HasValue && !l.NoAplicaExistencias)
            .Where(l => deltaEsNegativo || l.DevuelveInventario) // ventas: siempre; NC: solo si devuelve inventario
            .Select(l => l.ProductoId!.Value)
            .Distinct()
            .ToList();

        if (idsConStock.Count == 0) return;

        var productosEditables = await productoRepository.ObtenerPorIdsEditablesAsync(idsConStock, cancellationToken);
        var mapProductos = productosEditables.ToDictionary(p => p.Id);
        var movimientos = new List<MovimientoStock>();

        foreach (var linea in lineas)
        {
            if (!linea.ProductoId.HasValue || linea.NoAplicaExistencias) continue;
            if (!deltaEsNegativo && !linea.DevuelveInventario) continue;
            if (!mapProductos.TryGetValue(linea.ProductoId.Value, out var producto)) continue;

            var delta = deltaEsNegativo ? -linea.Cantidad : linea.Cantidad;
            var saldo = producto.AplicarMovimientoStock(delta);

            var movimiento = MovimientoStock.Crear(
                productoId: producto.Id,
                fechaUtc: fechaUtc,
                delta: delta,
                saldoResultante: saldo,
                usuarioId: usuarioId,
                tipoDocumentoOrigen: documento.TipoDocumento,
                documentoVentaId: documento.Id,
                consecutivoDocumento: documento.Consecutivo);

            if (!movimiento.IsError)
                movimientos.Add(movimiento.Value);
        }

        // Agrega movimientos al contexto sin guardar; los productos ya están tracked (Modified).
        // El SaveChangesAsync del documento que sigue persiste todo en bloque.
        await movimientoRepository.AgregarRangoSinPersistirAsync(movimientos, cancellationToken);
    }

    public static string ResolverTipoDocReferencia(TipoDocumentoVenta tipo) => tipo switch
    {
        TipoDocumentoVenta.Factura => "01",
        TipoDocumentoVenta.NotaDebito => "03",
        TipoDocumentoVenta.NotaCredito => "02",
        _ => "99"
    };

    private static decimal ConvertirDeCrcADocMoneda(decimal precioCrc, string monedaDocumento, decimal tipoCambio)
    {
        if (monedaDocumento == "CRC" || tipoCambio <= 0)
        {
            return precioCrc;
        }
        return precioCrc / tipoCambio;
    }

    private static TarifaIvaImpuesto? ObtenerTarifaProducto(
        Producto producto,
        IReadOnlyList<TarifaIvaImpuesto> tarifas)
    {
        return string.IsNullOrWhiteSpace(producto.TarifaIvaImpuestoCodigo)
            ? null
            : tarifas.FirstOrDefault(t => t.Codigo == producto.TarifaIvaImpuestoCodigo);
    }
}
