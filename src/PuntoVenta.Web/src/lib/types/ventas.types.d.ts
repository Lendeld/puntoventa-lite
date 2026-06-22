import type { ActionResult, FormValues } from "@lib/types/base.types";
import type { crearBorradorFacturaSchema } from "@lib/schemas/ventas.schema";

export interface DocumentoVentaLineaForm {
    Id?: string;
    ProductoId: string;
    TipoItem: "Bien" | "Servicio";
    Codigo: string;
    Descripcion: string;
    Cantidad: number;
    PrecioUnitario: number;
    MontoDescuento: number;
    PrecioUnitarioBaseCrc?: number;
    MontoDescuentoBaseCrc?: number;
    TarifaIvaImpuestoCodigo: string | null;
    PorcentajeImpuesto: number;
    PermiteModificarPrecioUnitario?: boolean;
    NoAplicaExistencias?: boolean;
}

export interface DocumentoVentaPagoForm {
    MonedaCodigo: string;
    TipoCambioAplicado: number;
    MedioPagoCodigo: string;
    MontoEntregado: number;
    MontoAplicadoMonedaPago: number;
    MontoAplicadoDocumento: number;
    MontoVueltoMonedaPago: number;
    MontoVueltoDocumento: number;
    Referencia: string;
    Observacion: string;
}

export interface CrearBorradorFacturaPayload {
    clienteId: string | null;
    vendedorId: string | null;
    cajaId?: string | null;
    condicionVentaCodigo: string;
    lineas: Array<{
        id?: string | null;
        productoId: string;
        cantidad: number;
        precioUnitario?: number;
        montoDescuento: number;
        devuelveInventario: boolean;
        descripcion?: string | null;
    }>;
    pagos: Array<{
        monedaCodigo: string;
        tipoCambioAplicado: number;
        medioPagoCodigo: string;
        montoEntregado: number;
        montoAplicadoMonedaPago: number;
        montoAplicadoDocumento: number;
        montoVueltoMonedaPago: number;
        montoVueltoDocumento: number;
        referencia: string | null;
        observacion: string | null;
    }>;
    plazoCreditoDias: number | null;
    fechaDocumento: string;
    fechaVencimiento?: string | null;
    monedaCodigo: string;
    tipoCambio: number;
    observaciones: string | null;
}

export type CrearProformaPayload = Omit<CrearBorradorFacturaPayload, "pagos">;

export interface EmitirFacturaPayload {
    cajaId?: string | null;
    pagos: Array<{
        monedaCodigo: string;
        tipoCambioAplicado: number;
        medioPagoCodigo: string;
        montoEntregado: number;
        montoAplicadoMonedaPago: number;
        montoAplicadoDocumento: number;
        montoVueltoMonedaPago: number;
        montoVueltoDocumento: number;
        referencia: string | null;
        observacion: string | null;
    }>;
}

export type FacturarProformaPayload = EmitirFacturaPayload;

export interface AbonarApartadoPayload {
    fechaPago?: string | null;
    pago: EmitirFacturaPayload["pagos"][number];
}

export interface ExtenderVencimientoApartadoPayload {
    fechaVencimiento: string;
}

export interface DocumentoVentaResumenDto {
    id: string;
    tipoDocumento: string;
    estado: string;
    clienteId: string | null;
    clienteNombre: string | null;
    clienteIdentificacion: string | null;
    vendedorId: string | null;
    vendedorNombre: string | null;
    consecutivo: string | null;
    fechaDocumento: string;
    tipoDocumentoDetalle: string;
    tipoDocumentoColor: string;
    estadoDetalle: string;
    estadoColor: string;
    condicionVentaCodigo: string;
    condicionVentaDetalleSnapshot: string;
    totalComprobante: number;
    totalPagado: number;
    saldoPendiente: number;
    monedaCodigo: string;
    creadoPor: string | null;
    esCredito: boolean;
    montoNotasCredito: number;
    montoNotasDebito: number;
    montoRedondeo: number;
}

export interface VentaCatalogoItemDto {
    valor: number;
    detalle: string;
    color: string;
}

export interface VentasCatalogosDto {
    tiposDocumento: VentaCatalogoItemDto[];
    estadosDocumento: VentaCatalogoItemDto[];
}

