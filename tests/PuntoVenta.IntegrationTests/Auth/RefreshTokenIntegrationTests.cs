using System.Net;
using System.Net.Http.Json;
using PuntoVenta.IntegrationTests.Fixtures;

namespace PuntoVenta.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class RefreshTokenIntegrationTests(IntegrationTestFixture fixture)
{
    // ──────────────────────────────────────────────
    // Rotación exitosa — devuelve nuevos tokens
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Refresh_DebeRotarToken_CuandoRefreshTokenValido()
    {
        // 1. Obtener tokens de login.
        var loginResponse = await fixture.Client.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = "admin",
            Password = "Admin1234!"
        }, TestContext.Current.CancellationToken);
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(loginBody);

        // 2. Refresh.
        var refreshResponse = await fixture.Client.PostAsJsonAsync("/auth/refresh", new
        {
            RefreshToken = loginBody.RefreshToken
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(refreshBody);
        Assert.False(string.IsNullOrWhiteSpace(refreshBody.AccessToken));
        Assert.NotEqual(loginBody.RefreshToken, refreshBody.RefreshToken);
    }

    // ──────────────────────────────────────────────
    // Reutilización de token rotado dentro de la ventana de gracia
    // devuelve el token de reemplazo (idempotencia de red)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Refresh_TokenRotadoReutilizadoEnVentanaGracia_Retorna200ConTokenReemplazo()
    {
        // 1. Login.
        var loginResponse = await fixture.Client.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = "admin",
            Password = "Admin1234!"
        }, TestContext.Current.CancellationToken);
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(loginBody);
        var refreshTokenOriginal = loginBody.RefreshToken;

        // 2. Primera rotación — el token original pasa a estado "rotado".
        var refreshResponse = await fixture.Client.PostAsJsonAsync("/auth/refresh", new
        {
            RefreshToken = refreshTokenOriginal
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(refreshBody);

        // 3. Reuso inmediato del token original dentro de la ventana de gracia:
        //    el handler sigue la cadena de reemplazo y rota de nuevo (tolerancia
        //    a reintentos en redes inestables). No debe devolver 401.
        var reintento = await fixture.Client.PostAsJsonAsync("/auth/refresh", new
        {
            RefreshToken = refreshTokenOriginal
        }, TestContext.Current.CancellationToken);

        // Dentro de la ventana de gracia la rotación se permite (200),
        // produciendo un nuevo par de tokens cada vez.
        Assert.Equal(HttpStatusCode.OK, reintento.StatusCode);
        var reintentoBody = await reintento.Content.ReadFromJsonAsync<AuthResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(reintentoBody);
        Assert.False(string.IsNullOrWhiteSpace(reintentoBody.AccessToken));
    }

    // ──────────────────────────────────────────────
    // Nuevo access token funciona en endpoint protegido
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Refresh_NuevoAccessToken_FuncionaEnEndpointProtegido()
    {
        // 1. Login.
        var loginResponse = await fixture.Client.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = "admin",
            Password = "Admin1234!"
        }, TestContext.Current.CancellationToken);
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(loginBody);

        // 2. Refresh.
        var refreshResponse = await fixture.Client.PostAsJsonAsync("/auth/refresh", new
        {
            RefreshToken = loginBody.RefreshToken
        }, TestContext.Current.CancellationToken);
        refreshResponse.EnsureSuccessStatusCode();
        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(refreshBody);

        // 3. Usar el nuevo access token en un endpoint protegido.
        var cliente = fixture.Factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", refreshBody.AccessToken);

        var response = await cliente.GetAsync("/clientes?pagina=1&tamano=10", TestContext.Current.CancellationToken);

        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NoContent,
            $"Status inesperado con nuevo access token: {response.StatusCode}");
    }

    // ──────────────────────────────────────────────
    // Refresh con token inválido → 401
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Refresh_Retorna401_CuandoTokenInvalido()
    {
        var response = await fixture.Client.PostAsJsonAsync("/auth/refresh", new
        {
            RefreshToken = "token-inexistente-abc123"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed record AuthResponse(
        string AccessToken,
        string RefreshToken,
        bool RequiresPasswordChange);
}
