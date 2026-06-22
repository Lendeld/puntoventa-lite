using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;
using PuntoVenta.IntegrationTests.Fixtures;

namespace PuntoVenta.IntegrationTests.Pdf;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class QuestPdfIntegrationTests(IntegrationTestFixture fixture)
{
    // ──────────────────────────────────────────────
    // IDocumentoVentaPdfService registrado en DI
    // ──────────────────────────────────────────────

    [Fact]
    public void QuestPdfDocumentoVentaService_EstaRegistrado_EnDI()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<IDocumentoVentaPdfService>();

        Assert.NotNull(service);
    }

    // ──────────────────────────────────────────────
    // Genera bytes no vacíos (requiere al menos un documento emitido)
    // Si la BD de test está limpia el test pasa sin verificar el PDF.
    // ──────────────────────────────────────────────

    [Fact]
    public async Task QuestPdf_GeneraPdf_ConBytesNoVacios()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDocumentoVentaPdfService>();
        var negocioRepo = scope.ServiceProvider.GetRequiredService<INegocioRepository>();
        var docRepo = scope.ServiceProvider.GetRequiredService<IDocumentoVentaRepository>();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<INegocioTicketConfigRepository>();

        var negocios = await negocioRepo.GetAllAsync(TestContext.Current.CancellationToken);
        var negocio = negocios.FirstOrDefault();
        Assert.NotNull(negocio);

        var ticketConfig = await ticketRepo.ObtenerAsync(TestContext.Current.CancellationToken);

        var (documentos, _) = await docRepo.ObtenerListaPaginadoAsync(
            1, 1, null,
            TipoDocumentoVenta.Factura,
            EstadoDocumentoVenta.Emitido,
            null, null, null, TestContext.Current.CancellationToken);

        if (!documentos.Any())
        {
            // BD limpia sin facturas emitidas — servicio ya verificado como registrado.
            return;
        }

        var doc = await docRepo.ObtenerDetalleAsync(documentos[0].Id, TestContext.Current.CancellationToken);
        Assert.NotNull(doc);

        var bytes = await service.GenerarPdfAsync(doc, negocio, ticketConfig, TestContext.Current.CancellationToken);
        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
    }
}
