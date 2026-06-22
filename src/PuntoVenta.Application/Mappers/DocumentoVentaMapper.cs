using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Common;
using PuntoVenta.Domain.Entities.Ventas;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class DocumentoVentaMapper
{
    [MapperIgnoreSource(nameof(DocumentoVenta.Cliente))]
    [MapperIgnoreSource(nameof(DocumentoVenta.Vendedor))]
    [MapperIgnoreSource(nameof(DocumentoVenta.DocumentoOrigenId))]
    [MapperIgnoreSource(nameof(DocumentoVenta.DocumentoOrigen))]
    [MapperIgnoreSource(nameof(DocumentoVenta.NumeroConsecutivo))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalServiciosGravados))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalServiciosExentos))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalMercanciasGravadas))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalMercanciasExentas))]
    [MapperIgnoreSource(nameof(DocumentoVenta.CajaId))]
    [MapperIgnoreSource(nameof(DocumentoVenta.Caja))]
    [MapperIgnoreSource(nameof(DocumentoVenta.Activo))]
    [MapperIgnoreSource(nameof(DocumentoVenta.FechaCreacion))]
    [MapperIgnoreSource(nameof(DocumentoVenta.FechaModificacion))]
    [MapperIgnoreSource(nameof(DocumentoVenta.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(DocumentoVenta.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(DocumentoVenta.UsuarioModificacion))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.ClienteNombre))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.ClienteIdentificacion))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.VendedorNombre))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.TipoDocumentoDetalle))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.TipoDocumentoColor))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.EstadoDetalle))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.EstadoColor))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.DocumentoOrigen))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.DocumentosGenerados))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.MontoRedondeo))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.MontoNotasCredito))]
    [MapperIgnoreTarget(nameof(DocumentoVentaDto.MontoNotasDebito))]
    [MapProperty([nameof(DocumentoVenta.UsuarioCreacion), nameof(Domain.Entities.Usuarios.Usuario.Nombre)], nameof(DocumentoVentaDto.CreadoPor))]
    private static partial DocumentoVentaDto ToDtoGenerated(DocumentoVenta documento);

    public static DocumentoVentaDto ToDto(DocumentoVenta documento)
        => ToDtoGenerated(documento) with
        {
            ClienteNombre = documento.Cliente?.Nombre,
            ClienteIdentificacion = documento.Cliente?.Identificacion,
            VendedorNombre = documento.Vendedor?.Nombre,
            TipoDocumentoDetalle = VentasCatalogosMapper.TipoDocumentoDetalle(documento.TipoDocumento),
            TipoDocumentoColor = VentasCatalogosMapper.TipoDocumentoColor(documento.TipoDocumento),
            EstadoDetalle = EstadoDocumentoDetalle(documento),
            EstadoColor = EstadoDocumentoColor(documento),
            TotalPagado = TotalPagado(documento),
            SaldoPendiente = SaldoPendiente(documento),
            MontoRedondeo = Dinero.Redondeo(documento.TotalComprobante),
            Pagos = [.. documento.Pagos
                .OrderBy(p => p.NumeroAbono == 0 ? int.MaxValue : p.NumeroAbono)
                .ThenBy(p => p.FechaPago)
                .ThenBy(p => p.Id)
                .Select(ToPagoDto)],
            DocumentoOrigen = documento.DocumentoOrigen is null ? null : ToRelacionadoDto(documento.DocumentoOrigen)
        };

    public static DocumentoVentaDto ToDto(DocumentoVenta documento, IReadOnlyList<DocumentoVenta> documentosGenerados)
        => ToDto(documento) with
        {
            DocumentosGenerados = [.. documentosGenerados.Select(d => ToRelacionadoDto(d))]
        };

    public static DocumentoVentaDto ToDto(
        DocumentoVenta documento,
        IReadOnlyList<DocumentoVenta> documentosGenerados,
        IReadOnlyDictionary<Guid, ConsumoNotaCreditoPorProductoDto> consumoNotasPorProducto,
        IReadOnlyDictionary<Guid, decimal> notasCreditoPorGenerado,
        decimal montoNotasCredito = 0m,
        decimal montoNotasDebito = 0m)
    {
        var baseDto = ToDto(documento, documentosGenerados);
        return baseDto with
        {
            MontoNotasCredito = montoNotasCredito,
            MontoNotasDebito = montoNotasDebito,
            SaldoPendiente = SaldoPendiente(documento, montoNotasCredito, montoNotasDebito),
            // Cada generado lleva las NCs emitidas en su contra: el frontend
            // calcula el saldo vigente de cada ND (total − NCs aplicadas).
            DocumentosGenerados = [.. documentosGenerados.Select(d =>
                ToRelacionadoDto(d, notasCreditoPorGenerado.TryGetValue(d.Id, out var nc) ? nc : 0m))],
            Lineas = [.. baseDto.Lineas.Select(linea =>
            {
                if (linea.ProductoId is null ||
                    !consumoNotasPorProducto.TryGetValue(linea.ProductoId.Value, out var consumo))
                {
                    return linea;
                }
                return linea with
                {
                    CantidadDevueltaEnNotasCredito = consumo.CantidadDevueltaInventario,
                    SubtotalAcumuladoNotasCredito = consumo.SubtotalAcumulado,
                };
            })],
        };
    }

    [MapperIgnoreSource(nameof(DocumentoVenta.Cliente))]
    [MapperIgnoreSource(nameof(DocumentoVenta.Vendedor))]
    [MapperIgnoreSource(nameof(DocumentoVenta.DocumentoOrigenId))]
    [MapperIgnoreSource(nameof(DocumentoVenta.DocumentoOrigen))]
    [MapperIgnoreSource(nameof(DocumentoVenta.PlazoCreditoDias))]
    [MapperIgnoreSource(nameof(DocumentoVenta.FechaVencimiento))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TipoCambio))]
    [MapperIgnoreSource(nameof(DocumentoVenta.NumeroConsecutivo))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalServiciosGravados))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalServiciosExentos))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalMercanciasGravadas))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalMercanciasExentas))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalVenta))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalDescuentos))]
    [MapperIgnoreSource(nameof(DocumentoVenta.TotalImpuesto))]
    [MapperIgnoreSource(nameof(DocumentoVenta.Observaciones))]
    [MapperIgnoreSource(nameof(DocumentoVenta.CajaId))]
    [MapperIgnoreSource(nameof(DocumentoVenta.Caja))]
    [MapperIgnoreSource(nameof(DocumentoVenta.Lineas))]
    [MapperIgnoreSource(nameof(DocumentoVenta.Pagos))]
    [MapperIgnoreSource(nameof(DocumentoVenta.Referencias))]
    [MapperIgnoreSource(nameof(DocumentoVenta.Activo))]
    [MapperIgnoreSource(nameof(DocumentoVenta.FechaCreacion))]
    [MapperIgnoreSource(nameof(DocumentoVenta.FechaModificacion))]
    [MapperIgnoreSource(nameof(DocumentoVenta.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(DocumentoVenta.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(DocumentoVenta.UsuarioModificacion))]
    [MapperIgnoreTarget(nameof(DocumentoVentaResumenDto.ClienteNombre))]
    [MapperIgnoreTarget(nameof(DocumentoVentaResumenDto.ClienteIdentificacion))]
    [MapperIgnoreTarget(nameof(DocumentoVentaResumenDto.VendedorNombre))]
    [MapperIgnoreTarget(nameof(DocumentoVentaResumenDto.TipoDocumentoDetalle))]
    [MapperIgnoreTarget(nameof(DocumentoVentaResumenDto.TipoDocumentoColor))]
    [MapperIgnoreTarget(nameof(DocumentoVentaResumenDto.EstadoDetalle))]
    [MapperIgnoreTarget(nameof(DocumentoVentaResumenDto.EstadoColor))]
    [MapperIgnoreTarget(nameof(DocumentoVentaResumenDto.MontoNotasCredito))]
    [MapperIgnoreTarget(nameof(DocumentoVentaResumenDto.MontoNotasDebito))]
    [MapperIgnoreTarget(nameof(DocumentoVentaResumenDto.MontoRedondeo))]
    [MapProperty([nameof(DocumentoVenta.UsuarioCreacion), nameof(Domain.Entities.Usuarios.Usuario.Nombre)], nameof(DocumentoVentaResumenDto.CreadoPor))]
    private static partial DocumentoVentaResumenDto ToResumenDtoGenerated(DocumentoVenta documento);

    public static DocumentoVentaResumenDto ToResumenDto(DocumentoVenta documento)
        => ToResumenDtoGenerated(documento) with
        {
            ClienteNombre = documento.Cliente?.Nombre,
            ClienteIdentificacion = documento.Cliente?.Identificacion,
            VendedorNombre = documento.Vendedor?.Nombre,
            TipoDocumentoDetalle = VentasCatalogosMapper.TipoDocumentoDetalle(documento.TipoDocumento),
            TipoDocumentoColor = VentasCatalogosMapper.TipoDocumentoColor(documento.TipoDocumento),
            EstadoDetalle = EstadoDocumentoDetalle(documento),
            EstadoColor = EstadoDocumentoColor(documento),
            TotalPagado = TotalPagado(documento),
            SaldoPendiente = SaldoPendiente(documento),
            MontoRedondeo = Dinero.Redondeo(documento.TotalComprobante)
        };

    public static DocumentoVentaResumenDto ToResumenDto(DocumentoVenta documento, decimal montoNotasCredito)
        => ToResumenDto(documento) with { MontoNotasCredito = montoNotasCredito };

    public static DocumentoVentaResumenDto ToResumenDto(
        DocumentoVenta documento,
        decimal montoNotasCredito,
        decimal montoNotasDebito)
        => ToResumenDto(documento) with
        {
            MontoNotasCredito = montoNotasCredito,
            MontoNotasDebito = montoNotasDebito,
            SaldoPendiente = SaldoPendiente(documento, montoNotasCredito, montoNotasDebito),
        };

    private static string EstadoDocumentoDetalle(DocumentoVenta documento)
        => documento.TipoDocumento == TipoDocumentoVenta.Proforma && documento.Estado == EstadoDocumentoVenta.Convertido
            ? "Facturada"
            : VentasCatalogosMapper.EstadoDocumentoDetalle(documento.Estado);

    private static string EstadoDocumentoColor(DocumentoVenta documento)
        => documento.TipoDocumento == TipoDocumentoVenta.Proforma && documento.Estado == EstadoDocumentoVenta.Convertido
            ? "green"
            : VentasCatalogosMapper.EstadoDocumentoColor(documento.Estado);

    private static decimal TotalPagado(DocumentoVenta documento)
        => documento.TipoDocumento == TipoDocumentoVenta.Proforma ? 0m : documento.TotalPagado;

    private static decimal SaldoPendiente(DocumentoVenta documento)
        => documento.TipoDocumento == TipoDocumentoVenta.Proforma ? 0m : documento.SaldoPendiente;

    private static decimal SaldoPendiente(
        DocumentoVenta documento,
        decimal montoNotasCredito,
        decimal montoNotasDebito)
    {
        if (documento.TipoDocumento == TipoDocumentoVenta.Proforma)
        {
            return 0m;
        }

        var saldo = documento.SaldoPendiente - montoNotasCredito + montoNotasDebito;
        return Math.Max(0m, Dinero.RedondearPago(saldo));
    }

    public static FacturaCreditoResumenDto ToFacturaCreditoResumen(DocumentoVenta documento, DateTime hoy)
    {
        var esVencida = documento.FechaVencimiento.HasValue && documento.FechaVencimiento.Value.Date < hoy.Date;
        var diasAtraso = esVencida ? (int)(hoy.Date - documento.FechaVencimiento!.Value.Date).TotalDays : 0;
        return new FacturaCreditoResumenDto(
            documento.Id,
            documento.Consecutivo,
            documento.FechaDocumento,
            documento.FechaVencimiento,
            documento.PlazoCreditoDias,
            documento.ClienteId,
            documento.Cliente?.Nombre,
            documento.Cliente?.Identificacion,
            documento.CondicionVentaCodigo,
            documento.CondicionVentaDetalleSnapshot,
            documento.TotalComprobante,
            documento.TotalPagado,
            documento.SaldoPendiente,
            diasAtraso,
            esVencida);
    }

    private static DocumentoVentaRelacionadoDto ToRelacionadoDto(DocumentoVenta documento)
        => ToRelacionadoDto(documento, 0m);

    private static DocumentoVentaRelacionadoDto ToRelacionadoDto(DocumentoVenta documento, decimal montoNotasCreditoAplicadas)
        => new()
        {
            Id = documento.Id,
            TipoDocumento = documento.TipoDocumento,
            Estado = documento.Estado,
            Consecutivo = documento.Consecutivo,
            FechaDocumento = documento.FechaDocumento,
            TipoDocumentoDetalle = VentasCatalogosMapper.TipoDocumentoDetalle(documento.TipoDocumento),
            TipoDocumentoColor = VentasCatalogosMapper.TipoDocumentoColor(documento.TipoDocumento),
            EstadoDetalle = EstadoDocumentoDetalle(documento),
            EstadoColor = EstadoDocumentoColor(documento),
            TotalComprobante = documento.TotalComprobante,
            TotalPagado = TotalPagado(documento),
            MonedaCodigo = documento.MonedaCodigo,
            MontoNotasCreditoAplicadas = montoNotasCreditoAplicadas
        };

    [MapperIgnoreSource(nameof(DocumentoVentaLinea.DocumentoVenta))]
    [MapperIgnoreSource(nameof(DocumentoVentaLinea.Producto))]
    [MapperIgnoreSource(nameof(DocumentoVentaLinea.DocumentoVentaId))]
    [MapperIgnoreSource(nameof(DocumentoVentaLinea.TarifaIvaImpuestoCodigo))]
    [MapperIgnoreSource(nameof(DocumentoVentaLinea.PorcentajeImpuesto))]
    [MapperIgnoreTarget(nameof(DocumentoVentaLineaDto.CantidadDevueltaEnNotasCredito))]
    [MapperIgnoreTarget(nameof(DocumentoVentaLineaDto.SubtotalAcumuladoNotasCredito))]
    private static partial DocumentoVentaLineaDto ToLineaDto(DocumentoVentaLinea linea);

    private static DocumentoVentaPagoDto ToPagoDto(DocumentoVentaPago pago)
        => new()
        {
            Id = pago.Id,
            NumeroAbono = pago.NumeroAbono,
            MonedaCodigo = pago.MonedaCodigo,
            TipoCambioAplicado = pago.TipoCambioAplicado,
            MedioPagoCodigo = pago.MedioPagoCodigo,
            MedioPagoDetalleSnapshot = pago.MedioPagoDetalleSnapshot,
            MontoEntregado = pago.MontoEntregado,
            MontoAplicadoMonedaPago = pago.MontoAplicadoMonedaPago,
            MontoAplicadoDocumento = pago.MontoAplicadoDocumento,
            MontoVueltoMonedaPago = pago.MontoVueltoMonedaPago,
            MontoVueltoDocumento = pago.MontoVueltoDocumento,
            FechaPago = pago.FechaPago,
            FechaRegistroUtc = pago.FechaRegistroUtc,
            UsuarioRegistroId = pago.UsuarioRegistroId,
            UsuarioRegistroNombre = pago.UsuarioRegistro?.Nombre,
            Referencia = pago.Referencia,
            Observacion = pago.Observacion,
            Anulado = pago.Anulado,
            FechaAnulacionUtc = pago.FechaAnulacionUtc,
            UsuarioAnulaId = pago.UsuarioAnulaId,
            UsuarioAnulaNombre = pago.UsuarioAnula?.Nombre,
            MotivoAnulacion = pago.MotivoAnulacion
        };

    [MapperIgnoreSource(nameof(DocumentoVentaReferencia.DocumentoVenta))]
    [MapperIgnoreSource(nameof(DocumentoVentaReferencia.DocumentoReferencia))]
    [MapperIgnoreSource(nameof(DocumentoVentaReferencia.DocumentoVentaId))]
    private static partial DocumentoVentaReferenciaDto ToReferenciaDto(DocumentoVentaReferencia referencia);
}