export interface DocumentoVentaPagoDto {
    id: string;
    numeroAbono: number;
    monedaCodigo: string;
    tipoCambioAplicado: number;
    medioPagoCodigo: string;
    medioPagoDetalleSnapshot: string;
    montoEntregado: number;
    montoAplicadoMonedaPago: number;
    montoAplicadoDocumento: number;
    montoVueltoMonedaPago: number;
    montoVueltoDocumento: number;
    fechaPago: string;
    fechaRegistroUtc: string;
    usuarioRegistroId: string | null;
    usuarioRegistroNombre: string | null;
    referencia: string | null;
    observacion: string | null;
    anulado: boolean;
    fechaAnulacionUtc: string | null;
    usuarioAnulaId: string | null;
    usuarioAnulaNombre: string | null;
    motivoAnulacion: string | null;
}

export interface DocumentoVentaLineaDto {
    id: string;
    productoId: string | null;
    tipoItem: "Bien" | "Servicio";
    codigo: string;
    descripcion: string;
    unidadMedidaCodigo: string;
    cantidad: number;
    precioUnitario: number;
    montoDescuento: number;
    subtotal: number;
    montoImpuesto: number;
    totalLinea: number;
    devuelveInventario: boolean;
    noAplicaExistencias: boolean;
    permiteModificarPrecioUnitario: boolean;
    cantidadDevueltaEnNotasCredito: number;
    subtotalAcumuladoNotasCredito: number;
}

export interface DocumentoVentaRelacionadoDto {
    id: string;
    tipoDocumento: string;
    estado: string;
    consecutivo: string | null;
    fechaDocumento: string;
    tipoDocumentoDetalle: string;
    tipoDocumentoColor: string;
    estadoDetalle: string;
    estadoColor: string;
    totalComprobante: number;
    totalPagado: number;
    monedaCodigo: string;
    montoNotasCreditoAplicadas: number;
}

export interface DocumentoVentaDto {
    id: string;
    tipoDocumento: string;
    estado: string;
    clienteId: string | null;
    clienteNombre: string | null;
    clienteIdentificacion: string | null;
    vendedorId: string | null;
    vendedorNombre: string | null;
    consecutivo: string | null;
    fechaDocumento: string;
    tipoDocumentoDetalle: string;
    tipoDocumentoColor: string;
    estadoDetalle: string;
    estadoColor: string;
    condicionVentaCodigo: string;
    condicionVentaDetalleSnapshot: string;
    totalComprobante: number;
    totalPagado: number;
    saldoPendiente: number;
    montoRedondeo: number;
    plazoCreditoDias: number | null;
    fechaVencimiento: string | null;
    monedaCodigo: string;
    tipoCambio: number;
    totalVenta: number;
    totalDescuentos: number;
    totalImpuesto: number;
    observaciones: string | null;
    fechaCancelacion: string | null;
    esCredito: boolean;
    creadoPor: string | null;
    lineas: DocumentoVentaLineaDto[];
    pagos: DocumentoVentaPagoDto[];
    referencias: Array<{
        id: string;
        documentoReferenciaId: string;
        tipoDocReferencia: string;
        fechaDocumentoReferencia: string;
        razon: string;
    }>;
    documentoOrigen: DocumentoVentaRelacionadoDto | null;
    documentosGenerados: DocumentoVentaRelacionadoDto[];
}

export interface ObtenerDocumentosVentaPaginadoParams {
    numeroPagina: number;
    tamanoPagina: number;
    filtroDinamico?: string;
    tipoDocumento?: number;
    estado?: number;
    clienteId?: string;
    fechaDesde?: string;
    fechaHasta?: string;
}

export interface FacturaTotales {
    subtotal: number;
    descuentos: number;
    impuesto: number;
    total: number;
    pagado: number;
    saldo: number;
}

export interface GuardarBorradorFacturaResult {
    id: string;
    detalle: DocumentoVentaDto | null;
}

export interface EmitirFacturaResult {
    id: string;
    detalle: DocumentoVentaDto | null;
}

export interface VentaActionResult<T = unknown> extends ActionResult {
    data: T | null;
}

export type CrearBorradorFacturaFormValues = FormValues<typeof crearBorradorFacturaSchema>;

