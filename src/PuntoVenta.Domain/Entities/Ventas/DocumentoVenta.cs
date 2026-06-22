using ErrorOr;
using PuntoVenta.Domain.Common;
using PuntoVenta.Domain.Entities.Cajas;
using PuntoVenta.Domain.Entities.Clientes;
using PuntoVenta.Domain.Entities.CondicionesVenta;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.Vendedores;
using PuntoVenta.Domain.Entities.Ventas.Eventos;

namespace PuntoVenta.Domain.Entities.Ventas;

public sealed class DocumentoVenta : BaseAuditableEntity
{
    public const int CondicionVentaCodigoMaxLength = 2;
    public const int CondicionVentaDetalleMaxLength = 100;
    public const int MonedaCodigoMaxLength = 3;
    public const int ConsecutivoMaxLength = 20;
    public const int ObservacionesMaxLength = 500;

    private readonly List<DocumentoVentaLinea> _lineas = [];
    private readonly List<DocumentoVentaPago> _pagos = [];
    private readonly List<DocumentoVentaReferencia> _referencias = [];

    private DocumentoVenta() { }

    public TipoDocumentoVenta TipoDocumento { get; private set; }
    public EstadoDocumentoVenta Estado { get; private set; }
    public Guid? ClienteId { get; private set; }
    public Cliente? Cliente { get; private set; }
    public Guid? VendedorId { get; private set; }
    public Vendedor? Vendedor { get; private set; }
    public string CondicionVentaCodigo { get; private set; } = string.Empty;
    public string CondicionVentaDetalleSnapshot { get; private set; } = string.Empty;
    public Guid? DocumentoOrigenId { get; private set; }
    public DocumentoVenta? DocumentoOrigen { get; private set; }
    public int? PlazoCreditoDias { get; private set; }
    public DateTime? FechaVencimiento { get; private set; }
    public DateTime FechaDocumento { get; private set; }
    public string MonedaCodigo { get; private set; } = "CRC";
    public decimal TipoCambio { get; private set; } = 1m;
    public long? NumeroConsecutivo { get; private set; }
    public string? Consecutivo { get; private set; }
    public decimal TotalServiciosGravados { get; private set; }
    public decimal TotalServiciosExentos { get; private set; }
    public decimal TotalMercanciasGravadas { get; private set; }
    public decimal TotalMercanciasExentas { get; private set; }
    public decimal TotalVenta { get; private set; }
    public decimal TotalDescuentos { get; private set; }
    public decimal TotalImpuesto { get; private set; }
    public decimal TotalComprobante { get; private set; }
    public decimal TotalPagado { get; private set; }
    public decimal SaldoPendiente { get; private set; }
    public DateTime? FechaCancelacion { get; private set; }
    public string? Observaciones { get; private set; }
    public bool EsCredito => CondicionVentaCodigos.EsCredito(CondicionVentaCodigo);
    public Guid? CajaId { get; private set; }
    public Caja? Caja { get; private set; }

    public IReadOnlyCollection<DocumentoVentaLinea> Lineas => _lineas;
    public IReadOnlyCollection<DocumentoVentaPago> Pagos => _pagos;
    public IReadOnlyCollection<DocumentoVentaReferencia> Referencias => _referencias;

