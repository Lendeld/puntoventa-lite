using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PuntoVenta.IntegrationTests.Fixtures;

namespace PuntoVenta.IntegrationTests.Ventas;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class VentasCreditoIntegrationTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task FacturaCredito_Flujo_AbonarConsultarReciboYAnular()
    {
        var cliente = await CrearClienteAutenticadoAsync();
        var productoId = await CrearProductoAsync(cliente, $"CXC-{Guid.NewGuid():N}"[..15], 1000m);
        var clienteId = await CrearClienteCreditoAsync(cliente, $"Cliente CxC {Guid.NewGuid():N}"[..24]);
        var facturaId = await CrearFacturaCreditoAsync(cliente, clienteId, productoId, 1m, 1000m);

        var fechaPagoInformativa = DateTime.UtcNow.AddMinutes(-5);
        var abonoResp = await cliente.PostAsJsonAsync($"/ventas/facturas/{facturaId}/abonos", new
        {
            Pago = new
            {
                MonedaCodigo = "CRC",
                TipoCambioAplicado = 1m,
                MedioPagoCodigo = "01",
                MontoEntregado = 400m,
                MontoAplicadoMonedaPago = 400m,
                MontoAplicadoDocumento = 400m,
                MontoVueltoMonedaPago = 0m,
                MontoVueltoDocumento = 0m
            },
            FechaPago = fechaPagoInformativa
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, abonoResp.StatusCode);
        var pagoId = await abonoResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, pagoId);

        var detalle = await ObtenerJsonAsync(cliente, $"/ventas/{facturaId}");
        Assert.Equal(600m, detalle.GetProperty("saldoPendiente").GetDecimal());
        var pagoDetalle = detalle.GetProperty("pagos").EnumerateArray().Single();
        Assert.Equal(pagoId, pagoDetalle.GetProperty("id").GetGuid());
        Assert.Equal(1, pagoDetalle.GetProperty("numeroAbono").GetInt32());
        Assert.False(pagoDetalle.GetProperty("anulado").GetBoolean());
        Assert.True(pagoDetalle.TryGetProperty("fechaRegistroUtc", out var fechaRegistroProp));

        var ticketRecibo = await ObtenerJsonAsync(cliente, $"/ventas/{facturaId}/ticket-data?pagoId={pagoId}");
        var pagoTicket = ticketRecibo.GetProperty("pagos").EnumerateArray().Single();
        Assert.True(ticketRecibo.GetProperty("esRecibo").GetBoolean());
        Assert.False(ticketRecibo.GetProperty("esReciboAnulado").GetBoolean());
        Assert.Equal(1000m, ticketRecibo.GetProperty("saldoAnterior").GetDecimal());
        Assert.Equal(600m, ticketRecibo.GetProperty("saldoNuevo").GetDecimal());
        Assert.Equal(1, pagoTicket.GetProperty("numeroAbono").GetInt32());

        var pdfRecibo = await cliente.GetAsync($"/ventas/{facturaId}/abonos/{pagoId}/pdf", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, pdfRecibo.StatusCode);
        Assert.Equal("application/pdf", pdfRecibo.Content.Headers.ContentType?.MediaType);
        Assert.NotEmpty(await pdfRecibo.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));

        var anularResp = await cliente.PostAsJsonAsync(
            $"/ventas/{facturaId}/abonos/{pagoId}/anular",
            new { Motivo = "Error de caja" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, anularResp.StatusCode);
        var pagoAnuladoId = await anularResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.Equal(pagoId, pagoAnuladoId);

        var detalleAnulado = await ObtenerJsonAsync(cliente, $"/ventas/{facturaId}");
        Assert.Equal(1000m, detalleAnulado.GetProperty("saldoPendiente").GetDecimal());
        var pagoAnulado = detalleAnulado.GetProperty("pagos").EnumerateArray().Single();
        Assert.True(pagoAnulado.GetProperty("anulado").GetBoolean());
        Assert.Equal("Error de caja", pagoAnulado.GetProperty("motivoAnulacion").GetString());

        var ticketAnulado = await ObtenerJsonAsync(cliente, $"/ventas/{facturaId}/ticket-data?pagoId={pagoId}");
        Assert.True(ticketAnulado.GetProperty("esRecibo").GetBoolean());
        Assert.True(ticketAnulado.GetProperty("esReciboAnulado").GetBoolean());
        Assert.Equal(600m, ticketAnulado.GetProperty("saldoAnterior").GetDecimal());
        Assert.Equal(1000m, ticketAnulado.GetProperty("saldoNuevo").GetDecimal());
        Assert.Equal("Error de caja", ticketAnulado.GetProperty("motivoAnulacion").GetString());
    }

    [Fact]
    public async Task FacturaCredito_DebeBloquearNotasYReportarPorFechaReal()
    {
        var cliente = await CrearClienteAutenticadoAsync();
        var productoId = await CrearProductoAsync(cliente, $"RPT-{Guid.NewGuid():N}"[..15], 900m);
        var clienteId = await CrearClienteCreditoAsync(cliente, $"Cliente Rpt {Guid.NewGuid():N}"[..24]);
        var facturaId = await CrearFacturaCreditoAsync(cliente, clienteId, productoId, 1m, 900m);

        var fechaPagoInformativa = DateTime.UtcNow.AddMinutes(-10);
        var abonoResp = await cliente.PostAsJsonAsync($"/ventas/facturas/{facturaId}/abonos", new
        {
            Pago = new
            {
                MonedaCodigo = "CRC",
                TipoCambioAplicado = 1m,
                MedioPagoCodigo = "01",
                MontoEntregado = 300m,
                MontoAplicadoMonedaPago = 300m,
                MontoAplicadoDocumento = 300m,
                MontoVueltoMonedaPago = 0m,
                MontoVueltoDocumento = 0m
            },
            FechaPago = fechaPagoInformativa
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, abonoResp.StatusCode);
        var pagoId = await abonoResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        var ticketRecibo = await ObtenerJsonAsync(cliente, $"/ventas/{facturaId}/ticket-data?pagoId={pagoId}");
        var pagoTicket = ticketRecibo.GetProperty("pagos").EnumerateArray().Single();
        var fechaRegistroUtc = ParseUtc(pagoTicket.GetProperty("fechaRegistroUtc").GetString()!);

        var reporteInformativo = await ObtenerJsonAsync(
            cliente,
            $"/ventas/reportes/movimientos-dinero?fechaDesde={FormatearUtc(fechaPagoInformativa)}&fechaHasta={FormatearUtc(fechaPagoInformativa)}");
        Assert.Empty(reporteInformativo.GetProperty("movimientos").EnumerateArray());

        var reporteAbono = await ObtenerJsonAsync(
            cliente,
            $"/ventas/reportes/movimientos-dinero?fechaDesde={FormatearUtc(fechaRegistroUtc)}&fechaHasta={FormatearUtc(fechaRegistroUtc)}");
        var movimientoAbono = reporteAbono.GetProperty("movimientos").EnumerateArray().Single();
        Assert.Equal(pagoId, movimientoAbono.GetProperty("pagoId").GetGuid());
        Assert.Equal("AbonoFacturaCredito", movimientoAbono.GetProperty("tipoMovimiento").GetString());
        Assert.Equal(300m, movimientoAbono.GetProperty("monto").GetDecimal());

        var reportePdf = await cliente.GetAsync(
            $"/ventas/reportes/movimientos-dinero/pdf?fechaDesde={FormatearUtc(fechaRegistroUtc)}&fechaHasta={FormatearUtc(fechaRegistroUtc)}",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, reportePdf.StatusCode);
        Assert.Equal("application/pdf", reportePdf.Content.Headers.ContentType?.MediaType);
        Assert.NotEmpty(await reportePdf.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));

        var notaCreditoResp = await cliente.PostAsJsonAsync("/ventas/notas-credito", new
        {
            DocumentoOrigenId = facturaId,
            Modo = 2,
            Lineas = Array.Empty<object>()
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, notaCreditoResp.StatusCode);

        var notaDebitoResp = await cliente.PostAsJsonAsync("/ventas/notas-debito", new
        {
            DocumentoOrigenId = facturaId,
            Lineas = new[]
            {
                new
                {
                    ProductoId = productoId,
                    Cantidad = 1m,
                    PrecioUnitario = 50m,
                    MontoDescuento = 0m
                }
            }
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, notaDebitoResp.StatusCode);

        var anularResp = await cliente.PostAsJsonAsync(
            $"/ventas/{facturaId}/abonos/{pagoId}/anular",
            new { Motivo = "Corrección" },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, anularResp.StatusCode);

        var ticketAnulado = await ObtenerJsonAsync(cliente, $"/ventas/{facturaId}/ticket-data?pagoId={pagoId}");
        var fechaAnulacionUtc = ParseUtc(ticketAnulado.GetProperty("fechaAnulacionUtc").GetString()!);

        var reporteAnulacion = await ObtenerJsonAsync(
            cliente,
            $"/ventas/reportes/movimientos-dinero?fechaDesde={FormatearUtc(fechaAnulacionUtc)}&fechaHasta={FormatearUtc(fechaAnulacionUtc)}");
        var movimientoAnulacion = reporteAnulacion.GetProperty("movimientos").EnumerateArray().Single();
        Assert.Equal("AnulacionAbono", movimientoAnulacion.GetProperty("tipoMovimiento").GetString());
        Assert.Equal(-300m, movimientoAnulacion.GetProperty("monto").GetDecimal());
        Assert.Equal("Corrección", movimientoAnulacion.GetProperty("motivoAnulacion").GetString());
    }

    private async Task<HttpClient> CrearClienteAutenticadoAsync()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var http = fixture.Factory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    private static async Task<JsonElement> ObtenerJsonAsync(HttpClient cliente, string url)
    {
        var resp = await cliente.GetAsync(url, TestContext.Current.CancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new InvalidOperationException($"GET {url} devolvió {(int)resp.StatusCode}: {body}");
        }

        return await resp.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
    }

    private static async Task<Guid> CrearProductoAsync(HttpClient cliente, string codigo, decimal precio)
    {
        var resp = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = codigo,
            Nombre = $"Producto {codigo}",
            TipoItem = 1,
            PrecioUnitario = precio,
            ExistenciaInicial = 1000m
        }, TestContext.Current.CancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private static async Task<Guid> CrearClienteCreditoAsync(HttpClient cliente, string nombre)
    {
        var resp = await cliente.PostAsJsonAsync("/clientes", new
        {
            Nombre = nombre
        }, TestContext.Current.CancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private static async Task<Guid> CrearFacturaCreditoAsync(
        HttpClient cliente,
        Guid clienteId,
        Guid productoId,
        decimal cantidad,
        decimal precio)
    {
        var facturaResp = await cliente.PostAsJsonAsync("/ventas/facturas", new
        {
            ClienteId = clienteId,
            CondicionVentaCodigo = "02",
            PlazoCreditoDias = 30,
            Lineas = new[]
            {
                new
                {
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    PrecioUnitario = precio,
                    MontoDescuento = 0m
                }
            },
            Pagos = Array.Empty<object>()
        }, TestContext.Current.CancellationToken);
        facturaResp.EnsureSuccessStatusCode();
        return await facturaResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private static DateTime ParseUtc(string texto)
        => DateTime.Parse(texto, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    private static string FormatearUtc(DateTime fechaUtc)
        => Uri.EscapeDataString(fechaUtc.ToString("O", CultureInfo.InvariantCulture));
}