export interface FacturaCreditoResumenDto {
    id: string;
    consecutivo: string | null;
    fechaDocumento: string;
    fechaVencimiento: string | null;
    plazoCreditoDias: number | null;
    clienteId: string | null;
    clienteNombre: string | null;
    clienteIdentificacion: string | null;
    condicionVentaCodigo: string;
    condicionVentaDetalleSnapshot: string;
    totalComprobante: number;
    totalPagado: number;
    saldoPendiente: number;
    diasAtraso: number;
    esVencida: boolean;
}

export interface ObtenerFacturasCreditoParams {
    pagina?: number;
    tamano?: number;
    filtro?: string;
    clienteId?: string;
    soloVencidas?: boolean;
}

export interface EmitirNotaCreditoLineaPayload {
    productoId: string;
    cantidad: number;
    precioUnitario?: number | null;
    montoDescuento?: number;
    descripcion?: string | null;
}

export type ModoNotaCreditoCode = 0 | 1 | 2;

export interface EmitirNotaCreditoPayload {
    documentoOrigenId: string;
    modo: ModoNotaCreditoCode;
    lineas: EmitirNotaCreditoLineaPayload[];
    razon: string;
    fechaDocumento?: string | null;
    observaciones?: string | null;
    productosSinReintegro?: string[];
}

// La ND reutiliza la misma forma de línea que la NC (mismo aggregate
// DocumentoVenta en el backend). Sin "modo": la ND es siempre cargo adicional.
export interface EmitirNotaDebitoPayload {
    documentoOrigenId: string;
    lineas: EmitirNotaCreditoLineaPayload[];
    razon: string;
    fechaDocumento?: string | null;
    observaciones?: string | null;
}

export interface RegistrarAbonoFacturaResult {
    pagoId: string;
}

export interface AnularAbonoFacturaResult {
    pagoId: string;
}

export interface RegistrarAbonoFacturaRequest {
    pago: {
        monedaCodigo: string;
        tipoCambioAplicado: number;
        medioPagoCodigo: string;
        montoEntregado: number;
        montoAplicadoMonedaPago: number;
        montoAplicadoDocumento: number;
        montoVueltoMonedaPago: number;
        montoVueltoDocumento: number;
        referencia?: string | null;
        observacion?: string | null;
    };
    fechaPago?: string | null;
}

export interface DocumentoVentaEventoDto {
    id: string;
    documentoVentaId: string;
    tipoCodigo: string;
    tipoNombre: string;
    categoria: string;
    iconoSugerido: string | null;
    colorSugerido: string | null;
    ocurridoEn: string;
    usuarioId: string | null;
    usuarioNombre: string | null;
    resumen: string;
    correlacionId: string | null;
}

export interface DocumentoVentaEventoListaDto {
    items: DocumentoVentaEventoDto[];
    total: number;
    skip: number;
    take: number;
}

export interface TicketLineaDto {
    codigo: string;
    descripcion: string;
    cantidad: number;
    unidadMedidaCodigo: string;
    precioUnitario: number;
    descuento: number;
    porcentajeImpuesto: number;
    total: number;
}

export interface TicketPagoDto {
    id: string;
    fechaUtc: string;
    medioPagoDetalle: string;
    monedaCodigo: string;
    montoAplicado: number;
    montoEntregado: number;
    montoVuelto: number;
    referencia: string | null;
    numeroAbono: number;
    fechaRegistroUtc: string | null;
    anulado: boolean;
    fechaAnulacionUtc: string | null;
    usuarioAnulaNombre: string | null;
    motivoAnulacion: string | null;
}

export interface TicketDataDto {
    encabezado: string;
    direccion: string | null;
    identificacionFiscal: string | null;
    telefono: string | null;
    correo: string | null;
    logoUrl: string | null;
    mostrarLogo: boolean;
    tipoDocumento: string;
    consecutivo: string;
    fechaUtc: string;
    cajaCodigo: string | null;
    cajaNombre: string | null;
    vendedorNombre: string | null;
    condicionVentaDetalle: string;
    clienteNombre: string;
    clienteIdentificacion: string | null;
    lineas: TicketLineaDto[];
    pagos: TicketPagoDto[];
    subtotal: number;
    descuentos: number;
    impuestos: number;
    total: number;
    pagado: number;
    saldo: number;
    monedaCodigo: string;
    tipoCambio: number;
    mensajePie: string | null;
    observaciones: string | null;
    aplicaCopiaClienteNegocio: boolean;
    mostrarCodigoBarras: boolean;
    lineasPie: TicketLineaPieDto[];
    referenciaTipoDocumento: string | null;
    referenciaConsecutivo: string | null;
    referenciaRazon: string | null;
    lineasEncabezado: TicketEncabezadoLineaDto[] | null;
    esRecibo: boolean;
    saldoAnterior: number;
    saldoNuevo: number;
    esReciboAnulado: boolean;
    fechaAnulacionUtc: string | null;
    usuarioAnulaNombre: string | null;
    motivoAnulacion: string | null;
}