    public static ErrorOr<DocumentoVenta> Crear(
        TipoDocumentoVenta tipoDocumento,
        Guid? clienteId,
        Guid? vendedorId,
        string condicionVentaCodigo,
        string condicionVentaDetalleSnapshot,
        DateTime fechaDocumento,
        string monedaCodigo = "CRC",
        decimal tipoCambio = 1m,
        int? plazoCreditoDias = null,
        string? observaciones = null,
        Guid? documentoOrigenId = null,
        DateTime? fechaVencimiento = null)
    {
        var errores = ValidarEncabezado(
            tipoDocumento,
            condicionVentaCodigo,
            condicionVentaDetalleSnapshot,
            fechaDocumento,
            monedaCodigo,
            tipoCambio,
            plazoCreditoDias,
            observaciones,
            documentoOrigenId,
            fechaVencimiento);

        if (clienteId.HasValue && clienteId.Value == Guid.Empty)
        {
            errores.Add(Error.Validation("DocumentoVenta_ClienteId", "El cliente indicado es inválido."));
        }

        if (vendedorId.HasValue && vendedorId.Value == Guid.Empty)
        {
            errores.Add(Error.Validation("DocumentoVenta_VendedorId", "El vendedor indicado es inválido."));
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        var estadoInicial = tipoDocumento switch
        {
            TipoDocumentoVenta.Factura => EstadoDocumentoVenta.Borrador,
            TipoDocumentoVenta.Proforma => EstadoDocumentoVenta.Borrador,
            TipoDocumentoVenta.Apartado => EstadoDocumentoVenta.Reservado,
            TipoDocumentoVenta.NotaCredito or TipoDocumentoVenta.NotaDebito => EstadoDocumentoVenta.Emitido,
            _ => EstadoDocumentoVenta.Borrador
        };

        var documento = new DocumentoVenta
        {
            TipoDocumento = tipoDocumento,
            Estado = estadoInicial,
            ClienteId = clienteId,
            VendedorId = vendedorId,
            CondicionVentaCodigo = condicionVentaCodigo.Trim(),
            CondicionVentaDetalleSnapshot = string.IsNullOrWhiteSpace(condicionVentaDetalleSnapshot) ? condicionVentaCodigo.Trim() : condicionVentaDetalleSnapshot.Trim(),
            FechaDocumento = fechaDocumento,
            MonedaCodigo = monedaCodigo.Trim().ToUpperInvariant(),
            TipoCambio = tipoCambio,
            PlazoCreditoDias = plazoCreditoDias,
            FechaVencimiento = tipoDocumento == TipoDocumentoVenta.Apartado
                ? fechaVencimiento
                : plazoCreditoDias.HasValue ? fechaDocumento.Date.AddDays(plazoCreditoDias.Value) : null,
            Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim(),
            DocumentoOrigenId = documentoOrigenId
        };

        documento.RecalcularTotales();
        return documento;
    }

    public ErrorOr<Success> AgregarLinea(
        Guid? productoId,
        TipoItem tipoItem,
        string codigo,
        string descripcion,
        string unidadMedidaCodigo,
        decimal cantidad,
        decimal precioUnitario,
        decimal montoDescuento = 0,
        string? tarifaIvaImpuestoCodigo = null,
        decimal porcentajeImpuesto = 0,
        bool devuelveInventario = false,
        bool noAplicaExistencias = false,
        bool permiteModificarPrecioUnitario = false)
    {
        if (!EsEditable())
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
        }

        var linea = DocumentoVentaLinea.Crear(
            Id,
            productoId,
            tipoItem,
            codigo,
            descripcion,
            unidadMedidaCodigo,
            cantidad,
            precioUnitario,
            montoDescuento,
            tarifaIvaImpuestoCodigo,
            porcentajeImpuesto,
            devuelveInventario,
            noAplicaExistencias,
            permiteModificarPrecioUnitario);

        if (linea.IsError)
        {
            return linea.Errors;
        }

        _lineas.Add(linea.Value);
        RecalcularTotales();
        return Result.Success;
    }

    public ErrorOr<Success> ActualizarEncabezado(
        Guid? clienteId,
        Guid? vendedorId,
        string condicionVentaCodigo,
        string condicionVentaDetalleSnapshot,
        DateTime fechaDocumento,
        string monedaCodigo = "CRC",
        decimal tipoCambio = 1m,
        int? plazoCreditoDias = null,
        string? observaciones = null)
    {
        if (!EsEditable())
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
        }

        var errores = ValidarEncabezado(
            TipoDocumento,
            condicionVentaCodigo,
            condicionVentaDetalleSnapshot,
            fechaDocumento,
            monedaCodigo,
            tipoCambio,
            plazoCreditoDias,
            observaciones,
            DocumentoOrigenId,
            FechaVencimiento);

        if (clienteId.HasValue && clienteId.Value == Guid.Empty)
        {
            errores.Add(Error.Validation("DocumentoVenta_ClienteId", "El cliente indicado es inválido."));
        }

