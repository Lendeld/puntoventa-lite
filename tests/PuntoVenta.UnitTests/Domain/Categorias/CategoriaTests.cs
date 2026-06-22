using PuntoVenta.Domain.Entities.Categorias;

namespace PuntoVenta.UnitTests.Domain.Categorias;

public class CategoriaTests
{
    private const string NombreValido = "Bebidas";

    [Fact]
    public void Crear_DebeRetornarCategoria_CuandoNombreValido()
    {
        var resultado = Categoria.Crear(NombreValido);

        Assert.False(resultado.IsError);
        Assert.Equal(NombreValido, resultado.Value.Nombre);
        Assert.Equal(Categoria.NormalizarNombre(NombreValido), resultado.Value.NombreNormalizado);
        Assert.Null(resultado.Value.Descripcion);
        Assert.NotEqual(Guid.Empty, resultado.Value.Id);
    }

    [Fact]
    public void Crear_DebeRetornarCategoria_CuandoNombreYDescripcion()
    {
        var resultado = Categoria.Crear(NombreValido, "Bebidas frías y calientes");

        Assert.False(resultado.IsError);
        Assert.Equal("Bebidas frías y calientes", resultado.Value.Descripcion);
    }

    [Fact]
    public void Crear_DebeTrimearNombre()
    {
        var resultado = Categoria.Crear("  Bebidas  ");

        Assert.False(resultado.IsError);
        Assert.Equal("Bebidas", resultado.Value.Nombre);
    }

    [Fact]
    public void Crear_DebeTrimearDescripcion()
    {
        var resultado = Categoria.Crear(NombreValido, "  desc  ");

        Assert.False(resultado.IsError);
        Assert.Equal("desc", resultado.Value.Descripcion);
    }

    [Fact]
    public void Crear_DebeNormalizarNombreEnMayusculas()
    {
        var resultado = Categoria.Crear("bebidas");

        Assert.False(resultado.IsError);
        Assert.Equal("BEBIDAS", resultado.Value.NombreNormalizado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoNombreVacioOEspacios(string nombre)
    {
        var resultado = Categoria.Crear(nombre);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CategoriaErrors.NombreRequerido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoNombreExcedeLongitudMaxima()
    {
        var nombreLargo = new string('a', Categoria.NombreMaxLength + 1);

        var resultado = Categoria.Crear(nombreLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CategoriaErrors.NombreExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoDescripcionExcedeLongitudMaxima()
    {
        var descLarga = new string('x', Categoria.DescripcionMaxLength + 1);

        var resultado = Categoria.Crear(NombreValido, descLarga);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CategoriaErrors.DescripcionExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebePermitirDescripcionNula()
    {
        var resultado = Categoria.Crear(NombreValido, null);

        Assert.False(resultado.IsError);
        Assert.Null(resultado.Value.Descripcion);
    }

    [Fact]
    public void Actualizar_DebeModificarCampos_CuandoDatosValidos()
    {
        var categoria = Categoria.Crear(NombreValido).Value;

        var resultado = categoria.Actualizar("  Nueva Categ  ", "nueva desc");

        Assert.False(resultado.IsError);
        Assert.Equal("Nueva Categ", categoria.Nombre);
        Assert.Equal("nueva desc", categoria.Descripcion);
    }

    [Fact]
    public void Actualizar_DebeRetornarError_CuandoNombreVacio()
    {
        var categoria = Categoria.Crear(NombreValido).Value;

        var resultado = categoria.Actualizar(string.Empty);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CategoriaErrors.NombreRequerido.Code);
    }
}
