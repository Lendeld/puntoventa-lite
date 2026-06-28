using System.Net;
using System.Net.Http.Json;
using PuntoVenta.IntegrationTests.Fixtures;

namespace PuntoVenta.IntegrationTests.Productos;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class ProductoCrudIntegrationTests(IntegrationTestFixture fixture)
{
    // ──────────────────────────────────────────────
    // Crear producto → 201 + Guid
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CrearProducto_Retorna201_CuandoDatosValidos()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        var response = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = $"PROD-TEST-{Guid.NewGuid():N}"[..20],
            Nombre = "Producto de Prueba Integration",
            TipoItem = 1, // Bien
            PrecioUnitario = 1000m,
            TarifaIvaImpuestoCodigo = "08"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, id);
    }

    // ──────────────────────────────────────────────
    // Obtener producto por id → 200 con datos
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ObtenerProducto_Retorna200_CuandoExiste()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        // Crear.
        var codigo = $"GET-{Guid.NewGuid():N}"[..15];
        var crear = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = codigo,
            Nombre = "Producto Para Obtener",
            TipoItem = 1,
            PrecioUnitario = 2500m,
            TarifaIvaImpuestoCodigo = "08"
        }, TestContext.Current.CancellationToken);
        crear.EnsureSuccessStatusCode();
        var id = await crear.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // Obtener.
        var obtener = await cliente.GetAsync($"/productos/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, obtener.StatusCode);
        var body = await obtener.Content.ReadFromJsonAsync<ProductoResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(id, body.Id);
        Assert.Equal(codigo, body.Codigo);
        Assert.Equal("Producto Para Obtener", body.Nombre);
    }

    // ──────────────────────────────────────────────
    // Editar producto → 204
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ActualizarProducto_Retorna204_CuandoDatosValidos()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        // Crear.
        var codigoOriginal = $"EDT-{Guid.NewGuid():N}"[..15];
        var crear = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = codigoOriginal,
            Nombre = "Producto Original",
            TipoItem = 1,
            PrecioUnitario = 500m,
            TarifaIvaImpuestoCodigo = "08"
        }, TestContext.Current.CancellationToken);
        crear.EnsureSuccessStatusCode();
        var id = await crear.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // Editar.
        var editar = await cliente.PutAsJsonAsync($"/productos/{id}", new
        {
            Codigo = codigoOriginal,
            Nombre = "Producto Actualizado",
            TipoItem = 1,
            PrecioUnitario = 750m,
            TarifaIvaImpuestoCodigo = "08"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, editar.StatusCode);

        // Verificar que el cambio se persiste.
        var obtener = await cliente.GetAsync($"/productos/{id}", TestContext.Current.CancellationToken);
        var body = await obtener.Content.ReadFromJsonAsync<ProductoResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal("Producto Actualizado", body.Nombre);
        Assert.Equal(750m, body.PrecioUnitario);
    }

    // ──────────────────────────────────────────────
    // Código duplicado → conflicto (422 o 400)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CrearProducto_RetornaError_CuandoCodigoDuplicado()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        var codigoDuplicado = $"DUP-{Guid.NewGuid():N}"[..15];

        // Primer producto.
        var primer = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = codigoDuplicado,
            Nombre = "Primer Producto",
            TipoItem = 1,
            PrecioUnitario = 100m,
            TarifaIvaImpuestoCodigo = "08"
        }, TestContext.Current.CancellationToken);
        primer.EnsureSuccessStatusCode();

        // Segundo con mismo código.
        var segundo = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = codigoDuplicado,
            Nombre = "Segundo Producto",
            TipoItem = 1,
            PrecioUnitario = 200m,
            TarifaIvaImpuestoCodigo = "08"
        }, TestContext.Current.CancellationToken);

        // El endpoint debe rechazar el duplicado con 4xx.
        Assert.True(
            (int)segundo.StatusCode >= 400 && (int)segundo.StatusCode < 500,
            $"Status inesperado al crear producto con código duplicado: {segundo.StatusCode}");
    }

    // ──────────────────────────────────────────────
    // Tarifa IVA requerida → 400 con código específico
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CrearProducto_Retorna400_CuandoTarifaIvaAusente()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        var response = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = $"TV-{Guid.NewGuid():N}"[..15],
            Nombre = "Producto Sin Tarifa",
            TipoItem = 1,
            PrecioUnitario = 500m
            // TarifaIvaImpuestoCodigo omitido
        }, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Producto_TarifaIvaImpuestoCodigo", body);
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private HttpClient ConstruirCliente(string token)
    {
        var cliente = fixture.Factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    // TipoItem se serializa como número entero (Bien=1, Servicio=2).
    private sealed record ProductoResponse(
        Guid Id,
        string Codigo,
        string Nombre,
        decimal PrecioUnitario,
        bool NoAplicaExistencias = false);
}
