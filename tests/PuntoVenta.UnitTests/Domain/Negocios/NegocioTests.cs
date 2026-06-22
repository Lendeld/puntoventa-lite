using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.UnitTests.Domain.Negocios;

public class NegocioTests
{
    private const string NombreValido = "Mi Tienda";

    [Fact]
    public void Crear_DebeRetornarNegocio_CuandoSoloNombre()
    {
        var resultado = Negocio.Crear(NombreValido);

        Assert.False(resultado.IsError);
        var negocio = resultado.Value;
        Assert.Equal(NombreValido, negocio.Nombre);
        Assert.Null(negocio.NombreComercial);
        Assert.Null(negocio.Direccion);
        Assert.Null(negocio.Identificacion);
        Assert.Null(negocio.Correo);
        Assert.Null(negocio.Telefono);
        Assert.False(negocio.AplicaVendedores);
        Assert.False(negocio.AplicaCajas);
        Assert.Equal(Negocio.TipoCambioPredeterminadoDefault, negocio.TipoCambioPredeterminado);
        Assert.NotEqual(Guid.Empty, negocio.Id);
    }

    [Fact]
    public void Crear_DebeUsarTipoCambioIndicado_CuandoEsPositivo()
    {
        var resultado = Negocio.Crear(NombreValido, tipoCambioPredeterminado: 620m);

        Assert.False(resultado.IsError);
        Assert.Equal(620m, resultado.Value.TipoCambioPredeterminado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Crear_DebeRetornarError_CuandoTipoCambioNoEsPositivo(double tipoCambio)
    {
        var resultado = Negocio.Crear(NombreValido, tipoCambioPredeterminado: (decimal)tipoCambio);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == NegocioErrors.TipoCambioPredeterminadoInvalido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarNegocio_CuandoAplicaVendedoresTrue()
    {
        var resultado = Negocio.Crear(NombreValido, aplicaVendedores: true);

        Assert.False(resultado.IsError);
        Assert.True(resultado.Value.AplicaVendedores);
        Assert.False(resultado.Value.AplicaCajas);
    }

    [Fact]
    public void Crear_DebeRetornarNegocio_CuandoAplicaCajasTrue()
    {
        var resultado = Negocio.Crear(NombreValido, aplicaGestionCajas: true);

        Assert.False(resultado.IsError);
        Assert.False(resultado.Value.AplicaVendedores);
        Assert.True(resultado.Value.AplicaCajas);
    }

    [Fact]
    public void Crear_DebeRetornarNegocio_CuandoAmbosFlags()
    {
        var resultado = Negocio.Crear(NombreValido, aplicaVendedores: true, aplicaGestionCajas: true);

        Assert.False(resultado.IsError);
        Assert.True(resultado.Value.AplicaVendedores);
        Assert.True(resultado.Value.AplicaCajas);
    }

    [Fact]
    public void Crear_DebeTrimearNombre()
    {
        var resultado = Negocio.Crear("  Mi Tienda  ");

        Assert.False(resultado.IsError);
        Assert.Equal("Mi Tienda", resultado.Value.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoNombreVacioOEspacios(string nombre)
    {
        var resultado = Negocio.Crear(nombre);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == NegocioErrors.NombreRequerido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoNombreExcedeLongitudMaxima()
    {
        var nombreLargo = new string('a', Negocio.NombreMaxLength + 1);

        var resultado = Negocio.Crear(nombreLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == NegocioErrors.NombreExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoNombreComercialExcedeLongitudMaxima()
    {
        var nombreComLargo = new string('a', Negocio.NombreComercialMaxLength + 1);

        var resultado = Negocio.Crear(NombreValido, nombreComercial: nombreComLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == NegocioErrors.NombreComercialExcedeLongitud.Code);
    }

    [Fact]
    public void Actualizar_DebeModificarFlags_CuandoDatosValidos()
    {
        var negocio = Negocio.Crear(NombreValido).Value;

        var resultado = negocio.Actualizar("Nuevo Nombre", aplicaVendedores: true, aplicaGestionCajas: true);

        Assert.False(resultado.IsError);
        Assert.Equal("Nuevo Nombre", negocio.Nombre);
        Assert.True(negocio.AplicaVendedores);
        Assert.True(negocio.AplicaCajas);
    }

    [Fact]
    public void ActualizarLogo_DebeAsignarUrl()
    {
        var negocio = Negocio.Crear(NombreValido).Value;

        negocio.ActualizarLogo("https://cdn.example.com/logo.png");

        Assert.Equal("https://cdn.example.com/logo.png", negocio.LogoUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ActualizarLogo_DebeLimpiarUrl_CuandoValorVacioONulo(string? logoUrl)
    {
        var negocio = Negocio.Crear(NombreValido).Value;
        negocio.ActualizarLogo("https://cdn.example.com/logo.png");

        negocio.ActualizarLogo(logoUrl);

        Assert.Null(negocio.LogoUrl);
    }
}
