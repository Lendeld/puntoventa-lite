using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.UnitTests.Domain.Negocios;

public class LineaPieDocumentoTests
{
    [Fact]
    public void Crear_DatosValidos_HaceTrimYAsigna()
    {
        var resultado = LineaPieDocumento.Crear(
            "  Cuenta BAC CR123  ",
            AlineacionLineaPie.Centro,
            negrita: true,
            orden: 2);

        Assert.False(resultado.IsError);
        Assert.Equal("Cuenta BAC CR123", resultado.Value.Texto);
        Assert.Equal(AlineacionLineaPie.Centro, resultado.Value.Alineacion);
        Assert.True(resultado.Value.Negrita);
        Assert.Equal(2, resultado.Value.Orden);
    }

    [Fact]
    public void Crear_TextoVacio_RetornaError()
    {
        var resultado = LineaPieDocumento.Crear("   ", AlineacionLineaPie.Izquierda, false, 0);

        Assert.True(resultado.IsError);
        Assert.Equal(LineaPieDocumentoErrors.TextoRequerido.Code, resultado.FirstError.Code);
    }

    [Fact]
    public void Crear_TextoExcedeLongitud_RetornaError()
    {
        var texto = new string('a', LineaPieDocumento.MaxTextoLength + 1);

        var resultado = LineaPieDocumento.Crear(texto, AlineacionLineaPie.Izquierda, false, 0);

        Assert.True(resultado.IsError);
        Assert.Equal(LineaPieDocumentoErrors.TextoExcedeLongitud.Code, resultado.FirstError.Code);
    }
}
