using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Domain.Common;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record EmitirNotaCreditoCommand(
    Guid DocumentoOrigenId,
    ModoNotaCredito Modo,
    IReadOnlyList<DocumentoVentaLineaCommand> Lineas,
    string? Razon = null,
    DateTime? FechaDocumento = null,
    string? Observaciones = null,
    IReadOnlyList<Guid>? ProductosSinReintegro = null) : IRequest<ErrorOr<Guid>>;

public sealed class EmitirNotaCreditoHandler(
    IUsuarioActual usuarioActual,
    IFechaActual fechaActual,
    IDocumentoVentaRepository documentoRepository,
    ISecuenciaRepository secuenciaRepository,
    IProductoRepository productoRepository,
    ITarifaIvaImpuestoRepository tarifaRepository,
    IDocumentoVentaEventoService eventoService,
    IMovimientoStockRepository movimientoStockRepository) : IRequestHandler<EmitirNotaCreditoCommand, ErrorOr<Guid>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly ISecuenciaRepository _secuenciaRepository = secuenciaRepository;
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly ITarifaIvaImpuestoRepository _tarifaRepository = tarifaRepository;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;
    private readonly IMovimientoStockRepository _movimientoStockRepository = movimientoStockRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(EmitirNotaCreditoCommand command, CancellationToken cancellationToken)
    {
        var origen = await _documentoRepository.ObtenerDetalleAsync(command.DocumentoOrigenId, cancellationToken);
        if (origen is null)
        {
            return DocumentoVentaErrors.DocumentoOrigenNoEncontrado;
        }

        if (origen.Estado != EstadoDocumentoVenta.Emitido)
        {
            return DocumentoVentaErrors.DocumentoOrigenNoEmitido;
        }

        if (origen.TipoDocumento is not (TipoDocumentoVenta.Factura or TipoDocumentoVenta.NotaDebito))
        {
            return DocumentoVentaErrors.NotaCreditoTipoOrigenInvalido;
        }

        if (origen.TipoDocumento == TipoDocumentoVenta.Factura
            && origen.EsCredito
            && origen.Pagos.Any(p => !p.Anulado))
        {
            return DocumentoVentaErrors.NotaCreditoSobreFacturaConAbonosActivos;
        }

        if (command.Modo == ModoNotaCredito.Anulacion
            && origen.TipoDocumento == TipoDocumentoVenta.Factura)
        {
            var notasDebitoVigentes = await _documentoRepository.ObtenerNotasDebitoVigentesAsync(
                origen.Id, cancellationToken);
            if (notasDebitoVigentes.Count > 0)
            {
                return DocumentoVentaErrors.AnulacionConNotasDebitoVigentes(
                    string.Join(", ", notasDebitoVigentes));
            }
        }

        var consumoPrevio = await _documentoRepository.ObtenerConsumoNotasCreditoPorProductoAsync(
            origen.Id, cancellationToken);

        var sinReintegro = command.ProductosSinReintegro is { Count: > 0 }
            ? new HashSet<Guid>(command.ProductosSinReintegro)
            : [];

        var lineasComando = command.Modo == ModoNotaCredito.Anulacion
            ? ArmarLineasAnulacion(origen, consumoPrevio, sinReintegro)
            : AplicarFlagDevolverInventario(command.Modo, command.Lineas, sinReintegro);

        if (origen.TipoDocumento == TipoDocumentoVenta.NotaDebito)
        {
            lineasComando = [.. lineasComando.Select(l => l with { DevuelveInventario = false })];
        }

        if (lineasComando.Count == 0)
        {
            return DocumentoVentaErrors.NotaCreditoLineasRequeridas;
        }

        var lineas = await VentasHandlerHelpers.PrepararLineasAsync(
            lineasComando,
            _productoRepository,
            _tarifaRepository,
            false,
            origen.MonedaCodigo,
            origen.TipoCambio,
            cancellationToken,
            permitirPrecioLibre: true);
        if (lineas.IsError)
        {
            return lineas.Errors;
        }

        var nota = DocumentoVenta.Crear(
            TipoDocumentoVenta.NotaCredito,
            origen.ClienteId,
            origen.VendedorId,
            origen.CondicionVentaCodigo,
            origen.CondicionVentaDetalleSnapshot,
            command.FechaDocumento ?? DateTime.UtcNow,
            origen.MonedaCodigo,
            origen.TipoCambio,
            null,
            command.Observaciones,
            origen.Id);
        if (nota.IsError)
        {
            return nota.Errors;
        }

        VentasHandlerHelpers.AgregarLineas(nota.Value, lineas.Value);

        var tipoDocReferencia = VentasHandlerHelpers.ResolverTipoDocReferencia(origen.TipoDocumento);
        var referencia = nota.Value.AgregarReferencia(origen.Id, tipoDocReferencia, origen.FechaDocumento, command.Razon);
        if (referencia.IsError)
        {
            return referencia.Errors;
        }

        var acumuladoNotasPrevias = await _documentoRepository.ObtenerMontoNotasEmitidasAsync(
            origen.Id, TipoDocumentoVenta.NotaCredito, cancellationToken);
        var acumuladoConEsta = acumuladoNotasPrevias + nota.Value.TotalComprobante;
        if (Dinero.RedondearPago(acumuladoConEsta) > Dinero.RedondearPago(origen.TotalComprobante))
        {
            return DocumentoVentaErrors.NotaCreditoExcedeMontoOrigen(
                acumuladoConEsta, origen.TotalComprobante);
        }

        if (command.Modo == ModoNotaCredito.CorrigeMonto
            && acumuladoConEsta >= origen.TotalComprobante - 0.005m)
        {
            return DocumentoVentaErrors.NotaCreditoCorrigeMontoTotalReversa;
        }

        var consecutivo = await VentasHandlerHelpers.SiguienteConsecutivoAsync(
            TipoDocumentoVenta.NotaCredito, _secuenciaRepository, cancellationToken);
        if (consecutivo.IsError)
        {
            return consecutivo.Errors;
        }

        var confirmar = nota.Value.ConfirmarNota(0, null, consecutivo.Value);
        if (confirmar.IsError)
        {
            return confirmar.Errors;
        }

        var correlacionId = Guid.NewGuid();

        _ = await _eventoService.RegistrarAsync(
            nota.Value.Id,
            "NotaCreditoEmitida",
            $"Nota de crédito {nota.Value.Consecutivo} emitida contra {origen.Consecutivo}",
            payload: new
            {
                consecutivo = nota.Value.Consecutivo,
                total = nota.Value.TotalComprobante,
                moneda = nota.Value.MonedaCodigo,
                documentoOrigenId = origen.Id,
                origenConsecutivo = origen.Consecutivo,
                modo = command.Modo.ToString(),
                razon = command.Razon,
                tipoDocReferencia
            },
            correlacionId: correlacionId,
            cancellationToken: cancellationToken);

        _ = await _eventoService.RegistrarAsync(
            origen.Id,
            "NotaCreditoAplicada",
            $"Se aplicó nota de crédito {nota.Value.Consecutivo} por {nota.Value.TotalComprobante:N2} {nota.Value.MonedaCodigo}",
            payload: new
            {
                notaCreditoId = nota.Value.Id,
                notaCreditoConsecutivo = nota.Value.Consecutivo,
                total = nota.Value.TotalComprobante,
                moneda = nota.Value.MonedaCodigo,
                modo = command.Modo.ToString(),
                razon = command.Razon
            },
            correlacionId: correlacionId,
            cancellationToken: cancellationToken);

        // Reintegro de inventario para líneas con DevuelveInventario=true
        await VentasHandlerHelpers.AplicarMovimientosStockAsync(
            nota.Value.Lineas,
            nota.Value,
            deltaEsNegativo: false,
            _productoRepository,
            _movimientoStockRepository,
            _fechaActual.AhoraUtc,
            _usuarioActual.UsuarioId,
            cancellationToken);

        await _documentoRepository.AddAsync(nota.Value, cancellationToken);
        return nota.Value.Id;
    }

    private static IReadOnlyList<DocumentoVentaLineaCommand> AplicarFlagDevolverInventario(
        ModoNotaCredito modo,
        IReadOnlyList<DocumentoVentaLineaCommand> lineas,
        IReadOnlySet<Guid> sinReintegro)
    {
        var devolver = modo == ModoNotaCredito.Devolucion;
        return [.. lineas.Select(l => l with
        {
            DevuelveInventario = devolver && !sinReintegro.Contains(l.ProductoId),
        })];
    }

    private static IReadOnlyList<DocumentoVentaLineaCommand> ArmarLineasAnulacion(
        DocumentoVenta origen,
        IReadOnlyDictionary<Guid, ConsumoNotaCreditoPorProductoDto> consumoPrevio,
        IReadOnlySet<Guid> sinReintegro)
    {
        var resultado = new List<DocumentoVentaLineaCommand>();
        foreach (var l in origen.Lineas)
        {
            if (!l.ProductoId.HasValue) continue;

            consumoPrevio.TryGetValue(l.ProductoId.Value, out var consumido);
            var cantidadDisponible = l.Cantidad - (consumido?.CantidadDevueltaInventario ?? 0m);
            var subtotalDisponible = l.Subtotal - (consumido?.SubtotalAcumulado ?? 0m);

            if (cantidadDisponible <= 0m || subtotalDisponible <= 0m) continue;

            var precioEfectivo = subtotalDisponible / cantidadDisponible;
            resultado.Add(new DocumentoVentaLineaCommand(
                l.ProductoId.Value,
                cantidadDisponible,
                precioEfectivo,
                MontoDescuento: 0m,
                DevuelveInventario: !sinReintegro.Contains(l.ProductoId.Value),
                Id: null,
                Descripcion: l.Descripcion));
        }
        return resultado;
    }
}
