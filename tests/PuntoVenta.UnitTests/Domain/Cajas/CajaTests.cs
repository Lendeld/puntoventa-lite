using PuntoVenta.Domain.Entities.Cajas;

namespace PuntoVenta.UnitTests.Domain.Cajas;

public class CajaTests
{
    private const string CodigoValido = "CAJA01";
    private const string NombreValido = "Caja Principal";

    [Fact]
    public void Crear_DebeRetornarCaja_CuandoDatosValidos()
    {
        var resultado = Caja.Crear(CodigoValido, NombreValido);

        Assert.False(resultado.IsError);
        var caja = resultado.Value;
        Assert.Equal(CodigoValido, caja.Codigo);
        Assert.Equal(Caja.NormalizarCodigo(CodigoValido), caja.CodigoNormalizado);
        Assert.Equal(NombreValido, caja.Nombre);
        Assert.NotEqual(Guid.Empty, caja.Id);
    }

    [Fact]
    public void Crear_DebeTrimearCodigo()
    {
        var resultado = Caja.Crear("  CAJA01  ", NombreValido);

        Assert.False(resultado.IsError);
        Assert.Equal("CAJA01", resultado.Value.Codigo);
    }

    [Fact]
    public void Crear_DebeTrimearNombre()
    {
        var resultado = Caja.Crear(CodigoValido, "  Caja Principal  ");

        Assert.False(resultado.IsError);
        Assert.Equal("Caja Principal", resultado.Value.Nombre);
    }

    [Fact]
    public void Crear_DebeNormalizarCodigoEnMayusculas()
    {
        var resultado = Caja.Crear("caja01", NombreValido);

        Assert.False(resultado.IsError);
        Assert.Equal("CAJA01", resultado.Value.CodigoNormalizado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoCodigoVacioOEspacios(string codigo)
    {
        var resultado = Caja.Crear(codigo, NombreValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CajaErrors.CodigoRequerido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoCodigoExcedeLongitudMaxima()
    {
        var codigoLargo = new string('A', Caja.CodigoMaxLength + 1);

        var resultado = Caja.Crear(codigoLargo, NombreValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CajaErrors.CodigoExcedeLongitud.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoNombreVacioOEspacios(string nombre)
    {
        var resultado = Caja.Crear(CodigoValido, nombre);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CajaErrors.NombreRequerido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoNombreExcedeLongitudMaxima()
    {
        var nombreLargo = new string('a', Caja.NombreMaxLength + 1);

        var resultado = Caja.Crear(CodigoValido, nombreLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CajaErrors.NombreExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebeRetornarMultiplesErrores_CuandoVariosCamposInvalidos()
    {
        var resultado = Caja.Crear(string.Empty, string.Empty);

        Assert.True(resultado.IsError);
        Assert.True(resultado.Errors.Count >= 2);
        Assert.Contains(resultado.Errors, e => e.Code == CajaErrors.CodigoRequerido.Code);
        Assert.Contains(resultado.Errors, e => e.Code == CajaErrors.NombreRequerido.Code);
    }

    [Fact]
    public void Actualizar_DebeModificarCampos_CuandoDatosValidos()
    {
        var caja = Caja.Crear(CodigoValido, NombreValido).Value;

        var resultado = caja.Actualizar("  CAJA02  ", "  Caja Secundaria  ");

        Assert.False(resultado.IsError);
        Assert.Equal("CAJA02", caja.Codigo);
        Assert.Equal("CAJA02", caja.CodigoNormalizado);
        Assert.Equal("Caja Secundaria", caja.Nombre);
    }

    [Fact]
    public void Actualizar_DebeRetornarError_CuandoCodigoVacio()
    {
        var caja = Caja.Crear(CodigoValido, NombreValido).Value;

        var resultado = caja.Actualizar(string.Empty, NombreValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CajaErrors.CodigoRequerido.Code);
    }
}
