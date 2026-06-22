using System.Net;
using System.Net.Http.Json;
using PuntoVenta.IntegrationTests.Fixtures;

namespace PuntoVenta.IntegrationTests.Negocio;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class NegocioTicketConfigIntegrationTests(IntegrationTestFixture fixture)
{
    // Bloquea el contrato del wire: el cuerpo PUT debe usar nombres planos
    // (MostrarCodigoBarras, MostrarLogo, …) para que el command bindee. Si el
    // cliente envía claves prefijadas que no bindean, el command queda en sus
    // defaults y los toggles "revierten" al recargar. Regresión real.
    [Fact]
    public async Task ActualizarTicketConfig_PersisteFlags_CuandoCuerpoUsaNombresPlanos()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        fixture.SetBearerToken(token);

        var actualizar = await fixture.Client.PutAsJsonAsync("/negocio/ticket-config", new
        {
            mensajePie = "Gracias por su compra",
            mostrarLogo = false,
            aplicaCopiaClienteNegocio = true,
            mostrarCodigoBarras = false,
            configuraciones = Array.Empty<object>(),
            // elementosEncabezado se omite (null) → conserva el encabezado actual;
            // un arreglo vacío fallaría la validación de elementos fijos.
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, actualizar.StatusCode);

        var obtener = await fixture.Client.GetAsync("/negocio/ticket-config", TestContext.Current.CancellationToken);
        obtener.EnsureSuccessStatusCode();
        var config = await obtener.Content.ReadFromJsonAsync<TicketConfigResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(config);
        Assert.False(config!.MostrarCodigoBarras);
        Assert.False(config.MostrarLogo);
        Assert.True(config.AplicaCopiaClienteNegocio);
        Assert.Equal("Gracias por su compra", config.MensajePie);
    }

    private sealed record TicketConfigResponse(
        string? MensajePie,
        bool MostrarLogo,
        bool AplicaCopiaClienteNegocio,
        bool MostrarCodigoBarras);
}
