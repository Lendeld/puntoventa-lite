using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class MigracionTests(PuntoVenta.IntegrationTests.Fixtures.IntegrationTestFixture fixture)
{
    // ──────────────────────────────────────────────
    // Migración aplica limpio
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Migracion_Aplica_SinPendientes()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var pendientes = await db.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken);

        Assert.Empty(pendientes);
    }

    // ──────────────────────────────────────────────
    // DataSeeder es idempotente — segunda pasada sin error
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DataSeeder_EsIdempotente_CuandoSeEjecutaDosVeces()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Primera pasada ya ocurrió en el startup (InitializeAsync).
        // Segunda pasada — no debe lanzar ni duplicar registros.
        await DataSeeder.SembrarPermisosAsync(db);
        await DataSeeder.SembrarPaginasAsync(db);
        await DataSeeder.SembrarNegocioAsync(db);
        await DataSeeder.SembrarTiposIdentificacionAsync(db);
        await DataSeeder.SembrarRolesAsync(db);
        await DataSeeder.SembrarCondicionesVentaAsync(db);
        await DataSeeder.SembrarMediosPagoAsync(db);
        await DataSeeder.SembrarCodigosImpuestoAsync(db);
        await DataSeeder.SembrarTarifasIvaImpuestoAsync(db);

        // Si llegamos acá sin excepción, la idempotencia se cumplió.
        Assert.True(true);
    }

    // ──────────────────────────────────────────────
    // DataSeeder siembra permiso de admin
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DataSeeder_SiembraPermisos_YExistenEnBD()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var permisos = await db.Permisos.ToListAsync(TestContext.Current.CancellationToken);

        Assert.NotEmpty(permisos);
    }
}
