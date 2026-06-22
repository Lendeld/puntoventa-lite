using System.Net;
using System.Net.Http.Json;
using Mediator;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Domain.Entities.Ventas.Eventos;

namespace PuntoVenta.IntegrationTests.Ventas;

/// <summary>
/// Verifica que al emitir una factura vía HTTP, el evento de dominio
/// FacturaEmitidaEvento se publica in-process. El spy se registra SOLO en
/// este test (no hay handler de producción).
/// </summary>
[Trait("Category", "Integration")]
public sealed class DomainEventsIntegrationTests
{
    [Fact]
    public async Task PostFactura_Emitida_FacturaEmitidaEventoSePublica()
    {
        // ── Spy ────────────────────────────────────────────────────────────────
        var spy = new FacturaEmitidaSpy();

        // ── Factory con spy inyectado ──────────────────────────────────────────
        var factory = new PuntoVentaWebFactory()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Registrar el spy como handler; Mediator lo resolverá via DI.
                    services.AddScoped<INotificationHandler<FacturaEmitidaEvento>>(_ => spy);
                });
            });

        await using (factory.ConfigureAwait(false))
        {
            // Inicializar migración (igual que el fixture).
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<PuntoVenta.Infrastructure.Persistence.ApplicationDbContext>();
                await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
            }

            var token = await ObtenerTokenAdminAsync(factory);
            var http = factory.CreateClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // ── Crear producto ─────────────────────────────────────────────────
            var productoResp = await http.PostAsJsonAsync("/productos", new
            {
                Codigo = $"EVT-{Guid.NewGuid():N}"[..15],
                Nombre = "Producto evento test",
                TipoItem = 1, // Bien
                PrecioUnitario = 1500m
            }, TestContext.Current.CancellationToken);
            productoResp.EnsureSuccessStatusCode();
            var productoId = await productoResp.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

            // ── Emitir factura ─────────────────────────────────────────────────
            var facturaResp = await http.PostAsJsonAsync("/ventas/facturas", new
            {
                CondicionVentaCodigo = "01",
                Lineas = new[]
                {
                    new
                    {
                        ProductoId = productoId,
                        Cantidad = 1m,
                        PrecioUnitario = 1500m,
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
                        MontoEntregado = 1500m,
                        MontoAplicadoMonedaPago = 1500m,
                        MontoAplicadoDocumento = 1500m,
                        MontoVueltoMonedaPago = 0m,
                        MontoVueltoDocumento = 0m
                    }
                }
            }, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, facturaResp.StatusCode);
            Assert.True(spy.ConteoPublicaciones >= 1, $"Se esperaba al menos 1 publicación de FacturaEmitidaEvento, pero se recibieron {spy.ConteoPublicaciones}.");
            Assert.NotNull(spy.UltimoEvento);
            Assert.Equal("CRC", spy.UltimoEvento.MonedaCodigo);
            Assert.Equal(1500m, spy.UltimoEvento.TotalComprobante);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static async Task<string> ObtenerTokenAdminAsync(WebApplicationFactory<Program> factory)
    {
        var http = factory.CreateClient();
        var resp = await http.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = "admin",
            Password = "Admin1234!"
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<AuthDto>();
        return body!.AccessToken;
    }

    private sealed record AuthDto(string AccessToken, string RefreshToken, bool RequiresPasswordChange);

    // ── Spy ────────────────────────────────────────────────────────────────────

    private sealed class FacturaEmitidaSpy : INotificationHandler<FacturaEmitidaEvento>
    {
        private int _conteo;
        private FacturaEmitidaEvento? _ultimo;

        public int ConteoPublicaciones => _conteo;
        public FacturaEmitidaEvento? UltimoEvento => _ultimo;

        public ValueTask Handle(FacturaEmitidaEvento notification, CancellationToken cancellationToken)
        {
            System.Threading.Interlocked.Increment(ref _conteo);
            _ultimo = notification;
            return ValueTask.CompletedTask;
        }
    }
}
