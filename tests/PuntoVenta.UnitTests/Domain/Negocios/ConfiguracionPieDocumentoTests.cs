using PuntoVenta.Domain.Entities.Negocios;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.UnitTests.Domain.Negocios;

public class ConfiguracionPieDocumentoTests
{
    private static LineaPieDocumento Linea(string texto = "x", int orden = 0)
        => LineaPieDocumento.Crear(texto, AlineacionLineaPie.Izquierda, false, orden).Value;

    [Fact]
    public void Crear_DatosValidos_HaceTrimYAsigna()
    {
        var resultado = ConfiguracionPieDocumento.Crear(
            "  Facturas  ",
            DestinoLineaPie.Pdf,
            [TipoDocumentoVenta.Factura],
            [Linea("Cuenta BAC")]);

        Assert.False(resultado.IsError);
        Assert.Equal("Facturas", resultado.Value.Nombre);
        Assert.Equal(DestinoLineaPie.Pdf, resultado.Value.Destino);
        Assert.Equal([TipoDocumentoVenta.Factura], resultado.Value.TiposDocumento);
        Assert.Single(resultado.Value.Lineas);
        Assert.False(resultado.Value.EsTodos);
    }

    [Fact]
    public void Crear_SinTipos_EsTodos()
    {
        var resultado = ConfiguracionPieDocumento.Crear("General", DestinoLineaPie.Ticket, null, [Linea()]);

        Assert.False(resultado.IsError);
        Assert.True(resultado.Value.EsTodos);
        Assert.True(resultado.Value.AplicaA(TipoDocumentoVenta.Factura));
        Assert.True(resultado.Value.AplicaA(TipoDocumentoVenta.Proforma));
    }

    [Fact]
    public void Crear_NombreVacio_RetornaError()
    {
        var resultado = ConfiguracionPieDocumento.Crear("  ", DestinoLineaPie.Pdf, null, null);

        Assert.True(resultado.IsError);
        Assert.Equal(ConfiguracionPieDocumentoErrors.NombreRequerido.Code, resultado.FirstError.Code);
    }

    [Fact]
    public void Crear_NombreExcedeLongitud_RetornaError()
    {
        var nombre = new string('a', ConfiguracionPieDocumento.MaxNombreLength + 1);

        var resultado = ConfiguracionPieDocumento.Crear(nombre, DestinoLineaPie.Pdf, null, null);

        Assert.True(resultado.IsError);
        Assert.Equal(ConfiguracionPieDocumentoErrors.NombreExcedeLongitud.Code, resultado.FirstError.Code);
    }

    [Fact]
    public void Crear_TipoDocumentoInvalido_RetornaError()
    {
        var resultado = ConfiguracionPieDocumento.Crear(
            "x", DestinoLineaPie.Pdf, [(TipoDocumentoVenta)999], null);

        Assert.True(resultado.IsError);
        Assert.Equal(ConfiguracionPieDocumentoErrors.TipoDocumentoInvalido.Code, resultado.FirstError.Code);
    }

    [Fact]
    public void Crear_TiposDuplicados_SeDeduplican()
    {
        var resultado = ConfiguracionPieDocumento.Crear(
            "x", DestinoLineaPie.Pdf, [TipoDocumentoVenta.Factura, TipoDocumentoVenta.Factura], null);

        Assert.False(resultado.IsError);
        Assert.Single(resultado.Value.TiposDocumento);
    }

    [Fact]
    public void Crear_DemasiadasLineas_RetornaError()
    {
        var lineas = Enumerable.Range(0, ConfiguracionPieDocumento.MaxLineas + 1)
            .Select(i => Linea($"linea {i}", i))
            .ToList();

        var resultado = ConfiguracionPieDocumento.Crear("x", DestinoLineaPie.Pdf, null, lineas);

        Assert.True(resultado.IsError);
        Assert.Equal(ConfiguracionPieDocumentoErrors.DemasiadasLineas.Code, resultado.FirstError.Code);
    }
}
