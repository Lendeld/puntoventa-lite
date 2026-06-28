using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.IntegrationTests.Dashboard;

[Trait("Category", "Integration")]
public sealed class DashboardIntegrationTests
{
    [Fact]
    public async Task Resumen_NoSumaAbonosAnuladosEnMetodosPago()
    {
        await using var factory = new PuntoVentaWebFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
        }

        var token = await ObtenerTokenAdminAsync(factory);
        using var cliente = factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var productoId = await CrearProductoAsync(cliente, $"DASH-{Guid.NewGuid():N}"[..15], 1000m);
        var clienteId = await CrearClienteCreditoAsync(cliente, $"Cliente dash {Guid.NewGuid():N}"[..24]);
        var facturaId = await CrearFacturaCreditoAsync(cliente, clienteId, productoId);
        var pagoId = await RegistrarAbonoAsync(cliente, facturaId);

        var resumenConAbono = await ObtenerJsonAsync(cliente, "/dashboard/resumen");
        var metodosConAbono = resumenConAbono.GetProperty("metodosPago").EnumerateArray().ToList();
        Assert.Contains(
            metodosConAbono,
            metodo => metodo.GetProperty("codigo").GetString() == "04"
                && metodo.GetProperty("total").GetDecimal() == 250m);

        var anularResp = await cliente.PostAsJsonAsync(
            $"/ventas/{facturaId}/abonos/{pagoId}/anular",
            new { Motivo = "Prueba dashboard" },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, anularResp.StatusCode);

        var resumen = await ObtenerJsonAsync(cliente, "/dashboard/resumen");
        var metodosPago = resumen.GetProperty("metodosPago").EnumerateArray().ToList();

        Assert.DoesNotContain(
            metodosPago,
            metodo => metodo.GetProperty("codigo").GetString() == "04"
                && metodo.GetProperty("total").GetDecimal() == 250m);
    }

    private static async Task<string> ObtenerTokenAdminAsync(PuntoVentaWebFactory factory)
    {
        using var http = factory.CreateClient();
        var resp = await http.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = "admin",
            Password = "Admin1234!"
        }, TestContext.Current.CancellationToken);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<AuthDto>(TestContext.Current.CancellationToken);
        return body?.AccessToken ?? throw new InvalidOperationException("Login no devolvio AccessToken.");
    }

    private static async Task<JsonElement> ObtenerJsonAsync(HttpClient cliente, string url)
    {
        var resp = await cliente.GetAsync(url, TestContext.Current.CancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new InvalidOperationException($"GET {url} devolvio {(int)resp.StatusCode}: {body}");
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
            TarifaIvaImpuestoCodigo = "08",
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

    private static async Task<Guid> CrearFacturaCreditoAsync(HttpClient cliente, Guid clienteId, Guid productoId)
    {
        var resp = await cliente.PostAsJsonAsync("/ventas/facturas", new
        {
            ClienteId = clienteId,
            CondicionVentaCodigo = "02",
            PlazoCreditoDias = 30,
            Lineas = new[]
            {
                new
                {
                    ProductoId = productoId,
                    Cantidad = 1m,
                    PrecioUnitario = 1000m,
                    MontoDescuento = 0m
                }
            },
            Pagos = Array.Empty<object>()
        }, TestContext.Current.CancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private static async Task<Guid> RegistrarAbonoAsync(HttpClient cliente, Guid facturaId)
    {
        var resp = await cliente.PostAsJsonAsync($"/ventas/facturas/{facturaId}/abonos", new
        {
            Pago = new
            {
                MonedaCodigo = "CRC",
                TipoCambioAplicado = 1m,
                MedioPagoCodigo = "04",
                MontoEntregado = 250m,
                MontoAplicadoMonedaPago = 250m,
                MontoAplicadoDocumento = 250m,
                MontoVueltoMonedaPago = 0m,
                MontoVueltoDocumento = 0m
            }
        }, TestContext.Current.CancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private sealed record AuthDto(string AccessToken, string RefreshToken, bool RequiresPasswordChange);
}
