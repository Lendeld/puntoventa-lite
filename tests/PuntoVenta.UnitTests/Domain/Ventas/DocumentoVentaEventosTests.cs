using PuntoVenta.Domain.Common.Events;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.Ventas;
using PuntoVenta.Domain.Entities.Ventas.Eventos;

namespace PuntoVenta.UnitTests.Domain.Ventas;

public sealed class DocumentoVentaEventosTests
{
    private static readonly DateTime FechaValida = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid CajaId = Guid.NewGuid();

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static DocumentoVenta CrearBorradorFactura(
        string condicion = "01",
        Guid? clienteId = null,
        int? plazoCreditoDias = null)
    {
        return DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            clienteId,
            null,
            condicion,
            "Contado",
            FechaValida,
            "CRC",
            1m,
            plazoCreditoDias).Value;
    }

    private static void AgregarLineaSimple(DocumentoVenta doc, decimal precio = 1000m)
    {
        doc.AgregarLinea(
            Guid.NewGuid(),
            TipoItem.Bien,
            "PROD001",
            "Producto prueba",
            "Unidad",
            1m,
            precio);
    }

    private static void AgregarPagoEfectivo(DocumentoVenta doc, decimal monto = 1000m)
    {
        doc.AgregarPago("CRC", 1m, "01", "Efectivo", monto, monto, monto, 0m, 0m);
    }

    private static DocumentoVenta CrearNotaCredito(Guid facturaOrigenId)
    {
        return DocumentoVenta.Crear(
            TipoDocumentoVenta.NotaCredito,
            null,
            null,
            "01",
            "Contado",
            FechaValida,
            "CRC",
            1m,
            documentoOrigenId: facturaOrigenId).Value;
    }

    private static DocumentoVenta CrearNotaDebito(Guid facturaOrigenId)
    {
        return DocumentoVenta.Crear(
            TipoDocumentoVenta.NotaDebito,
            null,
            null,
            "01",
            "Contado",
            FechaValida,
            "CRC",
            1m,
            documentoOrigenId: facturaOrigenId).Value;
    }

    // ── Tests ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Emitir_FacturaContado_RegistraFacturaEmitidaEvento()
    {
        var doc = CrearBorradorFactura();
        AgregarLineaSimple(doc);
        AgregarPagoEfectivo(doc);

        var resultado = doc.Emitir(CajaId, "FAC-000001");

        Assert.False(resultado.IsError);
        var eventos = doc.EventosDominio;
        Assert.Single(eventos);
        var evento = Assert.IsType<FacturaEmitidaEvento>(eventos[0]);
        Assert.Equal(doc.Id, evento.DocumentoVentaId);
        Assert.Equal("FAC-000001", evento.Consecutivo);
        Assert.Equal(1000m, evento.TotalComprobante);
        Assert.Equal("CRC", evento.MonedaCodigo);
        Assert.Null(evento.ClienteId);
        Assert.False(evento.EsCredito);
    }

    [Fact]
    public void Emitir_FacturaCredito_RegistraFacturaEmitidaEventoConEsCreditoTrue()
    {
        var clienteId = Guid.NewGuid();
        var doc = CrearBorradorFactura(condicion: "02", clienteId: clienteId, plazoCreditoDias: 30);
        AgregarLineaSimple(doc);

        var resultado = doc.Emitir(CajaId, "FAC-000002");

        Assert.False(resultado.IsError);
        var eventos = doc.EventosDominio;
        Assert.Single(eventos);
        var evento = Assert.IsType<FacturaEmitidaEvento>(eventos[0]);
        Assert.Equal(clienteId, evento.ClienteId);
        Assert.True(evento.EsCredito);
    }

    [Fact]
    public void ConfirmarNota_NotaCredito_RegistraNotaCreditoEmitidaEvento()
    {
        var facturaOrigenId = Guid.NewGuid();
        var nc = CrearNotaCredito(facturaOrigenId);
        AgregarLineaSimple(nc, 500m);
        nc.AgregarReferencia(facturaOrigenId, "FC", FechaValida, null);

        var resultado = nc.ConfirmarNota(1, CajaId, "NC-0000000001");

        Assert.False(resultado.IsError);
        var eventos = nc.EventosDominio;
        Assert.Single(eventos);
        var evento = Assert.IsType<NotaCreditoEmitidaEvento>(eventos[0]);
        Assert.Equal(nc.Id, evento.DocumentoVentaId);
        Assert.Equal("NC-0000000001", evento.Consecutivo);
        Assert.Equal(500m, evento.TotalComprobante);
        Assert.Equal("CRC", evento.MonedaCodigo);
        Assert.Equal(facturaOrigenId, evento.DocumentoOrigenId);
    }

    [Fact]
    public void ConfirmarNota_NotaDebito_NoRegistraEvento()
    {
        var facturaOrigenId = Guid.NewGuid();
        var nd = CrearNotaDebito(facturaOrigenId);
        AgregarLineaSimple(nd, 200m);
        nd.AgregarReferencia(facturaOrigenId, "FC", FechaValida, null);

        var resultado = nd.ConfirmarNota(1, CajaId, "ND-0000000001");

        Assert.False(resultado.IsError);
        Assert.Empty(nd.EventosDominio);
    }

    [Fact]
    public void ConfirmarNota_NotaCredito_HeredaCondicionCreditoSinPlazo_NoFalla()
    {
        // Regresión: una NC emitida contra una factura a crédito hereda la condición
        // "02" (crédito) del origen pero no lleva plazo. No debe exigir plazo de crédito.
        var facturaOrigenId = Guid.NewGuid();
        var nc = DocumentoVenta.Crear(
            TipoDocumentoVenta.NotaCredito,
            Guid.NewGuid(),
            null,
            CondicionVentaCodigos.Credito,
            "Crédito",
            FechaValida,
            "CRC",
            1m,
            plazoCreditoDias: null,
            documentoOrigenId: facturaOrigenId).Value;
        AgregarLineaSimple(nc, 500m);
        nc.AgregarReferencia(facturaOrigenId, "FC", FechaValida, null);

        var resultado = nc.ConfirmarNota(1, CajaId, "NC-0000000001");

        Assert.False(resultado.IsError);
    }

    [Fact]
    public void ConfirmarNota_NotaDebito_HeredaCondicionCreditoSinPlazo_NoFalla()
    {
        // Regresión: misma situación para una ND contra factura a crédito.
        var facturaOrigenId = Guid.NewGuid();
        var nd = DocumentoVenta.Crear(
            TipoDocumentoVenta.NotaDebito,
            Guid.NewGuid(),
            null,
            CondicionVentaCodigos.Credito,
            "Crédito",
            FechaValida,
            "CRC",
            1m,
            plazoCreditoDias: null,
            documentoOrigenId: facturaOrigenId).Value;
        AgregarLineaSimple(nd, 200m);
        nd.AgregarReferencia(facturaOrigenId, "FC", FechaValida, null);

        var resultado = nd.ConfirmarNota(1, CajaId, "ND-0000000001");

        Assert.False(resultado.IsError);
    }

    [Fact]
    public void LimpiarEventos_DespuesDeEmitir_EventosDominioQuedaVacio()
    {
        var doc = CrearBorradorFactura();
        AgregarLineaSimple(doc);
        AgregarPagoEfectivo(doc);
        doc.Emitir(CajaId, "FAC-000003");

        Assert.Single(doc.EventosDominio);

        doc.LimpiarEventos();

        Assert.Empty(doc.EventosDominio);
    }

    [Fact]
    public void EventosDominio_NuevoDocumento_EstaVacio()
    {
        var doc = CrearBorradorFactura();
        Assert.Empty(doc.EventosDominio);
    }
}