        if (vendedorId.HasValue && vendedorId.Value == Guid.Empty)
        {
            errores.Add(Error.Validation("DocumentoVenta_VendedorId", "El vendedor indicado es inválido."));
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        ClienteId = clienteId;
        VendedorId = vendedorId;
        CondicionVentaCodigo = condicionVentaCodigo.Trim();
        CondicionVentaDetalleSnapshot = string.IsNullOrWhiteSpace(condicionVentaDetalleSnapshot) ? condicionVentaCodigo.Trim() : condicionVentaDetalleSnapshot.Trim();
        FechaDocumento = fechaDocumento;
        MonedaCodigo = monedaCodigo.Trim().ToUpperInvariant();
        TipoCambio = tipoCambio;
        PlazoCreditoDias = plazoCreditoDias;
        FechaVencimiento = plazoCreditoDias.HasValue ? fechaDocumento.Date.AddDays(plazoCreditoDias.Value) : null;
        Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
        RecalcularTotales();

        return Result.Success;
    }

    public ErrorOr<Success> LimpiarLineas()
    {
        if (!EsEditable())
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
        }

        _lineas.Clear();
        RecalcularTotales();
        return Result.Success;
    }

    public ErrorOr<Success> ActualizarLinea(
        Guid lineaId,
        Guid? productoId,
        TipoItem tipoItem,
        string codigo,
        string descripcion,
        string unidadMedidaCodigo,
        decimal cantidad,
        decimal precioUnitario,
        decimal montoDescuento = 0,
        string? tarifaIvaImpuestoCodigo = null,
        decimal porcentajeImpuesto = 0,
        bool devuelveInventario = false,
        bool noAplicaExistencias = false,
        bool permiteModificarPrecioUnitario = false)
    {
        if (!EsEditable())
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
        }

        var linea = _lineas.FirstOrDefault(l => l.Id == lineaId);
        if (linea is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        var actualizar = linea.Actualizar(
            productoId,
            tipoItem,
            codigo,
            descripcion,
            unidadMedidaCodigo,
            cantidad,
            precioUnitario,
            montoDescuento,
            tarifaIvaImpuestoCodigo,
            porcentajeImpuesto,
            devuelveInventario,
            noAplicaExistencias,
            permiteModificarPrecioUnitario);

        if (actualizar.IsError)
        {
            return actualizar.Errors;
        }

        RecalcularTotales();
        return Result.Success;
    }

    public ErrorOr<Success> RemoverLineasExcepto(IReadOnlySet<Guid> lineasVigentes)
    {
        if (!EsEditable())
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
        }

        _lineas.RemoveAll(linea => !lineasVigentes.Contains(linea.Id));
        RecalcularTotales();
        return Result.Success;
    }

    public ErrorOr<Success> AgregarPago(
        string monedaCodigo,
        decimal tipoCambioAplicado,
        string medioPagoCodigo,
        string medioPagoDetalleSnapshot,
        decimal montoEntregado,
        decimal montoAplicadoMonedaPago,
        decimal montoAplicadoDocumento,
        decimal montoVueltoMonedaPago,
        decimal montoVueltoDocumento,
        string? referencia = null,
        string? observacion = null,
        DateTime? fechaPago = null,
        Guid? usuarioRegistroId = null,
        DateTime? fechaRegistroUtc = null)
    {
        if (!EsEditable())
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
        }

        var pago = AgregarPagoInterno(
            monedaCodigo,
            tipoCambioAplicado,
            medioPagoCodigo,
            medioPagoDetalleSnapshot,
            montoEntregado,
            montoAplicadoMonedaPago,
            montoAplicadoDocumento,
            montoVueltoMonedaPago,
            montoVueltoDocumento,
            referencia,
            observacion,
            fechaPago,
            usuarioRegistroId,
            fechaRegistroUtc);

        return pago.IsError ? pago.Errors : Result.Success;
    }

    public ErrorOr<DocumentoVentaPago> RegistrarAbonoApartado(
        string monedaCodigo,
        decimal tipoCambioAplicado,
        string medioPagoCodigo,
        string medioPagoDetalleSnapshot,
        decimal montoEntregado,
        decimal montoAplicadoMonedaPago,
        decimal montoAplicadoDocumento,
        decimal montoVueltoMonedaPago,
        decimal montoVueltoDocumento,
        DateTime fechaPago,
        DateTime fechaRegistroUtc,
        Guid? usuarioRegistroId = null,
        string? referencia = null,
        string? observacion = null)
    {
        if (!EsApartadoOperable())
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
        }

        if (Consecutivo is null)
        {
            return DocumentoVentaErrors.ConsecutivoRequerido;
        }

        if (Dinero.RedondearPago(montoAplicadoDocumento) > Dinero.RedondearPago(SaldoPendiente))
        {
            return DocumentoVentaErrors.PagoExcedeSaldo;
        }

        return AgregarPagoInterno(
            monedaCodigo,
            tipoCambioAplicado,
            medioPagoCodigo,
            medioPagoDetalleSnapshot,
            montoEntregado,
            montoAplicadoMonedaPago,
            montoAplicadoDocumento,
            montoVueltoMonedaPago,
            montoVueltoDocumento,
            referencia,
            observacion,
            fechaPago,
            usuarioRegistroId,
            fechaRegistroUtc);
    }

    public ErrorOr<DocumentoVentaPago> RegistrarAbonoCredito(
        string monedaCodigo,
        decimal tipoCambioAplicado,
        string medioPagoCodigo,
        string medioPagoDetalleSnapshot,
        decimal montoEntregado,
        decimal montoAplicadoMonedaPago,
        decimal montoAplicadoDocumento,
        decimal montoVueltoMonedaPago,
        decimal montoVueltoDocumento,
        DateTime fechaPago,
        DateTime ahora,
        DateTime fechaRegistroUtc,
        Guid? usuarioRegistroId = null,
        string? referencia = null,
        string? observacion = null)
    {
        if (TipoDocumento != TipoDocumentoVenta.Factura || Estado != EstadoDocumentoVenta.Emitido || !EsCredito)
        {
            return DocumentoVentaErrors.AbonoSoloEnCredito;
        }

        if (Consecutivo is null)
        {
            return DocumentoVentaErrors.ConsecutivoRequerido;
        }

        if (SaldoPendiente <= 0)
        {
            return DocumentoVentaErrors.AbonoSinSaldoPendiente;
        }

        if (Dinero.RedondearPago(montoAplicadoDocumento) > Dinero.RedondearPago(SaldoPendiente))
        {
            return DocumentoVentaErrors.PagoExcedeSaldo;
        }

        var pago = AgregarPagoInterno(
            monedaCodigo,
            tipoCambioAplicado,
            medioPagoCodigo,
            medioPagoDetalleSnapshot,
            montoEntregado,
            montoAplicadoMonedaPago,
            montoAplicadoDocumento,
            montoVueltoMonedaPago,
            montoVueltoDocumento,
            referencia,
            observacion,
            fechaPago,
            usuarioRegistroId,
            fechaRegistroUtc);

        if (pago.IsError)
        {
            return pago.Errors;
        }

        pago.Value.AsignarNumeroAbono(_pagos.Count);

        if (SaldoPendiente <= 0 && !FechaCancelacion.HasValue)
        {
            FechaCancelacion = ahora.Kind == DateTimeKind.Local ? ahora.ToUniversalTime() : DateTime.SpecifyKind(ahora, DateTimeKind.Utc);
        }

        return pago.Value;
    }

    public ErrorOr<Success> ExtenderVencimientoApartado(DateTime fechaVencimiento, DateTime ahora)
    {
        if (!EsApartadoOperable())
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
        }

        if (fechaVencimiento == default)
        {
            return DocumentoVentaErrors.FechaVencimientoRequerida;
        }

        if (fechaVencimiento.Date < FechaDocumento.Date)
        {
            return DocumentoVentaErrors.FechaVencimientoInvalida;
        }

        if (fechaVencimiento.Date < ahora.Date)
        {
            return DocumentoVentaErrors.FechaVencimientoInvalida;
        }

        FechaVencimiento = fechaVencimiento;
        if (Estado == EstadoDocumentoVenta.Vencido)
        {
            Estado = EstadoDocumentoVenta.Reservado;
        }

        return Result.Success;
    }

    private ErrorOr<DocumentoVentaPago> AgregarPagoInterno(
        string monedaCodigo,
        decimal tipoCambioAplicado,
        string medioPagoCodigo,
        string medioPagoDetalleSnapshot,
        decimal montoEntregado,
        decimal montoAplicadoMonedaPago,
        decimal montoAplicadoDocumento,
        decimal montoVueltoMonedaPago,
        decimal montoVueltoDocumento,
        string? referencia = null,
        string? observacion = null,
        DateTime? fechaPago = null,
        Guid? usuarioRegistroId = null,
        DateTime? fechaRegistroUtc = null)
    {
        var pago = DocumentoVentaPago.Crear(
            Id,
            monedaCodigo,
            tipoCambioAplicado,
            medioPagoCodigo,
            medioPagoDetalleSnapshot,
            montoEntregado,
            montoAplicadoMonedaPago,
            montoAplicadoDocumento,
            montoVueltoMonedaPago,
            montoVueltoDocumento,
            fechaPago ?? DateTime.UtcNow,
            fechaRegistroUtc ?? fechaPago ?? DateTime.UtcNow,
            usuarioRegistroId,
            referencia,
            observacion);
        if (pago.IsError)
        {
            return pago.Errors;
        }

        _pagos.Add(pago.Value);
        RecalcularTotales();
        return pago.Value;
    }

    public ErrorOr<DocumentoVentaPago> AnularAbono(
        Guid pagoId,
        Guid usuarioAnulaId,
        string motivo,
        DateTime ahoraUtc)
    {
        if (TipoDocumento != TipoDocumentoVenta.Factura || Estado != EstadoDocumentoVenta.Emitido || !EsCredito)
        {
            return DocumentoVentaErrors.AbonoSoloEnCredito;
        }

        var pago = _pagos.FirstOrDefault(p => p.Id == pagoId);
        if (pago is null)
        {
            return DocumentoVentaPagoErrors.NoEncontrado;
        }

        var anular = pago.AnularPago(usuarioAnulaId, motivo, ahoraUtc);
        if (anular.IsError)
        {
            return anular.Errors;
        }

        RecalcularTotales();

        if (FechaCancelacion.HasValue && SaldoPendiente > 0m)
        {
            FechaCancelacion = null;
        }

        return pago;
    }

    public ErrorOr<Success> LimpiarPagos()
    {
        if (!EsEditable())
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
        }

        _pagos.Clear();
        RecalcularTotales();
        return Result.Success;
    }

    public ErrorOr<Success> AgregarReferencia(
        Guid documentoReferenciaId,
        string tipoDocReferencia,
        DateTime fechaDocumentoReferencia,
        string? razon)
    {
        if (!EsEditable())
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
        }

        var referencia = DocumentoVentaReferencia.Crear(
            Id,
            documentoReferenciaId,
            tipoDocReferencia,
            fechaDocumentoReferencia,
            razon);

        if (referencia.IsError)
        {
            return referencia.Errors;
        }

        _referencias.Add(referencia.Value);
        return Result.Success;
    }

    public ErrorOr<Success> Emitir(Guid? cajaId, string consecutivo)
    {
        if (TipoDocumento != TipoDocumentoVenta.Factura || Estado != EstadoDocumentoVenta.Borrador)
        {
            return DocumentoVentaErrors.DocumentoNoEmitible;
        }

        if (string.IsNullOrWhiteSpace(consecutivo))
        {
            return DocumentoVentaErrors.ConsecutivoRequerido;
        }

        var validacion = ValidarParaConfirmar();
        if (validacion.IsError)
        {
            return validacion.Errors;
        }

        CajaId = cajaId == Guid.Empty ? null : cajaId;
        Consecutivo = consecutivo.Trim();
        Estado = EstadoDocumentoVenta.Emitido;
        RecalcularTotales();
        RegistrarEvento(new FacturaEmitidaEvento(Id, Consecutivo, TotalComprobante, MonedaCodigo, ClienteId, EsCredito));
        return Result.Success;
    }

    public ErrorOr<Success> NumerarProforma(long numeroConsecutivo, Guid? cajaId = null, string? consecutivoCustom = null)
    {
        if (TipoDocumento != TipoDocumentoVenta.Proforma || Estado != EstadoDocumentoVenta.Borrador)
        {
            return DocumentoVentaErrors.DocumentoNoEmitible;
        }

        if (Consecutivo is not null)
        {
            return DocumentoVentaErrors.ConsecutivoYaAsignado;
        }

        var validacion = ValidarLineasRequeridas();
        if (validacion.IsError)
        {
            return validacion.Errors;
        }

        AsignarConsecutivo(numeroConsecutivo, consecutivoCustom);
        if (cajaId.HasValue) CajaId = cajaId.Value;
        RecalcularTotales();
        return Result.Success;
    }

    public ErrorOr<Success> ConfirmarApartado(long numeroConsecutivo, Guid? cajaId = null, string? consecutivoCustom = null)
    {
        if (TipoDocumento != TipoDocumentoVenta.Apartado || Estado != EstadoDocumentoVenta.Reservado)
        {
            return DocumentoVentaErrors.DocumentoNoEmitible;
        }

        if (!FechaVencimiento.HasValue)
        {
            return DocumentoVentaErrors.FechaVencimientoRequerida;
        }

        var validacion = ValidarParaConfirmar(permitirPagoParcial: true);
        if (validacion.IsError)
        {
            return validacion.Errors;
        }

        AsignarConsecutivo(numeroConsecutivo, consecutivoCustom);
        if (cajaId.HasValue) CajaId = cajaId.Value;
        RecalcularTotales();
        return Result.Success;
    }

    public ErrorOr<Success> ConfirmarNota(long numeroConsecutivo, Guid? cajaId, string? consecutivoCustom = null)
    {
        if (TipoDocumento is not (TipoDocumentoVenta.NotaCredito or TipoDocumentoVenta.NotaDebito) || Estado != EstadoDocumentoVenta.Emitido)
        {
            return DocumentoVentaErrors.DocumentoNoEmitible;
        }

        var validacion = ValidarParaConfirmar(permitirPagoParcial: true);
        if (validacion.IsError)
        {
            return validacion.Errors;
        }

        if (_referencias.Count == 0 || !DocumentoOrigenId.HasValue)
        {
            return DocumentoVentaErrors.ReferenciaRequerida;
        }

        AsignarConsecutivo(numeroConsecutivo, consecutivoCustom);
        CajaId = cajaId == Guid.Empty ? null : cajaId;
        RecalcularTotales();
        if (TipoDocumento == TipoDocumentoVenta.NotaCredito && DocumentoOrigenId.HasValue)
        {
            RegistrarEvento(new NotaCreditoEmitidaEvento(Id, Consecutivo!, TotalComprobante, MonedaCodigo, DocumentoOrigenId.Value));
        }
        return Result.Success;
    }

    public ErrorOr<Success> MarcarConvertido()
    {
        var convertible =
            (TipoDocumento == TipoDocumentoVenta.Apartado && (Estado == EstadoDocumentoVenta.Reservado || Estado == EstadoDocumentoVenta.Vencido)) ||
            (TipoDocumento == TipoDocumentoVenta.Proforma && Estado == EstadoDocumentoVenta.Borrador);

        if (!convertible)
        {
            return DocumentoVentaErrors.DocumentoNoConvertible;
        }

        Estado = EstadoDocumentoVenta.Convertido;
        return Result.Success;
    }

    public ErrorOr<Success> Cancelar()
    {
        if (!EsApartadoOperable())
        {
            return DocumentoVentaErrors.DocumentoNoCancelable;
        }

        Estado = EstadoDocumentoVenta.Cancelado;
        return Result.Success;
    }

    public ErrorOr<Success> MarcarVencido(DateTime ahora)
    {
        if (TipoDocumento != TipoDocumentoVenta.Apartado || Estado != EstadoDocumentoVenta.Reservado)
        {
            return DocumentoVentaErrors.DocumentoNoCancelable;
        }

        if (!FechaVencimiento.HasValue || FechaVencimiento.Value.Date >= ahora.Date)
        {
            return DocumentoVentaErrors.FechaVencimientoInvalida;
        }

        Estado = EstadoDocumentoVenta.Vencido;
        return Result.Success;
    }

    private bool EsApartadoOperable()
        => TipoDocumento == TipoDocumentoVenta.Apartado
           && Estado is EstadoDocumentoVenta.Reservado or EstadoDocumentoVenta.Vencido;

    public MontosComprobante MontosRedondeados() => MontosComprobante.Desde(this);

    public void RecalcularTotales()
    {
        TotalServiciosGravados = _lineas.Where(l => l.TipoItem == TipoItem.Servicio && l.PorcentajeImpuesto > 0).Sum(l => l.Subtotal);
        TotalServiciosExentos = _lineas.Where(l => l.TipoItem == TipoItem.Servicio && l.PorcentajeImpuesto == 0).Sum(l => l.Subtotal);
        TotalMercanciasGravadas = _lineas.Where(l => l.TipoItem == TipoItem.Bien && l.PorcentajeImpuesto > 0).Sum(l => l.Subtotal);
        TotalMercanciasExentas = _lineas.Where(l => l.TipoItem == TipoItem.Bien && l.PorcentajeImpuesto == 0).Sum(l => l.Subtotal);
        TotalVenta = _lineas.Sum(l => l.Subtotal + l.MontoDescuento);
        TotalDescuentos = _lineas.Sum(l => l.MontoDescuento);
        TotalImpuesto = _lineas.Sum(l => l.MontoImpuesto);
        TotalComprobante = _lineas.Sum(l => l.TotalLinea);
        TotalPagado = _pagos.Where(p => !p.Anulado).Sum(p => p.MontoAplicadoDocumento);
        SaldoPendiente = Math.Max(0m, Dinero.RedondearPago(TotalComprobante - TotalPagado));
    }

    private bool EsEditable()
    {
        if (TipoDocumento == TipoDocumentoVenta.Proforma)
        {
            return Estado == EstadoDocumentoVenta.Borrador;
        }

        return Estado is EstadoDocumentoVenta.Borrador or EstadoDocumentoVenta.Reservado or EstadoDocumentoVenta.Emitido
               && Consecutivo is null;
    }

    private ErrorOr<Success> ValidarLineasRequeridas()
    {
        if (_lineas.Count == 0)
        {
            return DocumentoVentaErrors.DetallesRequeridos;
        }

        return Result.Success;
    }

    private ErrorOr<Success> ValidarParaConfirmar(bool permitirPagoParcial = false)
    {
        var errores = new List<Error>();

        if (_lineas.Count == 0)
        {
            errores.Add(DocumentoVentaErrors.DetallesRequeridos);
        }

        var esCredito = EsCredito;
        var esApartado = TipoDocumento == TipoDocumentoVenta.Apartado;
        var esNota = TipoDocumento is TipoDocumentoVenta.NotaCredito or TipoDocumentoVenta.NotaDebito;
        var requierePagoCompleto = TipoDocumento == TipoDocumentoVenta.Factura && CondicionVentaCodigo == "01";
        var requierePagoContado = requierePagoCompleto && TotalComprobante > 0;

        if (requierePagoContado && _pagos.Count == 0)
        {
            errores.Add(DocumentoVentaErrors.PagosRequeridos);
        }

        if (TipoDocumento == TipoDocumentoVenta.Factura && _pagos.Count > 4)
        {
            errores.Add(DocumentoVentaErrors.PagosExcedenMaximo);
        }

        var totalPagadoRedondeado = Dinero.RedondearPago(TotalPagado);
        var totalComprobanteRedondeado = Dinero.RedondearPago(TotalComprobante);

        if (requierePagoContado && totalPagadoRedondeado != totalComprobanteRedondeado)
        {
            errores.Add(DocumentoVentaErrors.PagosNoCuadran);
        }

        if (!permitirPagoParcial && !requierePagoCompleto && !esApartado && totalPagadoRedondeado > totalComprobanteRedondeado)
        {
            errores.Add(DocumentoVentaErrors.PagosNoCuadran);
        }

        if (esApartado && totalPagadoRedondeado > totalComprobanteRedondeado)
        {
            errores.Add(DocumentoVentaErrors.PagoExcedeSaldo);
        }

        // Las notas de crédito/débito heredan la condición de venta del documento
        // origen por trazabilidad, pero no son ventas a crédito: se emiten al instante
        // (Estado=Emitido) y no llevan plazo. No aplican las validaciones de crédito.
        if (!esNota && esCredito && (!PlazoCreditoDias.HasValue || PlazoCreditoDias.Value <= 0))
        {
            errores.Add(DocumentoVentaErrors.PlazoCreditoInvalido);
        }

        if (!esNota && esCredito && (!ClienteId.HasValue || ClienteId.Value == Guid.Empty))
        {
            errores.Add(DocumentoVentaErrors.CreditoRequiereCliente);
        }

        if (Consecutivo is not null)
        {
            errores.Add(DocumentoVentaErrors.ConsecutivoYaAsignado);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return Result.Success;
    }

    private void AsignarConsecutivo(long numeroConsecutivo, string? consecutivoCustom = null)
    {
        NumeroConsecutivo = numeroConsecutivo;
        Consecutivo = consecutivoCustom ?? $"{Prefijo(TipoDocumento)}-{numeroConsecutivo:0000000000}";
    }

    private static string Prefijo(TipoDocumentoVenta tipoDocumento)
        => tipoDocumento switch
        {
            TipoDocumentoVenta.Factura => "FAC",
            TipoDocumentoVenta.Apartado => "APA",
            TipoDocumentoVenta.NotaCredito => "NC",
            TipoDocumentoVenta.NotaDebito => "ND",
            TipoDocumentoVenta.Proforma => "PRO",
            _ => "DOC"
        };

    private static List<Error> ValidarEncabezado(
        TipoDocumentoVenta tipoDocumento,
        string condicionVentaCodigo,
        string condicionVentaDetalleSnapshot,
        DateTime fechaDocumento,
        string monedaCodigo,
        decimal tipoCambio,
        int? plazoCreditoDias,
        string? observaciones,
        Guid? documentoOrigenId,
        DateTime? fechaVencimiento)
    {
        var errores = new List<Error>();

        if (!Enum.IsDefined(typeof(TipoDocumentoVenta), tipoDocumento))
        {
            errores.Add(DocumentoVentaErrors.TipoInvalido);
        }

        if (string.IsNullOrWhiteSpace(condicionVentaCodigo))
        {
            errores.Add(DocumentoVentaErrors.CondicionVentaRequerida);
        }
        else if (condicionVentaCodigo.Trim().Length > CondicionVentaCodigoMaxLength)
        {
            errores.Add(DocumentoVentaErrors.CondicionVentaExcedeLongitud);
        }

        if (condicionVentaDetalleSnapshot is not null && condicionVentaDetalleSnapshot.Trim().Length > CondicionVentaDetalleMaxLength)
        {
            errores.Add(Error.Validation("DocumentoVenta_CondicionVentaDetalleSnapshot", $"El detalle de condición no puede exceder {CondicionVentaDetalleMaxLength} caracteres."));
        }

        if (fechaDocumento == default)
        {
            errores.Add(DocumentoVentaErrors.FechaRequerida);
        }

        if (tipoDocumento == TipoDocumentoVenta.Apartado)
        {
            if (!fechaVencimiento.HasValue)
            {
                errores.Add(DocumentoVentaErrors.FechaVencimientoRequerida);
            }
            else if (fechaVencimiento.Value.Date < fechaDocumento.Date)
            {
                errores.Add(DocumentoVentaErrors.FechaVencimientoInvalida);
            }
        }

        if (string.IsNullOrWhiteSpace(monedaCodigo))
        {
            errores.Add(DocumentoVentaErrors.MonedaRequerida);
        }
        else if (monedaCodigo.Trim().Length > MonedaCodigoMaxLength)
        {
            errores.Add(DocumentoVentaErrors.MonedaExcedeLongitud);
        }

        if (tipoCambio <= 0)
        {
            errores.Add(DocumentoVentaErrors.TipoCambioInvalido);
        }

        if (plazoCreditoDias.HasValue && plazoCreditoDias.Value <= 0)
        {
            errores.Add(DocumentoVentaErrors.PlazoCreditoInvalido);
        }

        if (observaciones is not null && observaciones.Trim().Length > ObservacionesMaxLength)
        {
            errores.Add(Error.Validation("DocumentoVenta_Observaciones", $"Las observaciones no pueden exceder {ObservacionesMaxLength} caracteres."));
        }

        if (tipoDocumento is TipoDocumentoVenta.NotaCredito or TipoDocumentoVenta.NotaDebito)
        {
            if (!documentoOrigenId.HasValue || documentoOrigenId.Value == Guid.Empty)
            {
                errores.Add(DocumentoVentaErrors.ReferenciaRequerida);
            }
        }

        return errores;
    }
}
