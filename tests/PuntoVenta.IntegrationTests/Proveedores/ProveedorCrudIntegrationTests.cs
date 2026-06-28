using System.Net;
using System.Net.Http.Json;
using PuntoVenta.IntegrationTests.Fixtures;

namespace PuntoVenta.IntegrationTests.Proveedores;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class ProveedorCrudIntegrationTests(IntegrationTestFixture fixture)
{
    // ──────────────────────────────────────────────
    // Crear proveedor → 201 + Guid
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CrearProveedor_Retorna201_CuandoDatosValidos()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        var response = await cliente.PostAsJsonAsync("/proveedores", new
        {
            Nombre = $"Proveedor Test {Guid.NewGuid():N}"[..30]
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, id);
    }

    // ──────────────────────────────────────────────
    // Obtener proveedor por id → 200 con datos
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ObtenerProveedor_Retorna200_CuandoExiste()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        var nombre = $"Prov-GET-{Guid.NewGuid():N}"[..20];
        var crear = await cliente.PostAsJsonAsync("/proveedores", new
        {
            Nombre = nombre,
            Correo = "contacto@ejemplo.cr",
            Telefono = "2222-0000"
        }, TestContext.Current.CancellationToken);
        crear.EnsureSuccessStatusCode();
        var id = await crear.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        var obtener = await cliente.GetAsync($"/proveedores/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, obtener.StatusCode);
        var body = await obtener.Content.ReadFromJsonAsync<ProveedorResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(id, body.Id);
        Assert.Equal(nombre, body.Nombre);
        Assert.Equal("contacto@ejemplo.cr", body.Correo);
        Assert.True(body.Activo);
    }

    // ──────────────────────────────────────────────
    // Editar y desactivar → 204 + verificar persistencia
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ActualizarProveedor_Retorna204_YDesactivaConActivoFalse()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        var nombre = $"Prov-EDT-{Guid.NewGuid():N}"[..20];
        var crear = await cliente.PostAsJsonAsync("/proveedores", new { Nombre = nombre }, TestContext.Current.CancellationToken);
        crear.EnsureSuccessStatusCode();
        var id = await crear.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // Editar con Activo:false = desactivar (soft delete)
        var editar = await cliente.PutAsJsonAsync($"/proveedores/{id}", new
        {
            Nombre = nombre + " Upd",
            Activo = false
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, editar.StatusCode);

        // Verificar que el cambio se persiste
        var obtener = await cliente.GetAsync($"/proveedores/{id}", TestContext.Current.CancellationToken);
        var body = await obtener.Content.ReadFromJsonAsync<ProveedorResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(nombre + " Upd", body.Nombre);
        Assert.False(body.Activo);
    }

    // ──────────────────────────────────────────────
    // Duplicado case-insensitive → 4xx
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CrearProveedor_RetornaError_CuandoNombreDuplicadoCaseInsensitive()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        var nombre = $"Prov-DUP-{Guid.NewGuid():N}"[..20];

        var primero = await cliente.PostAsJsonAsync("/proveedores", new { Nombre = nombre }, TestContext.Current.CancellationToken);
        primero.EnsureSuccessStatusCode();

        // Mismo nombre en minúsculas
        var segundo = await cliente.PostAsJsonAsync("/proveedores", new { Nombre = nombre.ToLowerInvariant() }, TestContext.Current.CancellationToken);