export interface TicketLineaPieDto {
    texto: string;
    alineacion: "Izquierda" | "Centro" | "Derecha";
    negrita: boolean;
}

export interface TicketEncabezadoLineaDto {
    texto: string;
    negrita: boolean;
}

// Reporte de ventas por rango — espejo de los DTO del backend. Los montos ya
// vienen colonizados (si aplica) y con signo NC resuelto por el handler.
export interface ReporteVentasRangoFila {
    documentoId: string;
    consecutivo: string;
    fechaFactura: string;
    clienteIdentificacion: string;
    clienteNombre: string;
    medioPago: string;
    condicionVenta: string;
    monedaCodigo: string;
    tipoCambio: number;
    numeroLinea: number;
    productoCodigo: string;
    productoDetalle: string;
    cantidad: number;
    precioUnitario: number;
    descuento: number;
    subtotal: number;
    tarifaPorcentaje: number;
    montoImpuesto: number;
    totalLinea: number;
    esColonizado: boolean;
    esNotaCredito: boolean;
}

export interface ReporteVentasRangoResumenFila {
    documentoId: string;
    consecutivo: string;
    fechaFactura: string;
    clienteIdentificacion: string;
    clienteNombre: string;
    medioPago: string;
    condicionVenta: string;
    monedaCodigo: string;
    tipoCambio: number;
    descuento: number;
    subtotal: number;
    montoImpuesto: number;
    totalDocumento: number;
    esColonizado: boolean;
    esNotaCredito: boolean;
}

export interface ReporteVentasRangoResultado {
    detallado: boolean;
    colonizado: boolean;
    filas: ReporteVentasRangoFila[];
    resumen: ReporteVentasRangoResumenFila[];
    totalSubtotal: number;
    totalDescuento: number;
    totalImpuesto: number;
    totalGeneral: number;
}

export interface ObtenerReporteVentasRangoParams {
    fechaDesde: string;
    fechaHasta: string;
    consecutivo?: string;
    colonizar?: boolean;
    detallado?: boolean;
}

export interface MovimientoDineroFilaDto {
    pagoId: string;
    documentoId: string;
    documentoConsecutivo: string | null;
    tipoMovimiento: string;
    tipoDocumento: string;
    fechaMovimientoUtc: string;
    fechaInformativaUtc: string;
    fechaRegistroUtc: string;
    fechaAnulacionUtc: string | null;
    cajaId: string | null;
    cajaCodigo: string | null;
    cajaNombre: string | null;
    clienteId: string | null;
    clienteNombre: string | null;
    clienteIdentificacion: string | null;
    usuarioId: string | null;
    usuarioNombre: string | null;
    medioPagoCodigo: string;
    medioPagoDetalle: string;
    referencia: string | null;
    monedaCodigo: string;
    monto: number;
    numeroAbono: number;
    motivoAnulacion: string | null;
    eventoId: string | null;
    eventoTipoCodigo: string | null;
    eventoResumen: string | null;
    eventoOcurridoEn: string | null;
}

export interface MovimientoDineroMedioDto {
    codigo: string;
    detalle: string;
    entradas: number;
    salidas: number;
    neto: number;
}

export interface ReporteMovimientosDineroResultado {
    movimientos: MovimientoDineroFilaDto[];
    totalesPorMedio: MovimientoDineroMedioDto[];
    totalEntradas: number;
    totalSalidas: number;
    totalNeto: number;
}

export interface ObtenerReporteMovimientosDineroParams {
    fechaDesde: string;
    fechaHasta: string;
    cajaId?: string | null;
}
