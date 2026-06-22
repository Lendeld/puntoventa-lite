using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Clientes;
using PuntoVenta.Domain.Entities.Negocios;
using PuntoVenta.Domain.Entities.Ventas;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PuntoVenta.Infrastructure.Services;

// PDF de comprobante de venta y recibo de abono usando QuestPDF in-process.
// Reemplaza Gotenberg en ambos modos (Cloud y LocalHost) — sin HTTP, sin Docker.
// El color de acento es fijo por ahora; cuando Negocio incorpore ColorMarca se
// lee de ahí (parametrizar es trivial, solo cambia AccentColor).
public sealed class QuestPdfDocumentoVentaService : IDocumentoVentaPdfService
{
    private const long LogoCacheSizeLimitBytes = 50 * 1024 * 1024;
    private static readonly TimeSpan LogoCacheSlidingExpiration = TimeSpan.FromHours(2);
    private static readonly TimeZoneInfo ZonaCR = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    private static string FormatoFechaCR(DateTime utc)
    {
        var kindUtc = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(kindUtc, ZonaCR);
        return local.ToString("dd/MM/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
    }

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<QuestPdfDocumentoVentaService> _logger;
    private readonly MemoryCache _logoCache = new(new MemoryCacheOptions
    {
        SizeLimit = LogoCacheSizeLimitBytes
    });

    public QuestPdfDocumentoVentaService(
        IHttpClientFactory httpClientFactory,
        ILogger<QuestPdfDocumentoVentaService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private const string AccentColor = "#475569";
    private const string AccentSoft = "#e2e8f0";
    private const string TextPrimary = "#1e293b";
    private const string TextMuted = "#64748b";
    private const string BorderColor = "#e2e8f0";
    private const string BackgroundSoft = "#f8fafc";
    private const string ZebraRow = "#f1f5f9";
    private const string SaldoColor = "#dc2626";

    public async Task<byte[]> GenerarPdfAsync(
        DocumentoVenta documento,
        Negocio negocio,
        NegocioTicketConfig? ticketConfig,
        CancellationToken cancellationToken = default)
    {
        var lineasPie = (ticketConfig?.ResolverLineas(DestinoLineaPie.Pdf, documento.TipoDocumento) ?? [])
            .ToList();
        var mostrarCodigoBarras = ticketConfig?.MostrarCodigoBarras ?? true;
        var logoBytes = await ObtenerLogoBytesAsync(negocio.LogoUrl, cancellationToken);
        var titulo = TituloDocumento(documento);
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurarPagina(page);
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Element(c => RenderHeader(c, documento, negocio, logoBytes));
                    col.Item().Element(c => RenderInfoEncabezado(c, documento));
                    if (documento.Referencias.Count > 0)
                    {
                        col.Item().Element(c => RenderReferenciaOrigen(c, documento));
                    }
                    if (documento.Cliente is not null)
                    {
                        col.Item().Element(c => RenderClienteInfo(c, documento.Cliente));
                    }
                    col.Item().Element(c => RenderLineas(c, documento));
                    col.Item().Element(c => RenderTotales(c, documento));
                    if (documento.Pagos.Any(p => !p.Anulado))
                    {
                        col.Item().Element(c => RenderPagos(c, documento));
                    }
                    if (!string.IsNullOrWhiteSpace(documento.Observaciones))
                    {
                        col.Item().Element(c => RenderObservaciones(c, documento));
                    }

                    if (lineasPie.Count > 0)
                    {
                        col.Item().Element(c => RenderLineasPie(c, lineasPie));
                    }
                    if (documento.TipoDocumento == TipoDocumentoVenta.Proforma)
                    {
                        col.Item().PaddingTop(6).Element(RenderProformaNota);
                    }
                    if (mostrarCodigoBarras && !string.IsNullOrWhiteSpace(documento.Consecutivo))
                    {
                        col.Item().Element(c => RenderCodigoBarras(c, documento.Consecutivo!));
                    }
                    col.Item().Element(RenderResolucionLegal);
                });
            });
        })
        .WithMetadata(new DocumentMetadata { Title = titulo })
        .GeneratePdf();

        return pdf;
    }

    public async Task<byte[]> GenerarReciboPagoPdfAsync(
        DocumentoVenta documento,
        DocumentoVentaPago pago,
        Negocio negocio,
        NegocioTicketConfig? ticketConfig,
        decimal montoNotasCreditoAplicadas = 0m,
        CancellationToken cancellationToken = default)
    {
        var mostrarCodigoBarras = ticketConfig?.MostrarCodigoBarras ?? true;
        var pagosOrdenados = documento.Pagos
            .Where(p => !p.Anulado)
            .OrderBy(p => p.NumeroAbono == 0 ? int.MaxValue : p.NumeroAbono)
            .ThenBy(p => p.FechaPago)
            .ThenBy(p => p.Id)
            .ToList();
        var pagadoAntes = pagosOrdenados.TakeWhile(p => p.Id != pago.Id).Sum(p => p.MontoAplicadoDocumento);
        var saldoBase = Math.Max(0m, documento.TotalComprobante - montoNotasCreditoAplicadas);
        decimal saldoAnterior;
        decimal saldoNuevo;
        if (pago.Anulado)
        {
            saldoNuevo = Math.Max(0m, documento.SaldoPendiente - montoNotasCreditoAplicadas);
            saldoAnterior = Math.Max(0, saldoNuevo - pago.MontoAplicadoDocumento);
        }
        else
        {
            saldoAnterior = Math.Max(0, saldoBase - pagadoAntes);
            saldoNuevo = Math.Max(0, saldoAnterior - pago.MontoAplicadoDocumento);
        }

        var logoBytes = await ObtenerLogoBytesAsync(negocio.LogoUrl, cancellationToken);
        var consecutivoLabel = string.IsNullOrWhiteSpace(documento.Consecutivo)
            ? documento.Id.ToString("N")[..8]
            : documento.Consecutivo;
        var numeroAbonoLabel = pago.NumeroAbono > 0 ? $" #{pago.NumeroAbono}" : string.Empty;
        var esAnulado = pago.Anulado;
        var tituloRecibo = esAnulado
            ? $"Anulación de abono{numeroAbonoLabel} — {consecutivoLabel}"
            : $"Recibo de pago{numeroAbonoLabel} — {consecutivoLabel}";
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurarPagina(page);
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Element(c => RenderHeaderRecibo(c, documento, negocio, logoBytes, esAnulado, pago.NumeroAbono));
                    col.Item().Element(c => RenderInfoRecibo(c, documento, pago));
                    if (esAnulado)
                    {
                        col.Item().Element(c => RenderBloqueAnulacion(c, pago));
                    }
                    col.Item().Element(c => RenderMontoRecibido(c, documento, pago, esAnulado));
                    col.Item().Element(SectionTitle(esAnulado ? "Estado de cuenta actual" : "Estado de cuenta"));
                    col.Item().Element(c => RenderEstadoCuenta(c, documento, pago, saldoAnterior, saldoNuevo, esAnulado));
                    if (mostrarCodigoBarras && !string.IsNullOrWhiteSpace(documento.Consecutivo))
                    {
                        col.Item().Element(c => RenderCodigoBarras(c, documento.Consecutivo!));
                    }
                });
            });
        })
        .WithMetadata(new DocumentMetadata { Title = tituloRecibo })
        .GeneratePdf();

        return pdf;
    }

    private async Task<byte[]?> ObtenerLogoBytesAsync(string? logoUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(logoUrl)) return null;
        if (_logoCache.TryGetValue(logoUrl, out byte[]? cached)) return cached;

        // Lite guarda el logo como data URI en SQLite (ver LocalImagenStorageService);
        // se decodifica el base64 directo, sin red.
        if (logoUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var coma = logoUrl.IndexOf(',');
            if (coma < 0) return null;
            try
            {
                var bytes = Convert.FromBase64String(logoUrl[(coma + 1)..]);
                _logoCache.Set(logoUrl, bytes, new MemoryCacheEntryOptions
                {
                    Size = bytes.Length,
                    SlidingExpiration = LogoCacheSlidingExpiration
                });
                return bytes;
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Logo del negocio en data URI inválido.");
                return null;
            }
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var bytes = await client.GetByteArrayAsync(logoUrl, cancellationToken);
            _logoCache.Set(logoUrl, bytes, new MemoryCacheEntryOptions
            {
                Size = bytes.Length,
                SlidingExpiration = LogoCacheSlidingExpiration
            });
            return bytes;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo descargar el logo del negocio desde {LogoUrl}", logoUrl);
            return null;
        }
    }

    private static void ConfigurarPagina(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(20, Unit.Point);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(t => t.FontSize(9).FontColor(TextPrimary).FontFamily("Helvetica"));
    }

    private static void RenderHeader(IContainer container, DocumentoVenta documento, Negocio negocio, byte[]? logoBytes)
    {
        container.Border(1).BorderColor(BorderColor).Row(row =>
        {
            row.RelativeItem().Padding(10).Row(inner =>
            {
                if (logoBytes is not null)
                {
                    inner.ConstantItem(52).Height(52).AlignCenter().AlignMiddle().Image(logoBytes).FitArea();
                    inner.ConstantItem(10);
                }
                inner.RelativeItem().Column(col =>
                {
                    col.Item().Text(NombreNegocio(negocio)).Bold().FontSize(13);
                    col.Item().PaddingTop(2).Text(t =>
                    {
                        t.DefaultTextStyle(s => s.FontSize(7.5f).FontColor(TextMuted));
                        t.Span(negocio.Nombre);
                        t.Span("   |   ");
                        t.Span($"ID: {Dash(negocio.Identificacion)}");
                    });
                    var contacto = ContactoNegocio(negocio);
                    if (!string.IsNullOrEmpty(contacto))
                    {
                        col.Item().PaddingTop(2).Text(contacto).FontSize(7.5f).FontColor(TextMuted);
                    }
                    if (!string.IsNullOrWhiteSpace(negocio.Direccion))
                    {
                        col.Item().PaddingTop(2).Text(negocio.Direccion!).FontSize(7.5f).FontColor(TextMuted);
                    }
                });
            });

            row.ConstantItem(170).Background(AccentColor).AlignMiddle().Padding(12).Column(col =>
            {
                col.Spacing(3);
                col.Item().AlignCenter().Text(TipoDocumentoLabel(documento.TipoDocumento).ToUpperInvariant())
                    .Bold().FontSize(12).FontColor(Colors.White);
                col.Item().AlignCenter().Text(Dash(documento.Consecutivo))
                    .FontSize(9.5f).FontColor(Colors.White);
                if (documento.TipoDocumento != TipoDocumentoVenta.Proforma
                    && documento.Estado != EstadoDocumentoVenta.Emitido)
                {
                    col.Item().AlignCenter().Text(EstadoDocumentoLabel(documento.Estado))
                        .FontSize(7.5f).FontColor(AccentSoft);
                }
            });
        });
    }

    private static void RenderInfoEncabezado(IContainer container, DocumentoVenta documento)
    {
        var ci = CultureInfo.InvariantCulture;
        container.Column(col =>
        {
            col.Spacing(8);
            col.Item().Row(row =>
            {
                row.Spacing(8);
                row.RelativeItem().Element(InfoBox("Fecha", FormatoFechaCR(documento.FechaDocumento)));
                row.RelativeItem().Element(InfoBox("Condición de venta", documento.CondicionVentaDetalleSnapshot));
                row.RelativeItem().Element(InfoBox("Moneda", $"{documento.MonedaCodigo}   TC: {documento.TipoCambio.ToString("N2", ci)}"));
            });

            var caja = documento.Caja is null
                ? null
                : string.IsNullOrWhiteSpace(documento.Caja.Nombre)
                    ? documento.Caja.Codigo
                    : $"{documento.Caja.Codigo} - {documento.Caja.Nombre}";
            var vendedor = documento.Vendedor?.Nombre;

            if (!string.IsNullOrWhiteSpace(caja) || !string.IsNullOrWhiteSpace(vendedor))
            {
                col.Item().Row(row =>
                {
                    row.Spacing(8);
                    if (!string.IsNullOrWhiteSpace(caja))
                    {
                        row.RelativeItem().Element(InfoBox("Caja", caja!));
                    }
                    if (!string.IsNullOrWhiteSpace(vendedor))
                    {
                        row.RelativeItem().Element(InfoBox("Vendedor", vendedor!));
                    }
                });
            }
        });
    }

    private static void RenderClienteInfo(IContainer container, Cliente cliente)
    {
        container.Row(row =>
        {
            row.Spacing(8);
            row.RelativeItem(2).Element(InfoBox("Cliente", cliente.Nombre));
            row.RelativeItem().Element(InfoBox("Identificación", Dash(cliente.Identificacion)));
            row.RelativeItem().Element(InfoBox("Contacto", ContactoCliente(cliente)));
        });
    }

    private static void RenderLineas(IContainer container, DocumentoVenta documento)
    {
        container.Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(52);
                    c.ConstantColumn(80);
                    c.RelativeColumn();
                    c.ConstantColumn(90);
                    c.ConstantColumn(90);
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).AlignRight().Text("Cant.");
                    h.Cell().Element(HeaderCell).Text("Código");
                    h.Cell().Element(HeaderCell).Text("Descripción");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Precio unit.");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Total");
                });

                var ci = CultureInfo.InvariantCulture;
                var zebra = false;
                foreach (var linea in documento.Lineas)
                {
                    var bg = zebra ? ZebraRow : "#ffffff";
                    table.Cell().Background(bg).Element(BodyCell).AlignRight()
                        .Text(linea.Cantidad.ToString("N2", ci)).FontSize(8);
                    table.Cell().Background(bg).Element(BodyCell)
                        .Text(linea.Codigo).FontSize(8);
                    table.Cell().Background(bg).Element(BodyCell)
                        .Text(linea.Descripcion).FontSize(8);
                    table.Cell().Background(bg).Element(BodyCell).AlignRight()
                        .Text(Money(linea.PrecioUnitario, documento.MonedaCodigo)).FontSize(8);
                    table.Cell().Background(bg).Element(BodyCell).AlignRight()
                        .Text(Money(linea.TotalLinea, documento.MonedaCodigo)).FontSize(8).Bold();

                    if (linea.MontoDescuento > 0 || linea.MontoImpuesto > 0)
                    {
                        table.Cell().ColumnSpan(2).Background(bg);
                        table.Cell().ColumnSpan(3).Background(bg).PaddingHorizontal(8).PaddingTop(3).PaddingBottom(5)
                            .Text($"Desc: {Money(linea.MontoDescuento, documento.MonedaCodigo)}   |   Imp ({linea.PorcentajeImpuesto:0.##}%): {Money(linea.MontoImpuesto, documento.MonedaCodigo)}")
                            .FontSize(7).FontColor("#94a3b8");
                    }
                    zebra = !zebra;
                }
            });
        });
    }

    private static void RenderTotales(IContainer container, DocumentoVenta documento)
    {
        container.PaddingTop(6).AlignRight().Width(260).Column(col =>
        {
            col.Spacing(5);
            col.Item().PaddingVertical(2).Row(r =>
            {
                r.RelativeItem().Text("Subtotal").FontColor(TextMuted);
                r.RelativeItem().AlignRight().Text(Money(documento.TotalVenta, documento.MonedaCodigo));
            });
            col.Item().PaddingVertical(2).Row(r =>
            {
                r.RelativeItem().Text("Descuentos").FontColor(TextMuted);
                r.RelativeItem().AlignRight().Text(Money(documento.TotalDescuentos, documento.MonedaCodigo));
            });
            col.Item().PaddingVertical(2).Row(r =>
            {
                r.RelativeItem().Text("Impuesto").FontColor(TextMuted);
                r.RelativeItem().AlignRight().Text(Money(documento.TotalImpuesto, documento.MonedaCodigo));
            });
            col.Item().PaddingTop(8).BorderTop(2).BorderColor(AccentColor).PaddingTop(6).Row(r =>
            {
                r.RelativeItem().Text("Total").Bold().FontSize(11).FontColor(AccentColor);
                r.RelativeItem().AlignRight().Text(Money(documento.TotalComprobante, documento.MonedaCodigo)).Bold().FontSize(11).FontColor(AccentColor);
            });

            // Equivalente en la otra moneda al TC del documento: USD→CRC (×TC) o
            // CRC→USD (÷TC). Solo informativo, debajo del Total.
            if (documento.TipoCambio > 0m)
            {
                var esUsd = string.Equals(documento.MonedaCodigo, "USD", StringComparison.OrdinalIgnoreCase);
                var monedaEquivalente = esUsd ? "CRC" : "USD";
                var totalEquivalente = esUsd
                    ? documento.TotalComprobante * documento.TipoCambio
                    : documento.TotalComprobante / documento.TipoCambio;

                col.Item().PaddingTop(2).Row(r =>
                {
                    r.RelativeItem().Text($"Equivalente ({monedaEquivalente})").FontColor(TextMuted);
                    r.RelativeItem().AlignRight().Text(Money(totalEquivalente, monedaEquivalente)).FontColor(TextMuted);
                });
            }

            if (documento.TipoDocumento != TipoDocumentoVenta.Proforma)
            {
                col.Item().PaddingTop(4).PaddingVertical(2).Row(r =>
                {
                    r.RelativeItem().Text("Pagado").FontColor(TextMuted);
                    r.RelativeItem().AlignRight().Text(Money(documento.TotalPagado, documento.MonedaCodigo));
                });

                if (documento.SaldoPendiente > 0)
                {
                    col.Item().PaddingVertical(2).Row(r =>
                    {
                        r.RelativeItem().Text("Saldo pendiente").Bold().FontColor(SaldoColor);
                        r.RelativeItem().AlignRight().Text(Money(documento.SaldoPendiente, documento.MonedaCodigo)).Bold().FontColor(SaldoColor);
                    });
                }
            }
        });
    }

    private static void RenderPagos(IContainer container, DocumentoVenta documento)
    {
        var pagosActivos = documento.Pagos.Where(p => !p.Anulado).ToList();
        container.Border(1).BorderColor(BorderColor).Background(BackgroundSoft).Padding(10).Column(col =>
        {
            col.Item().Text("Pagos").Bold().FontSize(9).FontColor(AccentColor);
            col.Item().PaddingTop(4).Column(inner =>
            {
                var total = pagosActivos.Count;
                for (var i = 0; i < total; i++)
                {
                    var pago = pagosActivos[i];
                    var esUltimo = i == total - 1;
                    // PaddingVertical 6 para separar texto de la linea, y sin
                    // BorderBottom en el ultimo pago (linea redundante con el
                    // border del card).
                    var item = inner.Item().PaddingVertical(6);
                    if (!esUltimo) item = item.BorderBottom(1).BorderColor("#e2e8f0");
                    item.Row(r =>
                    {
                        r.RelativeItem().Text(pago.MedioPagoDetalleSnapshot).FontSize(8);
                        r.RelativeItem().AlignRight().Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(8));
                            text.Span(Money(pago.MontoEntregado, pago.MonedaCodigo));
                            text.Span($"   Cambio: {Money(pago.MontoVueltoMonedaPago, pago.MonedaCodigo)}");
                        });
                    });
                }
            });
        });
    }

    private static void RenderReferenciaOrigen(IContainer container, DocumentoVenta documento)
    {
        var referencia = documento.Referencias.First();
        var consecutivoOrigen = documento.DocumentoOrigen?.Consecutivo ?? "—";
        var fechaOrigen = FormatoFechaCR(referencia.FechaDocumentoReferencia);
        var tipoOrigen = documento.DocumentoOrigen is null
            ? null
            : TipoDocumentoLabel(documento.DocumentoOrigen.TipoDocumento);
        var encabezado = tipoOrigen is null
            ? $"Documento referenciado: {consecutivoOrigen}"
            : $"{tipoOrigen} referenciada: {consecutivoOrigen}";

        container.Border(1).BorderColor(BorderColor).Background(BackgroundSoft).Padding(10).Column(col =>
        {
            col.Item().Text(encabezado).Bold().FontSize(9).FontColor(AccentColor);
            col.Item().PaddingTop(4).Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(8).FontColor(TextMuted));
                text.Span("Fecha origen: ").SemiBold();
                text.Span(fechaOrigen);
            });
            if (!string.IsNullOrWhiteSpace(referencia.Razon))
            {
                col.Item().PaddingTop(2).Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor(TextMuted));
                    text.Span("Razón: ").SemiBold();
                    text.Span(referencia.Razon);
                });
            }
        });
    }

    private static void RenderObservaciones(IContainer container, DocumentoVenta documento)
    {
        container.Border(1).BorderColor(BorderColor).Background(BackgroundSoft).Padding(10).Column(col =>
        {
            col.Item().Text("Observaciones").Bold().FontSize(9).FontColor(AccentColor);
            col.Item().PaddingTop(4).Text(documento.Observaciones!).FontSize(8).FontColor(TextMuted);
        });
    }

    private static void RenderLineasPie(IContainer container, IReadOnlyList<LineaPieDocumento> lineas)
    {
        container.PaddingTop(6).Column(col =>
        {
            col.Spacing(2);
            foreach (var linea in lineas)
            {
                var item = col.Item();
                item = linea.Alineacion switch
                {
                    AlineacionLineaPie.Centro => item.AlignCenter(),
                    AlineacionLineaPie.Derecha => item.AlignRight(),
                    _ => item.AlignLeft()
                };

                var texto = item.Text(linea.Texto).FontSize(8).FontColor(TextPrimary);
                if (linea.Negrita)
                {
                    texto.Bold();
                }
            }
        });
    }

    private static void RenderProformaNota(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Background(ZebraRow).Padding(10)
            .AlignCenter().Text("Este documento es una proforma y no representa una factura emitida.")
            .FontSize(8).FontColor(TextMuted);
    }

    // Code128 del consecutivo al pie del documento, para escanear y filtrar la
    // factura sin teclear. Texto legible debajo.
    private static void RenderCodigoBarras(IContainer container, string consecutivo)
    {
        var svg = Code128Barcode.GenerarSvg(consecutivo);
        if (svg is null)
        {
            return;
        }

        container.PaddingTop(10).AlignCenter().Width(240).Height(42).Svg(svg);
    }

    private static void RenderResolucionLegal(IContainer container)
    {
        container.PaddingTop(8).AlignCenter().Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(7).FontColor(TextMuted));
            text.Span("MH-DGT-RES-0027-2024").Bold();
            text.Span(" - Resolución General sobre las disposiciones técnicas de los comprobantes electrónicos");
        });
    }

    private static void RenderHeaderRecibo(IContainer container, DocumentoVenta documento, Negocio negocio, byte[]? logoBytes, bool esAnulado, int numeroAbono)
    {
        var tituloBloque = esAnulado ? "ANULACIÓN DE ABONO" : "RECIBO DE ABONO";
        var subtituloBloque = numeroAbono > 0
            ? $"{Dash(documento.Consecutivo)}   |   Abono #{numeroAbono}"
            : Dash(documento.Consecutivo);

        container.Border(1).BorderColor(BorderColor).Row(row =>
        {
            row.RelativeItem().Padding(12).Row(inner =>
            {
                if (logoBytes is not null)
                {
                    inner.ConstantItem(60).Height(60).AlignCenter().AlignMiddle().Image(logoBytes).FitArea();
                    inner.ConstantItem(12);
                }
                inner.RelativeItem().Column(col =>
                {
                    col.Item().Text(NombreNegocio(negocio)).Bold().FontSize(15);
                    col.Item().PaddingTop(2).Text(t =>
                    {
                        t.DefaultTextStyle(s => s.FontSize(8).FontColor(TextMuted));
                        t.Span(negocio.Nombre);
                        t.Span("   |   ");
                        t.Span($"ID: {Dash(negocio.Identificacion)}");
                    });
                    var contacto = ContactoNegocio(negocio);
                    if (!string.IsNullOrEmpty(contacto))
                    {
                        col.Item().PaddingTop(2).Text(contacto).FontSize(8).FontColor(TextMuted);
                    }
                });
            });

            row.ConstantItem(175).Background(AccentColor).Padding(14).Column(col =>
            {
                col.Spacing(3);
                col.Item().AlignCenter().Text(tituloBloque).Bold().FontSize(13).FontColor(Colors.White);
                col.Item().AlignCenter().Text(subtituloBloque).FontSize(10).FontColor(Colors.White);
                if (documento.Estado != EstadoDocumentoVenta.Emitido)
                {
                    col.Item().AlignCenter().Text(EstadoDocumentoLabel(documento.Estado)).FontSize(7.5f).FontColor(AccentSoft);
                }
            });
        });
    }

    private static void RenderInfoRecibo(IContainer container, DocumentoVenta documento, DocumentoVentaPago pago)
    {
        var numeroAbono = pago.NumeroAbono > 0 ? $"Abono #{pago.NumeroAbono}" : "Pago";
        container.Column(col =>
        {
            col.Spacing(8);
            col.Item().Row(row =>
            {
                row.Spacing(8);
                row.RelativeItem(2).Element(InfoBox("Cliente", documento.Cliente?.Nombre ?? "Cliente contado"));
                row.RelativeItem().Element(InfoBox("Número", numeroAbono));
                row.RelativeItem().Element(InfoBox("Medio de pago", pago.MedioPagoDetalleSnapshot));
                row.RelativeItem().Element(InfoBox("Recibido por", pago.UsuarioRegistro?.Nombre ?? "-"));
            });
            col.Item().Row(row =>
            {
                row.Spacing(8);
                row.RelativeItem().Element(InfoBox("Fecha informativa", FormatoFechaCR(pago.FechaPago)));
                row.RelativeItem().Element(InfoBox("Registro real", FormatoFechaCR(pago.FechaRegistroUtc)));
                if (!string.IsNullOrWhiteSpace(pago.Referencia))
                {
                    row.RelativeItem().Element(InfoBox("Referencia", pago.Referencia!));
                }
            });
        });
    }

    private static void RenderBloqueAnulacion(IContainer container, DocumentoVentaPago pago)
    {
        container.Border(1.5f).BorderColor("#dc2626").Background("#fff5f5").Padding(12).Column(col =>
        {
            col.Spacing(4);
            col.Item().Text("ABONO ANULADO").Bold().FontSize(9).FontColor("#dc2626").LetterSpacing(1f);
            col.Item().Row(row =>
            {
                row.Spacing(8);
                row.RelativeItem().Element(InfoBox("Fecha de anulación",
                    pago.FechaAnulacionUtc.HasValue ? FormatoFechaCR(pago.FechaAnulacionUtc.Value) : "-"));
                row.RelativeItem().Element(InfoBox("Anulado por", pago.UsuarioAnula?.Nombre ?? "-"));
            });
            if (!string.IsNullOrWhiteSpace(pago.MotivoAnulacion))
            {
                col.Item().Element(InfoBox("Motivo de anulación", pago.MotivoAnulacion!));
            }
        });
    }

    private static void RenderMontoRecibido(IContainer container, DocumentoVenta documento, DocumentoVentaPago pago, bool esAnulado = false)
    {
        var etiqueta = esAnulado ? "MONTO REVERTIDO" : "MONTO RECIBIDO";
        var colorFondo = esAnulado ? "#dc2626" : AccentColor;
        container.Background(colorFondo).Padding(18).AlignCenter().Column(col =>
        {
            col.Spacing(4);
            col.Item().AlignCenter().Text(etiqueta).Bold().FontSize(8).FontColor(AccentSoft).LetterSpacing(1.5f);
            col.Item().AlignCenter().Text(Money(pago.MontoAplicadoDocumento, documento.MonedaCodigo)).Bold().FontSize(22).FontColor(Colors.White);
            col.Item().AlignCenter().Text(pago.MedioPagoDetalleSnapshot).FontSize(8).FontColor(AccentSoft);
        });
    }

    private static void RenderEstadoCuenta(IContainer container, DocumentoVenta documento, DocumentoVentaPago pago, decimal saldoAnterior, decimal saldoNuevo, bool esAnulado = false)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(55);
                c.RelativeColumn(45);
            });

            void Fila(string label, string valor, bool destacado = false, bool resaltado = false)
            {
                var labelColor = resaltado ? AccentColor : TextMuted;
                var valorColor = resaltado ? AccentColor : TextPrimary;
                var size = destacado ? 10f : 9f;
                table.Cell().BorderBottom(1).BorderColor("#e2e8f0").PaddingVertical(7).PaddingHorizontal(10)
                    .Text(label).FontSize(size).FontColor(labelColor).Bold();
                table.Cell().BorderBottom(1).BorderColor("#e2e8f0").PaddingVertical(7).PaddingHorizontal(10)
                    .AlignRight().Text(valor).FontSize(size).FontColor(valorColor).Bold();
            }

            Fila(esAnulado ? "Saldo antes de anular" : "Saldo anterior", Money(saldoAnterior, documento.MonedaCodigo));
            Fila(esAnulado ? "Monto revertido" : "Monto abonado", Money(pago.MontoAplicadoDocumento, documento.MonedaCodigo), resaltado: true);
            Fila("Saldo pendiente", Money(saldoNuevo, documento.MonedaCodigo), destacado: true);
            if (!string.IsNullOrWhiteSpace(pago.Observacion))
            {
                Fila("Observación", pago.Observacion!);
            }
        });
    }

    private static Action<IContainer> InfoBox(string label, string value) => container =>
    {
        container.Border(1).BorderColor(BorderColor).Background(BackgroundSoft).Padding(7).Column(col =>
        {
            col.Item().Text(label.ToUpperInvariant()).FontSize(7).Bold().FontColor(TextMuted).LetterSpacing(0.4f);
            col.Item().PaddingTop(2).Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(9);
        });
    };

    private static Action<IContainer> SectionTitle(string text) => container =>
    {
        container.PaddingTop(6).PaddingBottom(3).BorderBottom(1.5f).BorderColor(AccentColor)
            .Text(text).Bold().FontSize(9.5f).FontColor(AccentColor);
    };

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(AccentColor).PaddingVertical(6).PaddingHorizontal(8).DefaultTextStyle(t => t.Bold().FontSize(8).FontColor(Colors.White));

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(1).BorderColor("#e2e8f0").PaddingVertical(5).PaddingHorizontal(8);

    private static string Money(decimal value, string monedaCodigo)
        => monedaCodigo.Equals("USD", StringComparison.OrdinalIgnoreCase)
            ? $"$ {value.ToString("N2", CultureInfo.InvariantCulture)}"
            : $"₡ {value.ToString("N2", CultureInfo.InvariantCulture)}";

    private static string Dash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();

    private static string NombreNegocio(Negocio negocio)
        => !string.IsNullOrWhiteSpace(negocio.NombreComercial) ? negocio.NombreComercial! : negocio.Nombre;

    private static string ContactoNegocio(Negocio negocio)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(negocio.Telefono)) partes.Add($"Tel: {negocio.Telefono}");
        if (!string.IsNullOrWhiteSpace(negocio.Correo)) partes.Add(negocio.Correo!);
        return string.Join("   |   ", partes);
    }

    private static string ContactoCliente(Cliente cliente)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(cliente.Correo)) partes.Add(cliente.Correo!);
        if (!string.IsNullOrWhiteSpace(cliente.Telefono)) partes.Add(cliente.Telefono!);
        return partes.Count == 0 ? "-" : string.Join("   ", partes);
    }

    private static string TituloDocumento(DocumentoVenta documento)
    {
        var tipo = TipoDocumentoLabel(documento.TipoDocumento);
        var consecutivo = string.IsNullOrWhiteSpace(documento.Consecutivo)
            ? $"borrador {documento.Id.ToString("N")[..8]}"
            : documento.Consecutivo;
        return $"{tipo} {consecutivo}";
    }

    private static string TipoDocumentoLabel(TipoDocumentoVenta tipo) => tipo switch
    {
        TipoDocumentoVenta.Factura => "Factura",
        TipoDocumentoVenta.Apartado => "Apartado",
        TipoDocumentoVenta.NotaCredito => "Nota de crédito",
        TipoDocumentoVenta.NotaDebito => "Nota de débito",
        TipoDocumentoVenta.Proforma => "Proforma",
        _ => tipo.ToString()
    };

    private static string EstadoDocumentoLabel(EstadoDocumentoVenta estado) => estado switch
    {
        EstadoDocumentoVenta.Borrador => "Borrador",
        EstadoDocumentoVenta.Emitido => "Emitido",
        EstadoDocumentoVenta.Anulado => "Anulado",
        EstadoDocumentoVenta.Reservado => "Reservado",
        EstadoDocumentoVenta.Convertido => "Convertido",
        EstadoDocumentoVenta.Cancelado => "Cancelado",
        EstadoDocumentoVenta.Vencido => "Vencido",
        _ => estado.ToString()
    };
}