        Assert.True(
            (int)segundo.StatusCode >= 400 && (int)segundo.StatusCode < 500,
            $"Status inesperado al crear proveedor con nombre duplicado: {segundo.StatusCode}");
    }

    // ──────────────────────────────────────────────
    // Crear proveedor + producto con ese proveedorId
    // GET producto devuelve el proveedorId
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CrearProducto_ConProveedorId_YGetProducto_DevuelveProveedorId()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        // Crear proveedor
        var nombreProv = $"Prov-FK-{Guid.NewGuid():N}"[..20];
        var crearProv = await cliente.PostAsJsonAsync("/proveedores", new { Nombre = nombreProv }, TestContext.Current.CancellationToken);
        crearProv.EnsureSuccessStatusCode();
        var proveedorId = await crearProv.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, proveedorId);

        // Crear producto con ese proveedorId
        var codigoProd = $"PV-{Guid.NewGuid():N}"[..15];
        var crearProd = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = codigoProd,
            Nombre = "Producto con Proveedor",
            TipoItem = 1,
            PrecioUnitario = 1500m,
            TarifaIvaImpuestoCodigo = "08",
            ProveedorId = proveedorId
        }, TestContext.Current.CancellationToken);
        crearProd.EnsureSuccessStatusCode();
        var productoId = await crearProd.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // GET producto → verifica que devuelve proveedorId
        var obtener = await cliente.GetAsync($"/productos/{productoId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, obtener.StatusCode);
        var body = await obtener.Content.ReadFromJsonAsync<ProductoConProveedorResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(proveedorId, body.ProveedorId);
    }

    // ──────────────────────────────────────────────
    // Editar producto asignando proveedorId (regresión)
    // PUT /productos/{id} con proveedorId → GET lo devuelve
    // ──────────────────────────────────────────────

    [Fact]
    public async Task EditarProducto_AsignaProveedorId_YGetProducto_DevuelveProveedorId()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        // Crear proveedor
        var nombreProv = $"Prov-Ed-{Guid.NewGuid():N}"[..20];
        var crearProv = await cliente.PostAsJsonAsync("/proveedores", new { Nombre = nombreProv }, TestContext.Current.CancellationToken);
        crearProv.EnsureSuccessStatusCode();
        var proveedorId = await crearProv.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // Crear producto SIN proveedor
        var codigoProd = $"PE-{Guid.NewGuid():N}"[..15];
        var crearProd = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = codigoProd,
            Nombre = "Producto sin Proveedor",
            TipoItem = 1,
            PrecioUnitario = 2000m,
            TarifaIvaImpuestoCodigo = "08"
        }, TestContext.Current.CancellationToken);
        crearProd.EnsureSuccessStatusCode();
        var productoId = await crearProd.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // Editar producto asignando el proveedor
        var editar = await cliente.PutAsJsonAsync($"/productos/{productoId}", new
        {
            Codigo = codigoProd,
            Nombre = "Producto sin Proveedor",
            TipoItem = 1,
            PrecioUnitario = 2000m,
            TarifaIvaImpuestoCodigo = "08",
            ProveedorId = proveedorId
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, editar.StatusCode);

        // GET producto → proveedorId persistido tras editar
        var obtener = await cliente.GetAsync($"/productos/{productoId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, obtener.StatusCode);
        var body = await obtener.Content.ReadFromJsonAsync<ProductoConProveedorResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(proveedorId, body.ProveedorId);
    }

    // ──────────────────────────────────────────────
    // Detalle de producto muestra el nombre del proveedor
    // AUNQUE el proveedor haya quedado inactivo (denormalizado)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DetalleProducto_MuestraProveedorNombre_AunqueProveedorInactivo()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        // Crear proveedor
        var nombreProv = $"Prov-Inact-{Guid.NewGuid():N}"[..20];
        var crearProv = await cliente.PostAsJsonAsync("/proveedores", new { Nombre = nombreProv }, TestContext.Current.CancellationToken);
        crearProv.EnsureSuccessStatusCode();
        var proveedorId = await crearProv.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // Crear producto con ese proveedor
        var codigoProd = $"PIN-{Guid.NewGuid():N}"[..15];
        var crearProd = await cliente.PostAsJsonAsync("/productos", new
        {
            Codigo = codigoProd,
            Nombre = "Producto Prov Inactivo",
            TipoItem = 1,
            PrecioUnitario = 1000m,
            TarifaIvaImpuestoCodigo = "08",
            ProveedorId = proveedorId
        }, TestContext.Current.CancellationToken);
        crearProd.EnsureSuccessStatusCode();
        var productoId = await crearProd.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // Desactivar el proveedor (soft delete)
        var desactivar = await cliente.PutAsJsonAsync($"/proveedores/{proveedorId}", new
        {
            Nombre = nombreProv,
            Activo = false
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, desactivar.StatusCode);

        // GET detalle producto → el nombre del proveedor sigue visible pese a estar inactivo
        var obtener = await cliente.GetAsync($"/productos/{productoId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, obtener.StatusCode);
        var body = await obtener.Content.ReadFromJsonAsync<ProductoConProveedorResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);
        Assert.Equal(proveedorId, body.ProveedorId);
        Assert.Equal(nombreProv, body.ProveedorNombre);
    }

    // ──────────────────────────────────────────────
    // GET /proveedores/activos → 200 lista
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ObtenerProveedoresActivos_Retorna200_ConListado()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = ConstruirCliente(token);

        // Crear al menos un proveedor activo
        await cliente.PostAsJsonAsync("/proveedores", new
        {
            Nombre = $"Prov-ACT-{Guid.NewGuid():N}"[..20]
        }, TestContext.Current.CancellationToken);

        var response = await cliente.GetAsync("/proveedores/activos", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<ProveedorResponse>>(TestContext.Current.CancellationToken);
        Assert.NotNull(lista);
        Assert.True(lista.Count >= 1);
        Assert.All(lista, p => Assert.True(p.Activo));
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

    private sealed record ProveedorResponse(
        Guid Id,
        string Nombre,
        string? Correo,
        string? Telefono,
        string? Observacion,
        bool Activo);

    private sealed record ProductoConProveedorResponse(
        Guid Id,
        string Codigo,
        Guid? ProveedorId,
        string? ProveedorNombre);
}
