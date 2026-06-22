using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.UnitTests.Domain.Ventas;

public class DocumentoVentaTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static readonly DateTime FechaValida = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid CajaId = Guid.NewGuid();

    private static DocumentoVenta CrearBorradorFactura(
        string condicion = "01",
        Guid? clienteId = null,
        int? plazoCreditoDias = null)
    {
        var doc = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            clienteId,
            null,
            condicion,
            "Contado",
            FechaValida,
            "CRC",
            1m,
            plazoCreditoDias);
        return doc.Value;
    }

    private static void AgregarLineaSimple(DocumentoVenta doc, decimal precio = 1000m, decimal cantidad = 1m)
    {
        doc.AgregarLinea(
            Guid.NewGuid(),
            TipoItem.Bien,
            "PROD001",
            "Producto de prueba",
            "Unidad",
            cantidad,
            precio);
    }

    private static void AgregarPagoEfectivo(DocumentoVenta doc, decimal monto = 1000m)
    {
        doc.AgregarPago(
            "CRC", 1m, "01", "Efectivo",
            monto, monto, monto, 0m, 0m);
    }

    // ──────────────────────────────────────────────
    // Crear — camino feliz
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarBorrador_CuandoDatosMinimosValidos()
    {
        var resultado = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            null, null,
            "01", "Contado",
            FechaValida);

        Assert.False(resultado.IsError);
        var doc = resultado.Value;
        Assert.Equal(TipoDocumentoVenta.Factura, doc.TipoDocumento);
        Assert.Equal(EstadoDocumentoVenta.Borrador, doc.Estado);
        Assert.Equal("01", doc.CondicionVentaCodigo);
        Assert.Equal("CRC", doc.MonedaCodigo);
        Assert.Equal(0m, doc.TotalComprobante);
        Assert.Null(doc.Consecutivo);
        Assert.NotEqual(Guid.Empty, doc.Id);
    }

    [Fact]
    public void Crear_DebeCrearApartadoEnEstadoReservado()
    {
        var resultado = DocumentoVenta.Crear(
            TipoDocumentoVenta.Apartado,
            null, null, "01", "Contado",
            FechaValida,
            fechaVencimiento: FechaValida.AddDays(30));

        Assert.False(resultado.IsError);
        Assert.Equal(EstadoDocumentoVenta.Reservado, resultado.Value.Estado);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoCondicionVentaVacia()
    {
        var resultado = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            null, null, string.Empty, "Contado",
            FechaValida);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.CondicionVentaRequerida.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoFechaDefault()
    {
        var resultado = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            null, null, "01", "Contado",
            default);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.FechaRequerida.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoTipoCambioCero()
    {
        var resultado = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            null, null, "01", "Contado",
            FechaValida, tipoCambio: 0m);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.TipoCambioInvalido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoApartadoSinFechaVencimiento()
    {
        var resultado = DocumentoVenta.Crear(
            TipoDocumentoVenta.Apartado,
            null, null, "01", "Contado",
            FechaValida);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.FechaVencimientoRequerida.Code);
    }

    // ──────────────────────────────────────────────
    // AgregarLinea
    // ──────────────────────────────────────────────

    [Fact]
    public void AgregarLinea_DebeAgregarYRecalcularTotales()
    {
        var doc = CrearBorradorFactura();

        var resultado = doc.AgregarLinea(Guid.NewGuid(), TipoItem.Bien, "P001", "Prod", "Unidad", 2m, 500m);

        Assert.False(resultado.IsError);
        Assert.Single(doc.Lineas);
        Assert.Equal(1000m, doc.TotalComprobante);
    }

    [Fact]
    public void AgregarLinea_DebeRetornarError_CuandoDocumentoYaEmitido()
    {
        var doc = CrearBorradorFactura();
        AgregarLineaSimple(doc, 1000m);
        AgregarPagoEfectivo(doc, 1000m);
        doc.Emitir(CajaId, "FAC-0000000001");

        var resultado = doc.AgregarLinea(Guid.NewGuid(), TipoItem.Bien, "P002", "Otro", "Unidad", 1m, 500m);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.DocumentoNoEditable.Code);
    }

    // ──────────────────────────────────────────────
    // Totales — cálculo correcto
    // ──────────────────────────────────────────────

    [Fact]
    public void RecalcularTotales_DebeCalcularCorrectamente_CuandoMultiplesLineas()
    {
        var doc = CrearBorradorFactura();
        doc.AgregarLinea(Guid.NewGuid(), TipoItem.Bien, "P001", "Prod1", "Unidad", 2m, 1000m);
        doc.AgregarLinea(Guid.NewGuid(), TipoItem.Bien, "P002", "Prod2", "Unidad", 1m, 500m);

        // 2*1000 + 1*500 = 2500
        Assert.Equal(2500m, doc.TotalComprobante);
        Assert.Equal(2m, doc.Lineas.Count);
    }

    [Fact]
    public void RecalcularTotales_DebeCalcularImpuesto_CuandoHayTarifaIva()
    {
        var doc = CrearBorradorFactura();
        doc.AgregarLinea(
            Guid.NewGuid(), TipoItem.Bien, "P001", "Prod", "Unidad",
            1m, 1000m,
            tarifaIvaImpuestoCodigo: "08",
            porcentajeImpuesto: 13m);

        // 1000 * 0.13 = 130
        Assert.Equal(130m, doc.TotalImpuesto);
        Assert.Equal(1130m, doc.TotalComprobante);
    }

    // ──────────────────────────────────────────────
    // Emitir
    // ──────────────────────────────────────────────

    [Fact]
    public void Emitir_DebeEmitir_CuandoDatosValidos()
    {
        var doc = CrearBorradorFactura();
        AgregarLineaSimple(doc, 1000m);
        AgregarPagoEfectivo(doc, 1000m);

        var resultado = doc.Emitir(CajaId, "FAC-0000000001");

        Assert.False(resultado.IsError);
        Assert.Equal(EstadoDocumentoVenta.Emitido, doc.Estado);
        Assert.Equal("FAC-0000000001", doc.Consecutivo);
        Assert.Equal(CajaId, doc.CajaId);
    }

    [Fact]
    public void Emitir_DebeRetornarError_CuandoNoEsFactura()
    {
        var proforma = DocumentoVenta.Crear(
            TipoDocumentoVenta.Proforma,
            null, null, "01", "Contado", FechaValida).Value;

        var resultado = proforma.Emitir(CajaId, "FAC-001");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.DocumentoNoEmitible.Code);
    }

    [Fact]
    public void Emitir_DebeRetornarError_CuandoSinLineas()
    {
        var doc = CrearBorradorFactura();

        var resultado = doc.Emitir(CajaId, "FAC-001");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.DetallesRequeridos.Code);
    }

    [Fact]
    public void Emitir_DebeRetornarError_CuandoContadoSinPagos()
    {
        var doc = CrearBorradorFactura("01");
        AgregarLineaSimple(doc, 1000m);
        // no se agrega pago

        var resultado = doc.Emitir(CajaId, "FAC-001");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.PagosRequeridos.Code);
    }

    [Fact]
    public void Emitir_DebeRetornarError_CuandoConsecutivoVacio()
    {
        var doc = CrearBorradorFactura();
        AgregarLineaSimple(doc, 1000m);
        AgregarPagoEfectivo(doc, 1000m);

        var resultado = doc.Emitir(CajaId, "   ");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.ConsecutivoRequerido.Code);
    }

    [Fact]
    public void Emitir_DebeTratarGuidVacioComoSinCaja()
    {
        var doc = CrearBorradorFactura();
        AgregarLineaSimple(doc, 1000m);
        AgregarPagoEfectivo(doc, 1000m);

        // Guid.Empty se trata como null — caja es opcional, no debe haber error
        var resultado = doc.Emitir(Guid.Empty, "FAC-001");

        Assert.False(resultado.IsError);
        Assert.Null(doc.CajaId);
    }

    // ──────────────────────────────────────────────
    // MarcarConvertido
    // ──────────────────────────────────────────────

    [Fact]
    public void MarcarConvertido_DebeMarcar_CuandoApartadoReservado()
    {
        var doc = DocumentoVenta.Crear(
            TipoDocumentoVenta.Apartado,
            null, null, "01", "Contado",
            FechaValida,
            fechaVencimiento: FechaValida.AddDays(30)).Value;

        var resultado = doc.MarcarConvertido();

        Assert.False(resultado.IsError);
        Assert.Equal(EstadoDocumentoVenta.Convertido, doc.Estado);
    }

    [Fact]
    public void MarcarConvertido_DebeRetornarError_CuandoFacturaEmitida()
    {
        var doc = CrearBorradorFactura();
        AgregarLineaSimple(doc, 1000m);
        AgregarPagoEfectivo(doc, 1000m);
        doc.Emitir(CajaId, "FAC-001");

        var resultado = doc.MarcarConvertido();

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.DocumentoNoConvertible.Code);
    }

    // ──────────────────────────────────────────────
    // RegistrarAbonoCredito
    // ──────────────────────────────────────────────

    [Fact]
    public void RegistrarAbonoCredito_DebeRetornarError_CuandoNoEsCredito()
    {
        var doc = CrearBorradorFactura("01");
        AgregarLineaSimple(doc, 1000m);
        AgregarPagoEfectivo(doc, 1000m);
        doc.Emitir(CajaId, "FAC-001");

        var resultado = doc.RegistrarAbonoCredito(
            "CRC", 1m, "EFEC", "Efectivo",
            500m, 500m, 500m, 0m, 0m,
            FechaValida, FechaValida, FechaValida);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.AbonoSoloEnCredito.Code);
    }

    [Fact]
    public void RegistrarAbonoCredito_DebeAsignarNumeroAbonoSecuencial()
    {
        var doc = CrearBorradorFactura("02", Guid.NewGuid(), 30);
        AgregarLineaSimple(doc, 1000m);
        doc.Emitir(CajaId, "FAC-CRED-001");

        var abono1 = doc.RegistrarAbonoCredito(
            "CRC", 1m, "01", "Efectivo",
            300m, 300m, 300m, 0m, 0m,
            FechaValida.AddHours(1), FechaValida.AddHours(1), FechaValida.AddHours(1));
        var abono2 = doc.RegistrarAbonoCredito(
            "CRC", 1m, "01", "Efectivo",
            200m, 200m, 200m, 0m, 0m,
            FechaValida.AddHours(2), FechaValida.AddHours(2), FechaValida.AddHours(2));

        Assert.False(abono1.IsError);
        Assert.False(abono2.IsError);
        Assert.Equal(1, abono1.Value.NumeroAbono);
        Assert.Equal(2, abono2.Value.NumeroAbono);
        Assert.Equal(500m, doc.TotalPagado);
        Assert.Equal(500m, doc.SaldoPendiente);
    }

    [Fact]
    public void AnularAbono_DebeExcluirPagoAnuladoDelSaldoYTotalPagado()
    {
        var doc = CrearBorradorFactura("02", Guid.NewGuid(), 30);
        AgregarLineaSimple(doc, 1000m);
        doc.Emitir(CajaId, "FAC-CRED-002");
        var abono = doc.RegistrarAbonoCredito(
            "CRC", 1m, "01", "Efectivo",
            400m, 400m, 400m, 0m, 0m,
            FechaValida.AddHours(1), FechaValida.AddHours(1), FechaValida.AddHours(1)).Value;

        var resultado = doc.AnularAbono(abono.Id, Guid.NewGuid(), "Reversa", FechaValida.AddHours(2));

        Assert.False(resultado.IsError);
        Assert.True(abono.Anulado);
        Assert.Equal(0m, doc.TotalPagado);
        Assert.Equal(1000m, doc.SaldoPendiente);
        Assert.Null(doc.FechaCancelacion);
    }

    [Fact]
    public void AnularAbono_DebeRetornarError_CuandoPagoYaAnulado()
    {
        var doc = CrearBorradorFactura("02", Guid.NewGuid(), 30);
        AgregarLineaSimple(doc, 1000m);
        doc.Emitir(CajaId, "FAC-CRED-003");
        var abono = doc.RegistrarAbonoCredito(
            "CRC", 1m, "01", "Efectivo",
            400m, 400m, 400m, 0m, 0m,
            FechaValida.AddHours(1), FechaValida.AddHours(1), FechaValida.AddHours(1)).Value;
        doc.AnularAbono(abono.Id, Guid.NewGuid(), "Primera", FechaValida.AddHours(2));

        var resultado = doc.AnularAbono(abono.Id, Guid.NewGuid(), "Segunda", FechaValida.AddHours(3));

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaPagoErrors.YaAnulado.Code);
    }

    [Fact]
    public void AnularAbono_DebeRetornarError_CuandoPagoNoExiste()
    {
        var doc = CrearBorradorFactura("02", Guid.NewGuid(), 30);
        AgregarLineaSimple(doc, 1000m);
        doc.Emitir(CajaId, "FAC-CRED-004");

        var resultado = doc.AnularAbono(Guid.NewGuid(), Guid.NewGuid(), "No existe", FechaValida.AddHours(2));

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaPagoErrors.NoEncontrado.Code);
    }

    // ──────────────────────────────────────────────
    // Cancelar apartado
    // ──────────────────────────────────────────────

    [Fact]
    public void Cancelar_DebeMarcarCancelado_CuandoApartadoReservado()
    {
        var doc = DocumentoVenta.Crear(
            TipoDocumentoVenta.Apartado,
            null, null, "01", "Contado",
            FechaValida,
            fechaVencimiento: FechaValida.AddDays(30)).Value;

        var resultado = doc.Cancelar();

        Assert.False(resultado.IsError);
        Assert.Equal(EstadoDocumentoVenta.Cancelado, doc.Estado);
    }

    [Fact]
    public void Cancelar_DebeRetornarError_CuandoFacturaBorrador()
    {
        var doc = CrearBorradorFactura();

        var resultado = doc.Cancelar();

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.DocumentoNoCancelable.Code);
    }
}
