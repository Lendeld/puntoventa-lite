using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class SeederIdempotenciaTests(PuntoVenta.IntegrationTests.Fixtures.IntegrationTestFixture fixture)
{
    // ──────────────────────────────────────────────
    // Segunda pasada del seeder no duplica permisos
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DataSeeder_NoDuplicaPermisos_CuandoSeEjecutaDosVeces()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cantesAntes = await db.Permisos.CountAsync(TestContext.Current.CancellationToken);
        Assert.True(cantesAntes > 0, "Se esperaban permisos sembrados en el startup.");

        await DataSeeder.SembrarPermisosAsync(db);

        var cantidadDespues = await db.Permisos.CountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(cantesAntes, cantidadDespues);
    }

    // ──────────────────────────────────────────────
    // Segunda pasada del seeder no duplica medios de pago
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DataSeeder_NoDuplicaMediosPago_CuandoSeEjecutaDosVeces()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cantidadAntes = await db.MediosPago.CountAsync(TestContext.Current.CancellationToken);
        Assert.True(cantidadAntes > 0, "Se esperaban medios de pago sembrados.");

        await DataSeeder.SembrarMediosPagoAsync(db);

        var cantidadDespues = await db.MediosPago.CountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(cantidadAntes, cantidadDespues);
    }

    // ──────────────────────────────────────────────
    // Segunda pasada del seeder no duplica condiciones de venta
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DataSeeder_NoDuplicaCondicionesVenta_CuandoSeEjecutaDosVeces()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cantidadAntes = await db.CondicionesVenta.CountAsync(TestContext.Current.CancellationToken);
        Assert.True(cantidadAntes > 0, "Se esperaban condiciones de venta sembradas.");

        await DataSeeder.SembrarCondicionesVentaAsync(db);

        var cantidadDespues = await db.CondicionesVenta.CountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(cantidadAntes, cantidadDespues);
    }

    // ──────────────────────────────────────────────
    // Segunda pasada del seeder no duplica perfiles de impresora
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DataSeeder_NoDuplicaPerfilesImpresora_CuandoSeEjecutaDosVeces()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cantidadAntes = await db.PerfilesImpresoraTicket.CountAsync(TestContext.Current.CancellationToken);

        await DataSeeder.SembrarPerfilesImpresoraTicketAsync(db);

        var cantidadDespues = await db.PerfilesImpresoraTicket.CountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(cantidadAntes, cantidadDespues);
    }

    // ──────────────────────────────────────────────
    // Segunda pasada del seeder no duplica roles
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DataSeeder_NoDuplicaRoles_CuandoSeEjecutaDosVeces()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cantidadAntes = await db.Roles.CountAsync(TestContext.Current.CancellationToken);
        Assert.True(cantidadAntes > 0, "Se esperaban roles sembrados.");

        await DataSeeder.SembrarRolesAsync(db);

        var cantidadDespues = await db.Roles.CountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(cantidadAntes, cantidadDespues);
    }

    // ──────────────────────────────────────────────
    // Medios de pago predefinidos existen con sus códigos correctos
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DataSeeder_SiembraMediosPago_CodigosCorrectos()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var codigos = await db.MediosPago.Select(m => m.Codigo).ToListAsync(TestContext.Current.CancellationToken);

        Assert.Contains("01", codigos); // Efectivo
        Assert.Contains("02", codigos); // Tarjeta
        Assert.Contains("04", codigos); // Transferencia
        Assert.Contains("06", codigos); // SINPE MÓVIL
    }

    // ──────────────────────────────────────────────
    // Condiciones de venta predefinidas existen
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DataSeeder_SiembraCondicionesVenta_ContadoYCredito()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var codigos = await db.CondicionesVenta.Select(c => c.Codigo).ToListAsync(TestContext.Current.CancellationToken);

        Assert.Contains("01", codigos); // Contado
        Assert.Contains("02", codigos); // Crédito
    }
}
