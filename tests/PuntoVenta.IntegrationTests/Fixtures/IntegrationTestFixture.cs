using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.IntegrationTests.Fixtures;

/// <summary>
/// Fixture compartida entre tests de la misma colección. Crea la fábrica una vez
/// y expone helpers de autenticación. Cada test opera contra la misma BD sembrada.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    public PuntoVentaWebFactory Factory { get; } = new();
    public HttpClient Client { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Client = Factory.CreateClient();

        // Esperar a que la migración/seeding del startup termine.
        // El factory con Environment=Development ejecuta MigrateAsync() y DataSeeder al arrancar.
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    public async Task<string> ObtenerTokenAdminAsync()
    {
        var response = await Client.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = "admin",
            Password = "Admin1234!"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthFlowDto>();
        return body?.AccessToken ?? throw new InvalidOperationException("Login no devolvió AccessToken.");
    }

    public void SetBearerToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private sealed record AuthFlowDto(
        string AccessToken,
        string RefreshToken,
        bool RequiresPasswordChange);
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationTestFixture> { }
