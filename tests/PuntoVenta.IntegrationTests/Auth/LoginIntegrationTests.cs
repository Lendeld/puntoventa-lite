using System.Net;
using System.Net.Http.Json;
using PuntoVenta.IntegrationTests.Fixtures;

namespace PuntoVenta.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class LoginIntegrationTests(IntegrationTestFixture fixture)
{
    // ──────────────────────────────────────────────
    // Login exitoso — devuelve tokens
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Login_Exitoso_CuandoCredencialesAdmin()
    {
        var response = await fixture.Client.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = "admin",
            Password = "Admin1234!"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(body.RefreshToken));
    }

    // ──────────────────────────────────────────────
    // Credenciales incorrectas → 401
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Login_Retorna401_CuandoPasswordIncorrecta()
    {
        var response = await fixture.Client.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = "admin",
            Password = "password-mala"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ──────────────────────────────────────────────
    // Usuario inexistente → 401
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Login_Retorna401_CuandoUsuarioNoExiste()
    {
        var response = await fixture.Client.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = "noexiste",
            Password = "cualquiera"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ──────────────────────────────────────────────
    // Endpoint protegido sin token → 401
    // ──────────────────────────────────────────────

    [Fact]
    public async Task EndpointProtegido_Retorna401_SinToken()
    {
        // Usar un cliente fresco sin Authorization header.
        var clientSinAuth = fixture.Factory.CreateClient();
        var response = await clientSinAuth.GetAsync("/clientes", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ──────────────────────────────────────────────
    // Endpoint protegido con token → 200/204
    // ──────────────────────────────────────────────

    [Fact]
    public async Task EndpointProtegido_Retorna200_ConTokenValido()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = fixture.Factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await cliente.GetAsync("/clientes?pagina=1&tamano=10", TestContext.Current.CancellationToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Status inesperado: {response.StatusCode}");
    }

    // ──────────────────────────────────────────────
    // DTOs internos
    // ──────────────────────────────────────────────

    private sealed record AuthResponse(
        string AccessToken,
        string RefreshToken,
        bool RequiresPasswordChange);
}
