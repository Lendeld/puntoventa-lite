using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.UnitTests.Domain.Negocios;

public class ElementoEncabezadoTests
{
    [Fact]
    public void Crear_TipoFijo_Valido()
    {
        var resultado = ElementoEncabezado.Crear(ElementoEncabezadoTipo.Telefono, 2, true, null);

        Assert.False(resultado.IsError);
        Assert.Equal(ElementoEncabezadoTipo.Telefono, resultado.Value.Tipo);
        Assert.Equal(2, resultado.Value.Orden);
        Assert.True(resultado.Value.Visible);
        Assert.Null(resultado.Value.TextoLibre);
        Assert.True(resultado.Value.EsFijo);
    }

    [Fact]
    public void Crear_Texto_HaceTrim()
    {
        var resultado = ElementoEncabezado.Crear(ElementoEncabezadoTipo.Texto, 0, true, "  Hola  ");

        Assert.False(resultado.IsError);
        Assert.Equal("Hola", resultado.Value.TextoLibre);
        Assert.False(resultado.Value.EsFijo);
    }

    [Fact]
    public void Crear_FijoIgnoraTextoLibre()
    {
        var resultado = ElementoEncabezado.Crear(ElementoEncabezadoTipo.Correo, 0, true, "no aplica");

        Assert.False(resultado.IsError);
        Assert.Null(resultado.Value.TextoLibre);
    }

    [Fact]
    public void Crear_TextoSinTextoLibre_RetornaError()
    {
        var resultado = ElementoEncabezado.Crear(ElementoEncabezadoTipo.Texto, 0, true, "   ");

        Assert.True(resultado.IsError);
        Assert.Equal(ElementoEncabezadoErrors.TextoLibreRequerido.Code, resultado.FirstError.Code);
    }

    [Fact]
    public void Crear_TextoExcedeLongitud_RetornaError()
    {
        var largo = new string('a', ElementoEncabezado.MaxTextoLibreLength + 1);

        var resultado = ElementoEncabezado.Crear(ElementoEncabezadoTipo.Texto, 0, true, largo);

        Assert.True(resultado.IsError);
        Assert.Equal(ElementoEncabezadoErrors.TextoLibreExcedeLongitud.Code, resultado.FirstError.Code);
    }
}
