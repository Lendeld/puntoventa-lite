using PuntoVenta.Domain.Entities.Vendedores;

namespace PuntoVenta.UnitTests.Domain.Vendedores;

public class VendedorTests
{
    private const string NombreValido = "María García";

    [Fact]
    public void Crear_DebeRetornarVendedor_CuandoNombreValido()
    {
        var resultado = Vendedor.Crear(NombreValido);

        Assert.False(resultado.IsError);
        var vendedor = resultado.Value;
        Assert.Equal(NombreValido, vendedor.Nombre);
        Assert.Equal(Vendedor.NormalizarNombre(NombreValido), vendedor.NombreNormalizado);
        Assert.False(vendedor.IsPrincipal);
        Assert.NotEqual(Guid.Empty, vendedor.Id);
    }

    [Fact]
    public void Crear_DebeRetornarVendedor_CuandoEsPrincipal()
    {
        var resultado = Vendedor.Crear(NombreValido, isPrincipal: true);

        Assert.False(resultado.IsError);
        Assert.True(resultado.Value.IsPrincipal);
    }

    [Fact]
    public void Crear_DebeTrimearNombre()
    {
        var resultado = Vendedor.Crear("  María García  ");

        Assert.False(resultado.IsError);
        Assert.Equal("María García", resultado.Value.Nombre);
    }

    [Fact]
    public void Crear_DebeNormalizarNombreEnMayusculas()
    {
        var resultado = Vendedor.Crear("maría garcía");

        Assert.False(resultado.IsError);
        Assert.Equal("MARÍA GARCÍA", resultado.Value.NombreNormalizado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoNombreVacioOEspacios(string nombre)
    {
        var resultado = Vendedor.Crear(nombre);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == VendedorErrors.NombreRequerido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoNombreExcedeLongitudMaxima()
    {
        var nombreLargo = new string('a', Vendedor.NombreMaxLength + 1);

        var resultado = Vendedor.Crear(nombreLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == VendedorErrors.NombreExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebePermitir_CuandoNombreEsExactamenteLongitudMaxima()
    {
        var nombreExacto = new string('a', Vendedor.NombreMaxLength);

        var resultado = Vendedor.Crear(nombreExacto);

        Assert.False(resultado.IsError);
    }

    [Fact]
    public void MarcarComoPrincipal_DebeAsignarIsPrincipal()
    {
        var vendedor = Vendedor.Crear(NombreValido).Value;

        vendedor.MarcarComoPrincipal();

        Assert.True(vendedor.IsPrincipal);
    }

    [Fact]
    public void QuitarPrincipal_DebeDesasignarIsPrincipal()
    {
        var vendedor = Vendedor.Crear(NombreValido, isPrincipal: true).Value;

        vendedor.QuitarPrincipal();

        Assert.False(vendedor.IsPrincipal);
    }

    [Fact]
    public void Actualizar_DebeModificarCampos_CuandoDatosValidos()
    {
        var vendedor = Vendedor.Crear(NombreValido).Value;

        var resultado = vendedor.Actualizar("  Carlos López  ", isPrincipal: true);

        Assert.False(resultado.IsError);
        Assert.Equal("Carlos López", vendedor.Nombre);
        Assert.True(vendedor.IsPrincipal);
    }

    [Fact]
    public void Actualizar_DebeRetornarError_CuandoNombreVacio()
    {
        var vendedor = Vendedor.Crear(NombreValido).Value;

        var resultado = vendedor.Actualizar(string.Empty);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == VendedorErrors.NombreRequerido.Code);
    }
}
