using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Negocios;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerTicketDataQuery(Guid DocumentoId, Guid? PagoId)
    : IRequest<ErrorOr<TicketDataDto>>;

public sealed class ObtenerTicketDataHandler(
    IDocumentoVentaRepository documentoRepository,
    INegocioRepository negocioRepository,
    INegocioTicketConfigRepository ticketConfigRepository) : IRequestHandler<ObtenerTicketDataQuery, ErrorOr<TicketDataDto>>
{
    private const string ClienteSinIdentificarNombre = "Cliente contado";

    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly INegocioRepository _negocioRepository = negocioRepository;
    private readonly INegocioTicketConfigRepository _ticketConfigRepository = ticketConfigRepository;

    public async ValueTask<ErrorOr<TicketDataDto>> Handle(
        ObtenerTicketDataQuery query,
        CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.ObtenerDetalleAsync(query.DocumentoId, cancellationToken);
        if (documento is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        var negocio = await _negocioRepository.ObtenerAsync(cancellationToken);
        if (negocio is null)
        {
            return NegocioErrors.NoEncontrado;
        }

        var config = await _ticketConfigRepository.ObtenerAsync(cancellationToken);
        var mostrarLogo = config?.MostrarLogo ?? true;
        var aplicaCopiaClienteNegocio = config?.AplicaCopiaClienteNegocio ?? false;
        var mostrarCodigoBarras = config?.MostrarCodigoBarras ?? true;
        var mensajePie = config?.MensajePie;

        var lineasPie = (config?.ResolverLineas(DestinoLineaPie.Ticket, documento.TipoDocumento) ?? [])
            .Select(l => new TicketLineaPieDto(l.Texto, l.Alineacion, l.Negrita))
            .ToList();

        var esRecibo = query.PagoId.HasValue;
        var montoNotasCredito = documento.TipoDocumento == TipoDocumentoVenta.Factura
            ? await _documentoRepository.ObtenerMontoNotasEmitidasAsync(documento.Id, TipoDocumentoVenta.NotaCredito, cancellationToken)
            : 0m;
        var saldoBase = Math.Max(0m, documento.TotalComprobante - montoNotasCredito);
        var pagosFiltrados = query.PagoId.HasValue
            ? documento.Pagos.Where(p => p.Id == query.PagoId.Value).ToList()
            : [.. documento.Pagos.Where(p => !p.Anulado)];

        if (query.PagoId.HasValue && pagosFiltrados.Count == 0)
        {
            return DocumentoVentaErrors.PagoNoEncontrado;
        }

        if (esRecibo
            && documento.TipoDocumento != TipoDocumentoVenta.Apartado
            && !(documento.TipoDocumento == TipoDocumentoVenta.Factura && documento.EsCredito))
        {
            return DocumentoVentaErrors.DocumentoNoEmiteReciboAbono;
        }

        var esReciboAnulado = false;
        decimal saldoAnterior = 0m;
        decimal saldoNuevo = 0m;
        DateTime? fechaAnulacionUtc = null;
        string? usuarioAnulaNombre = null;
        string? motivoAnulacion = null;

        if (esRecibo)
        {
            var pagoRecibo = pagosFiltrados[0];
            var pagosOrdenados = documento.Pagos
                .Where(p => !p.Anulado)
                .OrderBy(p => p.NumeroAbono == 0 ? int.MaxValue : p.NumeroAbono)
                .ThenBy(p => p.FechaPago)
                .ThenBy(p => p.Id)
                .ToList();
            var pagadoAntes = pagosOrdenados
                .TakeWhile(p => p.Id != pagoRecibo.Id)
                .Sum(p => p.MontoAplicadoDocumento);

            if (pagoRecibo.Anulado)
            {
                esReciboAnulado = true;
                fechaAnulacionUtc = pagoRecibo.FechaAnulacionUtc;
                usuarioAnulaNombre = pagoRecibo.UsuarioAnula?.Nombre;
                motivoAnulacion = pagoRecibo.MotivoAnulacion;
                saldoNuevo = Math.Max(0m, documento.SaldoPendiente - montoNotasCredito);
                saldoAnterior = Math.Max(0m, saldoNuevo - pagoRecibo.MontoAplicadoDocumento);
            }
            else
            {
                saldoAnterior = Math.Max(0m, saldoBase - pagadoAntes);
                saldoNuevo = Math.Max(0m, saldoAnterior - pagoRecibo.MontoAplicadoDocumento);
            }
        }

        List<TicketLineaDto> lineas = esRecibo
            ? []
            : documento.Lineas
                .Select(l => new TicketLineaDto(
                    l.Codigo,
                    l.Descripcion,
                    l.Cantidad,
                    l.UnidadMedidaCodigo,
                    l.PrecioUnitario,
                    l.MontoDescuento,
                    l.PorcentajeImpuesto,
                    l.TotalLinea))
                .ToList();

        var pagos = pagosFiltrados
            .Select(p => new TicketPagoDto(
                p.Id,
                p.FechaPago,
                p.MedioPagoDetalleSnapshot,
                p.MonedaCodigo,
                p.MontoAplicadoDocumento,
                p.MontoEntregado,
                p.MontoVueltoMonedaPago,
                p.Referencia,
                NumeroAbono: p.NumeroAbono,
                FechaRegistroUtc: p.FechaRegistroUtc,
                Anulado: p.Anulado,
                FechaAnulacionUtc: p.FechaAnulacionUtc,
                UsuarioAnulaNombre: p.UsuarioAnula?.Nombre,
                MotivoAnulacion: p.MotivoAnulacion))
            .ToList();

        var tipoDoc = documento.TipoDocumento switch
        {
            TipoDocumentoVenta.Factura => "Factura",
            TipoDocumentoVenta.Apartado => "Apartado",
            TipoDocumentoVenta.NotaCredito => "Nota de crédito",
            TipoDocumentoVenta.NotaDebito => "Nota de débito",
            TipoDocumentoVenta.Proforma => "Proforma",
            _ => "Documento",
        };

        var encabezado = string.IsNullOrWhiteSpace(negocio.NombreComercial)
            ? negocio.Nombre
            : negocio.NombreComercial!;

        var lineasEncabezado = config?.ElementosEncabezado.Count > 0
            ? config.ResolverEncabezado(
                    negocio.Nombre,
                    negocio.NombreComercial,
                    negocio.Identificacion,
                    negocio.Telefono,
                    negocio.Correo,
                    negocio.Direccion,
                    FormatoFechaCR(documento.FechaDocumento))
                .Select(x => new TicketEncabezadoLineaDto(x.Texto, x.Negrita))
                .ToList()
            : null;

        var referencia = documento.Referencias.FirstOrDefault();
        var refTipoDocumento = referencia is null || documento.DocumentoOrigen is null
            ? null
            : documento.DocumentoOrigen.TipoDocumento switch
            {
                TipoDocumentoVenta.Factura => "Factura",
                TipoDocumentoVenta.Apartado => "Apartado",
                TipoDocumentoVenta.NotaCredito => "Nota de crédito",
                TipoDocumentoVenta.NotaDebito => "Nota de débito",
                TipoDocumentoVenta.Proforma => "Proforma",
                _ => null,
            };

        var dto = new TicketDataDto(
            Encabezado: encabezado,
            Direccion: negocio.Direccion,
            IdentificacionFiscal: negocio.Identificacion,
            Telefono: negocio.Telefono,
            Correo: negocio.Correo,
            LogoUrl: negocio.LogoUrl,
            MostrarLogo: mostrarLogo,
            TipoDocumento: tipoDoc,
            Consecutivo: documento.Consecutivo ?? string.Empty,
            FechaUtc: documento.FechaDocumento,
            CajaCodigo: documento.Caja?.Codigo,
            CajaNombre: documento.Caja?.Nombre,
            VendedorNombre: documento.Vendedor?.Nombre,
            CondicionVentaDetalle: documento.CondicionVentaDetalleSnapshot,
            ClienteNombre: documento.Cliente?.Nombre ?? ClienteSinIdentificarNombre,
            ClienteIdentificacion: documento.Cliente?.Identificacion,
            Lineas: lineas,
            Pagos: pagos,
            Subtotal: documento.TotalVenta,
            Descuentos: documento.TotalDescuentos,
            Impuestos: documento.TotalImpuesto,
            Total: documento.TotalComprobante,
            Pagado: documento.TotalPagado,
            Saldo: Math.Max(0m, documento.SaldoPendiente - montoNotasCredito),
            MonedaCodigo: documento.MonedaCodigo,
            TipoCambio: documento.TipoCambio,
            MensajePie: mensajePie,
            Observaciones: documento.Observaciones,
            AplicaCopiaClienteNegocio: aplicaCopiaClienteNegocio,
            MostrarCodigoBarras: mostrarCodigoBarras,
            LineasPie: lineasPie,
            ReferenciaTipoDocumento: refTipoDocumento,
            ReferenciaConsecutivo: documento.DocumentoOrigen?.Consecutivo,
            ReferenciaRazon: referencia?.Razon,
            LineasEncabezado: lineasEncabezado,
            EsRecibo: esRecibo,
            SaldoAnterior: saldoAnterior,
            SaldoNuevo: saldoNuevo,
            EsReciboAnulado: esReciboAnulado,
            FechaAnulacionUtc: fechaAnulacionUtc,
            UsuarioAnulaNombre: usuarioAnulaNombre,
            MotivoAnulacion: motivoAnulacion);

        return dto;
    }

    private static string FormatoFechaCR(DateTime utc)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
        var fechaUtc = utc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(utc, DateTimeKind.Utc)
            : utc.ToUniversalTime();
        var local = TimeZoneInfo.ConvertTimeFromUtc(fechaUtc, tz);
        return local.ToString("dd/MM/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
    }
}
