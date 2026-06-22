using System.Globalization;
using ClosedXML.Excel;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Services;

public sealed class ClosedXmlReporteVentasService : IReporteVentasExcelService
{
    private static readonly TimeZoneInfo ZonaCR = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
    private const string FormatoNumero = "#,##0.00";
    private const string FormatoCantidad = "#,##0.#####";

    public byte[] Generar(ReporteVentasRangoResultadoDto resultado, DateTime fechaDesdeUtc, DateTime fechaHastaUtc)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Ventas");

        // Encabezado
        var titulo = hoja.Cell(1, 1);
        titulo.Value = "Reporte de ventas por rango";
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 14;

        hoja.Cell(2, 1).Value =
            $"Del {FechaCR(fechaDesdeUtc)} al {FechaCR(fechaHastaUtc)}";

        if (resultado.Colonizado)
        {
            hoja.Cell(3, 1).Value = "Montos colonizados (CRC)";
            hoja.Cell(3, 1).Style.Font.Italic = true;
        }

        var filaInicio = 5;

        if (resultado.Detallado)
        {
            EscribirDetallado(hoja, resultado, filaInicio);
        }
        else
        {
            EscribirResumido(hoja, resultado, filaInicio);
        }

        hoja.Columns().AdjustToContents();

        using var memoria = new MemoryStream();
        workbook.SaveAs(memoria);
        return memoria.ToArray();
    }

    private static void EscribirDetallado(IXLWorksheet hoja, ReporteVentasRangoResultadoDto resultado, int filaInicio)
    {
        string[] encabezados =
        [
            "Consecutivo", "Fecha factura", "Identificación", "Cliente", "Medio pago",
            "Condición venta", "Moneda", "Tipo cambio", "# Línea", "Código", "Detalle",
            "Cantidad", "Precio unitario", "Descuento", "Subtotal", "Tarifa %",
            "Impuesto", "Total línea"
        ];

        EscribirEncabezados(hoja, filaInicio, encabezados);

        var fila = filaInicio + 1;
        foreach (var f in resultado.Filas)
        {
            var c = 1;
            hoja.Cell(fila, c++).Value = f.Consecutivo;
            hoja.Cell(fila, c++).Value = FechaCR(f.FechaFactura);
            hoja.Cell(fila, c++).Value = f.ClienteIdentificacion;
            hoja.Cell(fila, c++).Value = f.ClienteNombre;
            hoja.Cell(fila, c++).Value = f.MedioPago;
            hoja.Cell(fila, c++).Value = f.CondicionVenta;
            hoja.Cell(fila, c++).Value = f.MonedaCodigo;
            Numero(hoja.Cell(fila, c++), f.TipoCambio);
            hoja.Cell(fila, c++).Value = f.NumeroLinea;
            hoja.Cell(fila, c++).Value = f.ProductoCodigo;
            hoja.Cell(fila, c++).Value = f.ProductoDetalle;
            Cantidad(hoja.Cell(fila, c++), f.Cantidad);
            Numero(hoja.Cell(fila, c++), f.PrecioUnitario);
            Numero(hoja.Cell(fila, c++), f.Descuento);
            Numero(hoja.Cell(fila, c++), f.Subtotal);
            Numero(hoja.Cell(fila, c++), f.TarifaPorcentaje);
            Numero(hoja.Cell(fila, c++), f.MontoImpuesto);
            Numero(hoja.Cell(fila, c++), f.TotalLinea);
            fila++;
        }

        // Fila de totales: Descuento(14), Subtotal(15), Impuesto(17), Total(18).
        var totalCol = hoja.Cell(fila, 1);
        totalCol.Value = "Totales";
        totalCol.Style.Font.Bold = true;
        Numero(hoja.Cell(fila, 14), resultado.TotalDescuento, negrita: true);
        Numero(hoja.Cell(fila, 15), resultado.TotalSubtotal, negrita: true);
        Numero(hoja.Cell(fila, 17), resultado.TotalImpuesto, negrita: true);
        Numero(hoja.Cell(fila, 18), resultado.TotalGeneral, negrita: true);
    }

    private static void EscribirResumido(IXLWorksheet hoja, ReporteVentasRangoResultadoDto resultado, int filaInicio)
    {
        string[] encabezados =
        [
            "Consecutivo", "Fecha factura", "Identificación", "Cliente", "Medio pago",
            "Condición venta", "Moneda", "Tipo cambio", "Descuento", "Subtotal",
            "Impuesto", "Total documento"
        ];

        EscribirEncabezados(hoja, filaInicio, encabezados);

        var fila = filaInicio + 1;
        foreach (var f in resultado.Resumen)
        {
            var c = 1;
            hoja.Cell(fila, c++).Value = f.Consecutivo;
            hoja.Cell(fila, c++).Value = FechaCR(f.FechaFactura);
            hoja.Cell(fila, c++).Value = f.ClienteIdentificacion;
            hoja.Cell(fila, c++).Value = f.ClienteNombre;
            hoja.Cell(fila, c++).Value = f.MedioPago;
            hoja.Cell(fila, c++).Value = f.CondicionVenta;
            hoja.Cell(fila, c++).Value = f.MonedaCodigo;
            Numero(hoja.Cell(fila, c++), f.TipoCambio);
            Numero(hoja.Cell(fila, c++), f.Descuento);
            Numero(hoja.Cell(fila, c++), f.Subtotal);
            Numero(hoja.Cell(fila, c++), f.MontoImpuesto);
            Numero(hoja.Cell(fila, c++), f.TotalDocumento);
            fila++;
        }

        // Fila de totales: Descuento(9), Subtotal(10), Impuesto(11), Total(12).
        var totalCol = hoja.Cell(fila, 1);
        totalCol.Value = "Totales";
        totalCol.Style.Font.Bold = true;
        Numero(hoja.Cell(fila, 9), resultado.TotalDescuento, negrita: true);
        Numero(hoja.Cell(fila, 10), resultado.TotalSubtotal, negrita: true);
        Numero(hoja.Cell(fila, 11), resultado.TotalImpuesto, negrita: true);
        Numero(hoja.Cell(fila, 12), resultado.TotalGeneral, negrita: true);
    }

    private static void EscribirEncabezados(IXLWorksheet hoja, int fila, string[] encabezados)
    {
        for (var i = 0; i < encabezados.Length; i++)
        {
            var celda = hoja.Cell(fila, i + 1);
            celda.Value = encabezados[i];
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.LightGray;
        }
    }

    private static void Numero(IXLCell celda, decimal valor, bool negrita = false)
    {
        celda.Value = valor;
        celda.Style.NumberFormat.Format = FormatoNumero;
        if (negrita)
        {
            celda.Style.Font.Bold = true;
        }
    }

    private static void Cantidad(IXLCell celda, decimal valor)
    {
        celda.Value = valor;
        celda.Style.NumberFormat.Format = FormatoCantidad;
    }

    private static string FechaCR(DateTime utc)
    {
        var kindUtc = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(kindUtc, ZonaCR);
        return local.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
    }
}
