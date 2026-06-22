using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.IntegrationTests;

/// <summary>
/// WebApplicationFactory que reemplaza la BD SQLite de producción por una
/// BD SQLite en disco con nombre único por instancia — evita conflictos entre
/// tests paralelos y garantiza un estado limpio en cada fixture.
/// </summary>
public sealed class PuntoVentaWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;

    public PuntoVentaWebFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"puntoventa-test-{Guid.NewGuid():N}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // UseSetting va a la configuración de host, disponible desde que Program.cs
        // construye el builder — AddInMemoryCollection en ConfigureAppConfiguration
        // llega tarde con minimal hosting (en CI no existe appsettings.Development.json
        // porque está gitignored, y Program.cs lee Jwt:SecretKey al arrancar).
        builder.UseSetting("Jwt:SecretKey", "dev-secret-key-minimo-32-caracteres-ok");
        builder.UseSetting("Jwt:Issuer", "puntoventa-dev");
        builder.UseSetting("Jwt:Audience", "puntoventa-dev");
        builder.UseSetting("Jwt:ExpiracionMinutos", "60");
        builder.UseSetting("Jwt:RefreshExpiracionDias", "7");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
        builder.UseSetting("Seed:Admin:Username", "admin");
        builder.UseSetting("Seed:Admin:Password", "Admin1234!");

        builder.ConfigureTestServices(services =>
        {
            // Reemplazar el DbContext registrado por uno con BD de test aislada.
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={_dbPath}")
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution));
        });

    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_dbPath))
        {
            try { File.Delete(_dbPath); } catch { /* ignorar — ya puede estar bloqueado por el runner */ }
        }
    }
}
