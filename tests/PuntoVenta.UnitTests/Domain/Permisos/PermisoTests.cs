using ErrorOr;
using PuntoVenta.Domain.Entities.Permisos;

namespace PuntoVenta.UnitTests.Domain.Permisos;

public class PermisoTests
{
    // ─── FACTORY: camino feliz ───────────────────────────────────────────────

    [Fact]
    public void Crear_ConDatosValidos_RetornaPermiso()
    {
        var resultado = Permiso.Crear("roles:crear", "Crear roles", "roles");

        Assert.False(resultado.IsError);
        Assert.Equal("roles:crear", resultado.Value.Clave);
        Assert.Equal("Crear roles", resultado.Value.Descripcion);
        Assert.Equal("roles", resultado.Value.Modulo);
        Assert.True(resultado.Value.Activo);
        Assert.NotEqual(Guid.Empty, resultado.Value.Id);
    }

    // ─── FACTORY: normalización de clave ─────────────────────────────────────

    [Fact]
    public void Crear_ClaveConMayusculas_NormalizaALowercase()
    {
        var resultado = Permiso.Crear("Roles:Crear", "Crear roles", "Roles");

        Assert.False(resultado.IsError);
        Assert.Equal("roles:crear", resultado.Value.Clave);
        Assert.Equal("roles", resultado.Value.Modulo);
    }

    [Fact]
    public void Crear_ClaveConEspacios_AplicaTrimYLowercase()
    {
        var resultado = Permiso.Crear("  Roles:Ver  ", "Ver roles", "  Roles  ");

        Assert.False(resultado.IsError);
        Assert.Equal("roles:ver", resultado.Value.Clave);
        Assert.Equal("roles", resultado.Value.Modulo);
    }

    [Fact]
    public void Crear_DescripcionConEspacios_AplicaTrim()
    {
        var resultado = Permiso.Crear("roles:ver", "  Ver listado de roles  ", "roles");

        Assert.False(resultado.IsError);
        Assert.Equal("Ver listado de roles", resultado.Value.Descripcion);
    }

    // ─── FACTORY: campo requerido vacío ──────────────────────────────────────

    [Fact]
    public void Crear_ClaveVacia_RetornaError()
    {
        var resultado = Permiso.Crear("", "Crear roles", "roles");

        Assert.True(resultado.IsError);
        Assert.Equal(ErrorType.Validation, resultado.FirstError.Type);
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Clave");
    }

    [Fact]
    public void Crear_ClaveSoloEspacios_RetornaError()
    {
        var resultado = Permiso.Crear("   ", "Crear roles", "roles");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Clave");
    }

    [Fact]
    public void Crear_DescripcionVacia_RetornaError()
    {
        var resultado = Permiso.Crear("roles:crear", "", "roles");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Descripcion");
    }

    [Fact]
    public void Crear_ModuloVacio_RetornaError()
    {
        var resultado = Permiso.Crear("roles:crear", "Crear roles", "");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Modulo");
    }

    // ─── FACTORY: campo excede longitud ──────────────────────────────────────

    [Fact]
    public void Crear_ClaveExcedeLongitud_RetornaError()
    {
        var clave = new string('a', Permiso.ClaveMaxLength + 1);

        var resultado = Permiso.Crear(clave, "Descripción", "modulo");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Clave");
    }

    [Fact]
    public void Crear_DescripcionExcedeLongitud_RetornaError()
    {
        var descripcion = new string('x', Permiso.DescripcionMaxLength + 1);

        var resultado = Permiso.Crear("roles:crear", descripcion, "roles");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Descripcion");
    }

    [Fact]
    public void Crear_ModuloExcedeLongitud_RetornaError()
    {
        var modulo = new string('m', Permiso.ModuloMaxLength + 1);

        var resultado = Permiso.Crear("roles:crear", "Descripción", modulo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Modulo");
    }

    // ─── FACTORY: acumulación de múltiples errores ───────────────────────────

    [Fact]
    public void Crear_TodosCamposVacios_RetornaMultiplesErrores()
    {
        var resultado = Permiso.Crear("", "", "");

        Assert.True(resultado.IsError);
        Assert.True(resultado.Errors.Count >= 3);
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Clave");
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Descripcion");
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Modulo");
    }

    [Fact]
    public void Crear_ClaveYModuloExcedeLongitud_RetormaMultiplesErrores()
    {
        var claveGrande = new string('a', Permiso.ClaveMaxLength + 1);
        var moduloGrande = new string('m', Permiso.ModuloMaxLength + 1);

        var resultado = Permiso.Crear(claveGrande, "Descripción", moduloGrande);

        Assert.True(resultado.IsError);
        Assert.True(resultado.Errors.Count >= 2);
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Clave");
        Assert.Contains(resultado.Errors, e => e.Code == "Permiso_Modulo");
    }

}
