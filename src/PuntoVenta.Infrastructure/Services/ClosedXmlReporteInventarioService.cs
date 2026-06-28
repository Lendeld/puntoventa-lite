using System.Globalization;
using ClosedXML.Excel;
using PuntoVenta.Application.DTOs.Inventarios;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Services;

public sealed class ClosedXmlReporteInventarioService : IReporteInventarioExcelService
{
    private static readonly TimeZoneInfo ZonaCR = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
    private const string FormatoNumero = "#,##0.00";
    private const string FormatoCantidad = "#,##0.#####";

    public byte[] Generar(ReporteInventarioResultadoDto resultado)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Inventario");

        // Encabezado
        var titulo = hoja.Cell(1, 1);
        titulo.Value = "Reporte de inventario";
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 14;

        hoja.Cell(2, 1).Value = $"Generado: {FechaCR(DateTime.UtcNow)}";

        string[] encabezados =
        [
            "Código", "Nombre", "Descripción", "Categoría", "Proveedor", "Fecha de creación",
            "Existencia", "Precio costo", "Precio neto", "Tarifa %",
            "Monto impuesto", "Precio de venta", "Valor al costo", "Valor de venta"
        ];

        var filaInicio = 4;
        EscribirEncabezados(hoja, filaInicio, encabezados);

        var fila = filaInicio + 1;
        foreach (var f in resultado.Filas)
        {
            var c = 1;
            hoja.Cell(fila, c++).Value = f.Codigo;
            hoja.Cell(fila, c++).Value = f.Nombre;
            hoja.Cell(fila, c++).Value = f.Descripcion;
            hoja.Cell(fila, c++).Value = string.IsNullOrEmpty(f.Categoria) ? "-" : f.Categoria;
            hoja.Cell(fila, c++).Value = string.IsNullOrEmpty(f.Proveedor) ? "-" : f.Proveedor;
            hoja.Cell(fila, c++).Value = FechaCR(f.FechaCreacion);
            Cantidad(hoja.Cell(fila, c++), f.Existencia);
            Numero(hoja.Cell(fila, c++), f.PrecioCosto);
            Numero(hoja.Cell(fila, c++), f.PrecioNeto);
            Numero(hoja.Cell(fila, c++), f.TarifaPorcentaje);
            Numero(hoja.Cell(fila, c++), f.MontoImpuesto);
            Numero(hoja.Cell(fila, c++), f.PrecioVenta);
            Numero(hoja.Cell(fila, c++), f.ValorCosto);
            Numero(hoja.Cell(fila, c++), f.ValorVenta);
            fila++;
        }

        // Fila de totales: Existencia(7), Monto impuesto(11), Valor al costo(13), Valor de venta(14).
        var totalCol = hoja.Cell(fila, 1);
        totalCol.Value = "Totales";
        totalCol.Style.Font.Bold = true;
        Cantidad(hoja.Cell(fila, 7), resultado.TotalExistencia, negrita: true);
        Numero(hoja.Cell(fila, 11), resultado.TotalValorImpuesto, negrita: true);
        Numero(hoja.Cell(fila, 13), resultado.TotalValorCosto, negrita: true);
        Numero(hoja.Cell(fila, 14), resultado.TotalValorVenta, negrita: true);

        hoja.Columns().AdjustToContents();

        using var memoria = new MemoryStream();
        workbook.SaveAs(memoria);
        return memoria.ToArray();
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
        if (negrita) celda.Style.Font.Bold = true;
    }

    private static void Cantidad(IXLCell celda, decimal valor, bool negrita = false)
    {
        celda.Value = valor;
        celda.Style.NumberFormat.Format = FormatoCantidad;
        if (negrita) celda.Style.Font.Bold = true;
    }

    private static string FechaCR(DateTime utc)
    {
        var kindUtc = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(kindUtc, ZonaCR);
        return local.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
    }
}
