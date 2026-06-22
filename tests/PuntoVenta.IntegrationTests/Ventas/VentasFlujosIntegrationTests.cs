using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PuntoVenta.IntegrationTests.Fixtures;

namespace PuntoVenta.IntegrationTests.Ventas;

/// <summary>
/// Flujos completos de ventas via HTTP: factura, nota de crédito, apartado, proforma.
/// Cada test es independiente: crea su propio producto con código único.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class VentasFlujosIntegrationTests(IntegrationTestFixture fixture)
{
    // ══════════════════════════════════════════════
    // FLUJO FACTURA COMPLETO
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Factura_Flujo_CrearYObtener()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        // 1. Crear producto.
        var productoId = await CrearProductoAsync(cliente, $"FACT-{Guid.NewGuid():N}"[..15], 1000m);

        // 2. Crear y emitir la factura en una sola operación (Lite no tiene borrador).
        var facturaResp = await cliente.PostAsJsonAsync("/ventas/facturas", new
        {
            CondicionVentaCodigo = "01", // Contado
            Lineas = new[]
            {
                new
                {
                    ProductoId = productoId,
                    Cantidad = 2m,
                    PrecioUnitario = 1000m,
                    MontoDescuento = 0m
                }
            },
            Pagos = new[]
            {
                new
                {
                    MonedaCodigo = "CRC",
                    TipoCambioAplicado = 1m,
                    MedioPagoCodigo = "01", // Efectivo
                    MontoEntregado = 2000m,
                    MontoAplicadoMonedaPago = 2000m,
                    MontoAplicadoDocumento = 2000m,
                    MontoVueltoMonedaPago = 0m,
                    MontoVueltoDocumento = 0m
                }
            }
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, facturaResp.StatusCode);
        var facturaId = await facturaResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, facturaId);

        // 3. Obtener el documento y verificar estado emitido.
        var obtenerResp = await cliente.GetAsync($"/ventas/{facturaId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, obtenerResp.StatusCode);

        var body = await obtenerResp.Content.ReadFromJsonAsync<DocumentoVentaResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(facturaId, body.Id);
        Assert.Equal("Emitido", body.EstadoDetalle);
        Assert.False(string.IsNullOrWhiteSpace(body.Consecutivo), "Debe tener consecutivo asignado.");
        Assert.Equal(2000m, body.TotalComprobante);
    }

    // ══════════════════════════════════════════════
    // NOTA DE CRÉDITO
    // ══════════════════════════════════════════════

    [Fact]
    public async Task NotaCredito_Flujo_EmitirSobreFacturaYVerificarTotales()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        // 1. Crear producto.
        var productoId = await CrearProductoAsync(cliente, $"NC-{Guid.NewGuid():N}"[..15], 3000m);

        // 2. Crear y emitir factura.
        var facturaId = await CrearYEmitirFacturaAsync(cliente, productoId, 1, 3000m);

        // 3. Emitir nota de crédito por anulación.
        var ncResp = await cliente.PostAsJsonAsync("/ventas/notas-credito", new
        {
            DocumentoOrigenId = facturaId,
            Modo = 2, // Anulacion
            Razon = "Anulación de prueba de integración",
            Lineas = new[]
            {
                new
                {
                    ProductoId = productoId,
                    Cantidad = 1m,
                    PrecioUnitario = 3000m,
                    MontoDescuento = 0m
                }
            }
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, ncResp.StatusCode);
        var ncId = await ncResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, ncId);

        // 4. Obtener la nota de crédito y verificar totales.
        var ncObtener = await cliente.GetAsync($"/ventas/{ncId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, ncObtener.StatusCode);

        var nc = await ncObtener.Content.ReadFromJsonAsync<DocumentoVentaResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(nc);
        Assert.Equal("Emitido", nc.EstadoDetalle);
        Assert.Equal(3000m, nc.TotalComprobante);
        Assert.False(string.IsNullOrWhiteSpace(nc.Consecutivo));

        // 5. La factura original sigue emitida (la NC no cancela la factura origen,
        //    solo registra la reversión para efectos de Hacienda).
        var facturaObtener = await cliente.GetAsync($"/ventas/{facturaId}", TestContext.Current.CancellationToken);
        var factura = await facturaObtener.Content.ReadFromJsonAsync<DocumentoVentaResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(factura);
        Assert.Equal("Emitido", factura.EstadoDetalle);
    }

    [Fact]
    public async Task NotaCredito_SobreFacturaCredito_NoExigePlazo()
    {
        // Regresión: emitir una NC sobre una factura a CRÉDITO fallaba con
        // "plazo de crédito inválido" porque la nota heredaba la condición de
        // crédito del origen pero no lleva plazo. Debe emitirse sin pedir plazo.
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        var productoId = await CrearProductoAsync(cliente, $"NCC-{Guid.NewGuid():N}"[..15], 4000m);

        // Cliente requerido por la condición de crédito.
        var clienteResp = await cliente.PostAsJsonAsync("/clientes", new
        {
            Nombre = $"Cliente crédito {Guid.NewGuid():N}"[..30]
        }, TestContext.Current.CancellationToken);
        clienteResp.EnsureSuccessStatusCode();
        var clienteId = await clienteResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // Factura a crédito (sin pagos), con plazo.
        var facturaResp = await cliente.PostAsJsonAsync("/ventas/facturas", new
        {
            ClienteId = clienteId,
            CondicionVentaCodigo = "02", // Crédito
            PlazoCreditoDias = 30,
            Lineas = new[]
            {
                new { ProductoId = productoId, Cantidad = 1m, PrecioUnitario = 4000m, MontoDescuento = 0m }
            },
            Pagos = Array.Empty<object>()
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, facturaResp.StatusCode);
        var facturaId = await facturaResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // Emitir NC por anulación: antes del fix devolvía 400 (plazo de crédito inválido).
        var ncResp = await cliente.PostAsJsonAsync("/ventas/notas-credito", new
        {
            DocumentoOrigenId = facturaId,
            Modo = 2, // Anulacion
            Razon = "Anulación sobre factura a crédito",
            Lineas = new[]
            {
                new { ProductoId = productoId, Cantidad = 1m, PrecioUnitario = 4000m, MontoDescuento = 0m }
            }
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, ncResp.StatusCode);
        var ncId = await ncResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, ncId);
    }

    // ══════════════════════════════════════════════
    // APARTADO
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Apartado_Flujo_CrearAbonarYConvertirAFactura()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        // 1. Crear producto.
        var productoId = await CrearProductoAsync(cliente, $"APT-{Guid.NewGuid():N}"[..15], 5000m);

        // 2. Crear apartado con abono inicial del 50%.
        var crearResp = await cliente.PostAsJsonAsync("/ventas/apartados", new
        {
            CondicionVentaCodigo = "01", // Contado
            Lineas = new[]
            {
                new
                {
                    ProductoId = productoId,
                    Cantidad = 1m,
                    PrecioUnitario = 5000m,
                    MontoDescuento = 0m
                }
            },
            Pagos = new[]
            {
                new
                {
                    MonedaCodigo = "CRC",
                    TipoCambioAplicado = 1m,
                    MedioPagoCodigo = "01", // Efectivo
                    MontoEntregado = 2500m,
                    MontoAplicadoMonedaPago = 2500m,
                    MontoAplicadoDocumento = 2500m,
                    MontoVueltoMonedaPago = 0m,
                    MontoVueltoDocumento = 0m
                }
            }
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, crearResp.StatusCode);
        var apartadoId = await crearResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, apartadoId);

        // 3. Registrar abono adicional para cancelar el saldo.
        var inicioReporteUtc = DateTime.UtcNow.AddSeconds(-5);
        var abonarResp = await cliente.PostAsJsonAsync($"/ventas/apartados/{apartadoId}/abonos", new
        {
            Pago = new
            {
                MonedaCodigo = "CRC",
                TipoCambioAplicado = 1m,
                MedioPagoCodigo = "01",
                MontoEntregado = 2500m,
                MontoAplicadoMonedaPago = 2500m,
                MontoAplicadoDocumento = 2500m,
                MontoVueltoMonedaPago = 0m,
                MontoVueltoDocumento = 0m
            }
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, abonarResp.StatusCode);
        var finReporteUtc = DateTime.UtcNow.AddSeconds(5);

        var reporteResp = await cliente.GetAsync(
            $"/ventas/reportes/movimientos-dinero?fechaDesde={FormatearUtc(inicioReporteUtc)}&fechaHasta={FormatearUtc(finReporteUtc)}",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, reporteResp.StatusCode);
        var reporte = await reporteResp.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Contains(
            reporte.GetProperty("movimientos").EnumerateArray(),
            m => m.GetProperty("documentoId").GetGuid() == apartadoId
                && m.GetProperty("tipoMovimiento").GetString() == "AbonoApartado"
                && m.GetProperty("monto").GetDecimal() == 2500m);

        // 4. Convertir apartado a factura.
        var convertirResp = await cliente.PostAsync($"/ventas/apartados/{apartadoId}/convertir", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, convertirResp.StatusCode);
        var facturaId = await convertirResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, facturaId);

        // 5. Verificar que la factura resultante existe y está emitida.
        var facturaResp = await cliente.GetAsync($"/ventas/{facturaId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, facturaResp.StatusCode);

        var factura = await facturaResp.Content.ReadFromJsonAsync<DocumentoVentaResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(factura);
        Assert.Equal("Emitido", factura.EstadoDetalle);
        Assert.Equal(5000m, factura.TotalComprobante);
    }

    // ══════════════════════════════════════════════
    // PROFORMA
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Proforma_Flujo_CrearYObtener()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        // 1. Crear producto.
        var productoId = await CrearProductoAsync(cliente, $"PRF-{Guid.NewGuid():N}"[..15], 800m);

        // 2. Crear proforma.
        var crearResp = await cliente.PostAsJsonAsync("/ventas/proformas", new
        {
            CondicionVentaCodigo = "01", // Contado
            Lineas = new[]
            {
                new
                {
                    ProductoId = productoId,
                    Cantidad = 3m,
                    PrecioUnitario = 800m,
                    MontoDescuento = 0m
                }
            }
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, crearResp.StatusCode);
        var proformaId = await crearResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, proformaId);

        // 3. Obtener la proforma y verificar datos.
        var obtenerResp = await cliente.GetAsync($"/ventas/{proformaId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, obtenerResp.StatusCode);

        var body = await obtenerResp.Content.ReadFromJsonAsync<DocumentoVentaResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(proformaId, body.Id);
        Assert.Equal("Proforma", body.TipoDocumentoDetalle);
        Assert.Equal(2400m, body.TotalComprobante); // 3 × 800
        Assert.False(string.IsNullOrWhiteSpace(body.Consecutivo), "Proforma debe tener número asignado.");
    }

    // ══════════════════════════════════════════════
    // Helpers
    // ══════════════════════════════════════════════

    private HttpClient ConstruirCliente(string token)
    {
        var http = fixture.Factory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    private async Task<Guid> CrearProductoAsync(HttpClient cliente, string codigo, decimal precio)
    {
        var resp = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = codigo,
            Nombre = $"Producto {codigo}",
            TipoItem = 1, // Bien
            PrecioUnitario = precio
        });
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Guid>();
    }

    private async Task<Guid> CrearYEmitirFacturaAsync(
        HttpClient cliente,
        Guid productoId,
        decimal cantidad,
        decimal precio)
    {
        var facturaResp = await cliente.PostAsJsonAsync("/ventas/facturas", new
        {
            CondicionVentaCodigo = "01",
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
            Pagos = new[]
            {
                new
                {
                    MonedaCodigo = "CRC",
                    TipoCambioAplicado = 1m,
                    MedioPagoCodigo = "01",
                    MontoEntregado = cantidad * precio,
                    MontoAplicadoMonedaPago = cantidad * precio,
                    MontoAplicadoDocumento = cantidad * precio,
                    MontoVueltoMonedaPago = 0m,
                    MontoVueltoDocumento = 0m
                }
            }
        });
        facturaResp.EnsureSuccessStatusCode();
        return await facturaResp.Content.ReadFromJsonAsync<Guid>();
    }

    private static string FormatearUtc(DateTime fechaUtc)
        => Uri.EscapeDataString(fechaUtc.ToString("O", CultureInfo.InvariantCulture));

    // DTO mínimo para deserializar la respuesta del endpoint.
    private sealed record DocumentoVentaResponse(
        Guid Id,
        string TipoDocumentoDetalle,
        string EstadoDetalle,
        string? Consecutivo,
        decimal TotalComprobante,
        decimal TotalPagado,
        decimal MontoNotasCredito);
}
