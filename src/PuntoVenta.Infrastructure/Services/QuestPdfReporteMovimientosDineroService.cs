using System.Globalization;
using System.Text;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PuntoVenta.Infrastructure.Services;

public sealed class QuestPdfReporteMovimientosDineroService : IReporteMovimientosDineroPdfService
{
    private static readonly TimeZoneInfo ZonaCR = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
    private static readonly CultureInfo CulturaCR = new("es-CR");

    private const string AccentColor = "#475569";
    private const string AccentSoft = "#e2e8f0";
    private const string TextPrimary = "#1e293b";
    private const string TextMuted = "#64748b";
    private const string BorderColor = "#e2e8f0";
    private const string BackgroundSoft = "#f8fafc";
    private const string PositiveColor = "#15803d";
    private const string NegativeColor = "#dc2626";

    public Task<byte[]> GenerarAsync(
        ReporteMovimientosDineroPdfData data,
        CancellationToken cancellationToken = default)
    {
        var titulo = TituloReporte(data);
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20, Unit.Point);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(t => t.FontSize(9).FontColor(TextPrimary).FontFamily("Helvetica"));

                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Element(c => RenderHeader(c, data));
                    col.Item().Element(c => RenderInfo(c, data));
                    col.Item().Element(SectionTitle("Resumen de flujo"));
                    col.Item().Element(c => RenderResumen(c, data.Reporte));
                    col.Item().Element(SectionTitle("Ingresos"));
                    col.Item().Element(c => RenderPorConcepto(c, data.Reporte, entradas: true));
                    if (data.Reporte.TotalSalidas > 0m)
                    {
                        col.Item().Element(SectionTitle("Egresos"));
                        col.Item().Element(c => RenderPorConcepto(c, data.Reporte, entradas: false));
                    }
                    col.Item().Element(SectionTitle("Medios de pago"));
                    col.Item().Element(c => RenderMedios(c, data.Reporte.TotalesPorMedio));
                    col.Item().Element(SectionTitle("Detalle trazable"));
                    col.Item().Element(c => RenderDetalle(c, data.Reporte.Movimientos));
                });
            });
        })
        .WithMetadata(new DocumentMetadata { Title = titulo })
        .GeneratePdf();

        return Task.FromResult(pdf);
    }

    private static string TituloReporte(ReporteMovimientosDineroPdfData data)
    {
        var caja = !string.IsNullOrWhiteSpace(data.CajaCodigo)
            ? $" caja {data.CajaCodigo}"
            : string.Empty;
        return $"Reporte de movimientos de dinero{caja} {FormatoFechaCorta(data.FechaDesdeUtc)} - {FormatoFechaCorta(data.FechaHastaUtc)}";
    }

    private static string FormatoFechaCorta(DateTime utc)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(ComoUtc(utc), ZonaCR);
        return local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static string FormatoFechaCR(DateTime utc)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(ComoUtc(utc), ZonaCR);
        return local.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
    }

    private static DateTime ComoUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string FormatoMoneda(decimal monto) => monto.ToString("C", CulturaCR);

    private static Action<IContainer> SectionTitle(string label) => c =>
        c.PaddingTop(4).BorderBottom(1).BorderColor(AccentSoft).PaddingBottom(2)
            .Text(label).Bold().FontSize(10).FontColor(AccentColor);

    private static void RenderHeader(IContainer container, ReporteMovimientosDineroPdfData data)
    {
        container.Border(1).BorderColor(BorderColor).Padding(12).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(data.Negocio.NombreComercial ?? data.Negocio.Nombre).Bold().FontSize(13);
                if (!string.IsNullOrWhiteSpace(data.Negocio.Identificacion))
                {
                    col.Item().PaddingTop(2).Text($"ID: {data.Negocio.Identificacion}").FontSize(7.5f).FontColor(TextMuted);
                }
                if (!string.IsNullOrWhiteSpace(data.Negocio.Direccion))
                {
                    col.Item().Text(data.Negocio.Direccion!).FontSize(7.5f).FontColor(TextMuted);
                }
            });
            row.ConstantItem(210).AlignRight().Column(col =>
            {
                col.Item().AlignRight().Text("REPORTE DE MOVIMIENTOS DE DINERO").Bold().FontSize(12).FontColor(AccentColor);
                if (!string.IsNullOrWhiteSpace(data.CajaCodigo))
                {
                    col.Item().AlignRight().Text($"Caja {data.CajaCodigo} · {data.CajaNombre}")
                        .FontSize(8).FontColor(TextMuted);
                }
                else
                {
                    col.Item().AlignRight().Text("Todas las cajas").FontSize(8).FontColor(TextMuted);
                }
            });
        });
    }

    private static void RenderInfo(IContainer container, ReporteMovimientosDineroPdfData data)
    {
        container.Background(BackgroundSoft).Padding(10).Column(col =>
        {
            col.Spacing(4);
            col.Item().Row(r =>
            {
                r.RelativeItem().Element(c => Info(c, "Desde", FormatoFechaCR(data.FechaDesdeUtc)));
                r.RelativeItem().Element(c => Info(c, "Hasta", FormatoFechaCR(data.FechaHastaUtc)));
            });
            col.Item().Text("Contexto: entradas y salidas generadas por ventas contado, abonos a crédito, abonos de apartado y anulaciones registradas en el rango.")
                .FontSize(8).FontColor(TextMuted);
        });
    }

    private static void RenderResumen(IContainer container, ReporteMovimientosDineroResultadoDto reporte)
    {
        container.Row(row =>
        {
            row.RelativeItem().Element(c => ResumenBox(c, "Entradas", reporte.TotalEntradas, PositiveColor));
            row.ConstantItem(8);
            row.RelativeItem().Element(c => ResumenBox(c, "Salidas", reporte.TotalSalidas, NegativeColor));
            row.ConstantItem(8);
            row.RelativeItem().Element(c => ResumenBox(c, "Neto", reporte.TotalNeto, reporte.TotalNeto >= 0m ? AccentColor : NegativeColor));
        });
    }

    private static void ResumenBox(IContainer container, string label, decimal monto, string color)
    {
        container.Border(1).BorderColor(BorderColor).Background(BackgroundSoft).Padding(10).Column(col =>
        {
            col.Item().Text(label).FontSize(7.5f).FontColor(TextMuted);
            col.Item().PaddingTop(2).Text(FormatoMoneda(monto)).Bold().FontSize(12).FontColor(color);
        });
    }

    private static void RenderPorConcepto(IContainer container, ReporteMovimientosDineroResultadoDto reporte, bool entradas)
    {
        var movimientos = reporte.Movimientos
            .Where(m => entradas ? m.Monto > 0m : m.Monto < 0m)
            .GroupBy(m => m.TipoMovimiento)
            .Select(g => new
            {
                Tipo = g.Key,
                Cantidad = g.Count(),
                Total = entradas ? g.Sum(m => m.Monto) : g.Sum(m => Math.Abs(m.Monto))
            })
            .OrderBy(x => EtiquetaMovimiento(x.Tipo))
            .ToList();

        if (movimientos.Count == 0)
        {
            container.Padding(6).Text(entradas ? "Sin ingresos en el rango." : "Sin egresos en el rango.")
                .Italic().FontColor(TextMuted).FontSize(8);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn();
                c.RelativeColumn(2);
            });

            table.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("Concepto");
                h.Cell().Element(HeaderCell).AlignRight().Text("Cant.");
                h.Cell().Element(HeaderCell).AlignRight().Text("Total");
            });

            foreach (var item in movimientos)
            {
                table.Cell().Element(BodyCell).Text(EtiquetaMovimiento(item.Tipo)).FontSize(8);
                table.Cell().Element(BodyCell).AlignRight().Text(item.Cantidad.ToString(CultureInfo.InvariantCulture)).FontSize(8);
                table.Cell().Element(BodyCell).AlignRight().Text(FormatoMoneda(item.Total)).FontSize(8)
                    .FontColor(entradas ? TextPrimary : NegativeColor);
            }
        });
    }

    private static void RenderMedios(IContainer container, IReadOnlyList<MovimientoDineroMedioDto> medios)
    {
        if (medios.Count == 0)
        {
            container.Padding(6).Text("Sin pagos registrados.").Italic().FontColor(TextMuted).FontSize(8);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(2);
                c.RelativeColumn();
                c.RelativeColumn();
                c.RelativeColumn();
            });

            table.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("Medio");
                h.Cell().Element(HeaderCell).AlignRight().Text("Entradas");
                h.Cell().Element(HeaderCell).AlignRight().Text("Salidas");
                h.Cell().Element(HeaderCell).AlignRight().Text("Neto");
            });

            foreach (var m in medios)
            {
                table.Cell().Element(BodyCell).Text(m.Detalle).FontSize(8);
                table.Cell().Element(BodyCell).AlignRight().Text(FormatoMoneda(m.Entradas)).FontSize(8);
                table.Cell().Element(BodyCell).AlignRight().Text(m.Salidas > 0m ? FormatoMoneda(m.Salidas) : "—")
                    .FontSize(8).FontColor(m.Salidas > 0m ? NegativeColor : TextMuted);
                table.Cell().Element(BodyCell).AlignRight().Text(FormatoMoneda(m.Neto)).FontSize(8).Bold()
                    .FontColor(m.Neto < 0m ? NegativeColor : TextPrimary);
            }

            var totalEntradas = medios.Sum(m => m.Entradas);
            var totalSalidas = medios.Sum(m => m.Salidas);
            var totalNeto = medios.Sum(m => m.Neto);
            table.Cell().Element(FooterCell).Text("Total").Bold().FontSize(8);
            table.Cell().Element(FooterCell).AlignRight().Text(FormatoMoneda(totalEntradas)).Bold().FontSize(8);
            table.Cell().Element(FooterCell).AlignRight().Text(FormatoMoneda(totalSalidas))
                .Bold().FontSize(8).FontColor(totalSalidas > 0m ? NegativeColor : TextPrimary);
            table.Cell().Element(FooterCell).AlignRight().Text(FormatoMoneda(totalNeto)).Bold().FontSize(8)
                .FontColor(totalNeto < 0m ? NegativeColor : TextPrimary);
        });
    }

    private static void RenderDetalle(IContainer container, IReadOnlyList<MovimientoDineroFilaDto> movimientos)
    {
        if (movimientos.Count == 0)
        {
            container.Border(1).BorderColor(BorderColor).Background(BackgroundSoft).Padding(8)
                .Text("No hay movimientos de dinero en el rango seleccionado.")
                .FontSize(8).FontColor(TextMuted);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(78);
                c.ConstantColumn(82);
                c.RelativeColumn(1.4f);
                c.RelativeColumn(1.2f);
                c.RelativeColumn(1.3f);
                c.ConstantColumn(72);
            });

            table.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("Fecha");
                h.Cell().Element(HeaderCell).Text("Movimiento");
                h.Cell().Element(HeaderCell).Text("Documento");
                h.Cell().Element(HeaderCell).Text("Cliente");
                h.Cell().Element(HeaderCell).Text("Medio / ref.");
                h.Cell().Element(HeaderCell).AlignRight().Text("Monto");
            });

            foreach (var m in movimientos)
            {
                table.Cell().Element(BodyCell).Text(FormatoFechaCR(m.FechaMovimientoUtc)).FontSize(7);
                table.Cell().Element(BodyCell).Column(col =>
                {
                    col.Item().Text(EtiquetaMovimiento(m.TipoMovimiento)).FontSize(7.2f)
                        .FontColor(m.Monto >= 0m ? PositiveColor : NegativeColor);
                    if (m.NumeroAbono > 0)
                    {
                        col.Item().Text($"Abono #{m.NumeroAbono}").FontSize(6.5f).FontColor(TextMuted);
                    }
                });
                table.Cell().Element(BodyCell).Column(col =>
                {
                    col.Item().Text(m.DocumentoConsecutivo ?? "Sin consecutivo").FontSize(7.2f).Bold();
                    col.Item().Text(m.TipoDocumento).FontSize(6.5f).FontColor(TextMuted);
                    if (!string.IsNullOrWhiteSpace(m.EventoTipoCodigo))
                    {
                        col.Item().Text($"Evento: {EtiquetaEvento(m.EventoTipoCodigo)}").FontSize(6.2f).FontColor(TextMuted);
                    }
                });
                table.Cell().Element(BodyCell).Text(m.ClienteNombre ?? "Cliente contado").FontSize(7);
                table.Cell().Element(BodyCell).Column(col =>
                {
                    col.Item().Text(m.MedioPagoDetalle).FontSize(7.2f);
                    col.Item().Text(string.IsNullOrWhiteSpace(m.Referencia) ? "Sin referencia" : m.Referencia).FontSize(6.5f).FontColor(TextMuted);
                    if (!string.IsNullOrWhiteSpace(m.MotivoAnulacion))
                    {
                        col.Item().Text($"Motivo: {m.MotivoAnulacion}").FontSize(6.2f).FontColor(TextMuted);
                    }
                });
                table.Cell().Element(BodyCell).AlignRight().Text(FormatoMoneda(m.Monto)).FontSize(7.2f).Bold()
                    .FontColor(m.Monto >= 0m ? TextPrimary : NegativeColor);
            }
        });
    }

    private static string EtiquetaMovimiento(string tipoMovimiento)
        => tipoMovimiento switch
        {
            "VentaContado" => "Venta contado",
            "AbonoFacturaCredito" => "Abono crédito",
            "AbonoApartado" => "Abono apartado",
            "AnulacionAbono" => "Anulación abono",
            _ => tipoMovimiento
        };

    private static string EtiquetaEvento(string tipoEvento)
        => tipoEvento switch
        {
            "FacturaEmitida" => "Factura Emitida",
            "FacturaEmitidaDesdeProforma" => "Factura Emitida desde Proforma",
            "ApartadoCreado" => "Apartado Creado",
            "ApartadoCancelado" => "Apartado Cancelado",
            "ApartadoConvertidoAFactura" => "Apartado Convertido a Factura",
            "AbonoRegistrado" => "Abono Registrado",
            "AbonoRevertido" => "Abono Revertido",
            "SaldoCancelado" => "Saldo Cancelado",
            "VencimientoExtendido" => "Vencimiento Extendido",
            "NotaCreditoEmitida" => "Nota de Crédito Emitida",
            "NotaCreditoAplicada" => "Nota de Crédito Aplicada",
            "NotaDebitoEmitida" => "Nota de Débito Emitida",
            "NotaDebitoAplicada" => "Nota de Débito Aplicada",
            _ => SepararCamelCase(tipoEvento)
        };

    private static string SepararCamelCase(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return "Evento";
        }

        var texto = valor.Trim();
        var resultado = new StringBuilder(texto.Length + 8);
        for (var i = 0; i < texto.Length; i++)
        {
            var actual = texto[i];
            if (i > 0 && char.IsUpper(actual))
            {
                var anterior = texto[i - 1];
                var siguienteEsMinuscula = i + 1 < texto.Length && char.IsLower(texto[i + 1]);
                if (char.IsLower(anterior) || siguienteEsMinuscula)
                {
                    resultado.Append(' ');
                }
            }
            resultado.Append(actual);
        }

        return resultado.ToString();
    }

    private static void Info(IContainer container, string label, string value)
        => container.Column(c =>
        {
            c.Item().Text(label).FontSize(7.5f).FontColor(TextMuted);
            c.Item().Text(value).FontSize(9);
        });

    private static IContainer HeaderCell(IContainer c)
        => c.Background(AccentSoft).PaddingHorizontal(5).PaddingVertical(4);

    private static IContainer BodyCell(IContainer c)
        => c.BorderBottom(0.5f).BorderColor(BorderColor).PaddingHorizontal(5).PaddingVertical(4);

    private static IContainer FooterCell(IContainer c)
        => c.Background(BackgroundSoft).PaddingHorizontal(5).PaddingVertical(4);
}
