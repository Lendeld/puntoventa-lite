using System.Net;
using System.Net.Http.Json;
using PuntoVenta.IntegrationTests.Fixtures;

namespace PuntoVenta.IntegrationTests.Clientes;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class ClienteCrudIntegrationTests(IntegrationTestFixture fixture)
{
    // ──────────────────────────────────────────────
    // Crear cliente → 201 + Guid
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CrearCliente_Retorna201_CuandoDatosValidos()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = fixture.Factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await cliente.PostAsJsonAsync("/clientes", new
        {
            Nombre = "Juan Pérez Integration"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var id = await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        Assert.NotEqual(Guid.Empty, id);
    }

    // ──────────────────────────────────────────────
    // Listar clientes → 200 con paginación
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ObtenerListaClientes_Retorna200()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = fixture.Factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await cliente.GetAsync("/clientes?pagina=1&tamano=10", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ──────────────────────────────────────────────
    // Crear cliente sin nombre → 400
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CrearCliente_Retorna400OErrorNegocio_CuandoNombreVacio()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var cliente = fixture.Factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await cliente.PostAsJsonAsync("/clientes", new
        {
            Nombre = ""
        }, TestContext.Current.CancellationToken);

        // FastEndpoints puede devolver 400 (FluentValidation) o 422 (errores de dominio)
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.UnprocessableEntity,
            $"Status inesperado: {response.StatusCode}");
    }
}
