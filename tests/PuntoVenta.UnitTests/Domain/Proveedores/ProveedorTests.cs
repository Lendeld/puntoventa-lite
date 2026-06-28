using PuntoVenta.Domain.Entities.Proveedores;

namespace PuntoVenta.UnitTests.Domain.Proveedores;

public class ProveedorTests
{
    private const string NombreValido = "ACME S.A.";

    // ──────────────────────────────────────────────
    // Camino feliz
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarProveedor_CuandoSoloNombre()
    {
        var resultado = Proveedor.Crear(NombreValido);

        Assert.False(resultado.IsError);
        Assert.Equal(NombreValido, resultado.Value.Nombre);
        Assert.Equal(Proveedor.NormalizarNombre(NombreValido), resultado.Value.NombreNormalizado);
        Assert.Null(resultado.Value.Correo);
        Assert.Null(resultado.Value.Telefono);
        Assert.Null(resultado.Value.Observacion);
        Assert.NotEqual(Guid.Empty, resultado.Value.Id);
    }

    [Fact]
    public void Crear_DebeRetornarProveedor_CuandoTodosLosCampos()
    {
        var resultado = Proveedor.Crear(NombreValido, "ventas@acme.cr", "2222-3333", "Distribuidor zona norte");

        Assert.False(resultado.IsError);
        Assert.Equal("ventas@acme.cr", resultado.Value.Correo);
        Assert.Equal("2222-3333", resultado.Value.Telefono);
        Assert.Equal("Distribuidor zona norte", resultado.Value.Observacion);
    }

    // ──────────────────────────────────────────────
    // Trim
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeTrimearNombre()
    {
        var resultado = Proveedor.Crear("  ACME S.A.  ");

        Assert.False(resultado.IsError);
        Assert.Equal("ACME S.A.", resultado.Value.Nombre);
    }

    [Fact]
    public void Crear_DebeTrimearCorreo()
    {
        var resultado = Proveedor.Crear(NombreValido, "  ventas@acme.cr  ");

        Assert.False(resultado.IsError);
        Assert.Equal("ventas@acme.cr", resultado.Value.Correo);
    }

    [Fact]
    public void Crear_DebeTrimearTelefono()
    {
        var resultado = Proveedor.Crear(NombreValido, null, "  2222-3333  ");

        Assert.False(resultado.IsError);
        Assert.Equal("2222-3333", resultado.Value.Telefono);
    }

    [Fact]
    public void Crear_DebeTrimearObservacion()
    {
        var resultado = Proveedor.Crear(NombreValido, null, null, "  nota  ");

        Assert.False(resultado.IsError);
        Assert.Equal("nota", resultado.Value.Observacion);
    }

    // ──────────────────────────────────────────────
    // Normalización de nombre
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeNormalizarNombreEnMayusculas()
    {
        var resultado = Proveedor.Crear("acme s.a.");

        Assert.False(resultado.IsError);
        Assert.Equal("ACME S.A.", resultado.Value.NombreNormalizado);
    }

    // ──────────────────────────────────────────────
    // Nombre requerido
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoNombreVacioOEspacios(string nombre)
    {
        var resultado = Proveedor.Crear(nombre);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.NombreRequerido.Code);
    }

    // ──────────────────────────────────────────────
    // Longitud de Nombre
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarError_CuandoNombreExcedeLongitudMaxima()
    {
        var nombreLargo = new string('a', Proveedor.NombreMaxLength + 1);

        var resultado = Proveedor.Crear(nombreLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.NombreExcedeLongitud.Code);
    }

    // ──────────────────────────────────────────────
    // Correo
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarError_CuandoCorreoTieneFormatoInvalido()
    {
        var resultado = Proveedor.Crear(NombreValido, "no-es-un-correo");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.CorreoInvalido.Code);
    }

    [Theory]
    [InlineData("ventas@acme.cr")]
    [InlineData("a@b.co")]
    [InlineData("test.user+tag@example.com")]
    public void Crear_DebeAceptarCorreo_CuandoFormatoValido(string correo)
    {
        var resultado = Proveedor.Crear(NombreValido, correo);

        Assert.False(resultado.IsError);
        Assert.Equal(correo, resultado.Value.Correo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeAceptar_CuandoCorreoVacioONulo(string? correo)
    {
        var resultado = Proveedor.Crear(NombreValido, correo);

        Assert.False(resultado.IsError);
        Assert.Null(resultado.Value.Correo);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoCorreoExcedeLongitudMaxima()
    {
        // CorreoMaxLength+1 caracteres: relleno + "@b.co" supera el límite
        var correoLargo = new string('a', Proveedor.CorreoMaxLength - 4) + "@b.co";

        var resultado = Proveedor.Crear(NombreValido, correoLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.CorreoExcedeLongitud.Code);
    }

    // ──────────────────────────────────────────────
    // Longitudes opcionales
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarError_CuandoTelefonoExcedeLongitudMaxima()
    {
        var telefonoLargo = new string('1', Proveedor.TelefonoMaxLength + 1);

        var resultado = Proveedor.Crear(NombreValido, null, telefonoLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.TelefonoExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoObservacionExcedeLongitudMaxima()
    {
        var obsLarga = new string('x', Proveedor.ObservacionMaxLength + 1);

        var resultado = Proveedor.Crear(NombreValido, null, null, obsLarga);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.ObservacionExcedeLongitud.Code);
    }

    // ──────────────────────────────────────────────
    // Múltiples errores acumulados
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeAcumularMultiplesErrores()
    {
        var telefonoLargo = new string('1', Proveedor.TelefonoMaxLength + 1);
        var obsLarga = new string('x', Proveedor.ObservacionMaxLength + 1);

        var resultado = Proveedor.Crear(string.Empty, "no-es-correo", telefonoLargo, obsLarga);

        Assert.True(resultado.IsError);
        Assert.True(resultado.Errors.Count >= 3);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.NombreRequerido.Code);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.CorreoInvalido.Code);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.TelefonoExcedeLongitud.Code);
    }

    // ──────────────────────────────────────────────
    // Actualizar
    // ──────────────────────────────────────────────

    [Fact]
    public void Actualizar_DebeModificarCampos_CuandoDatosValidos()
    {
        var proveedor = Proveedor.Crear(NombreValido).Value;

        var resultado = proveedor.Actualizar("  Nuevo Proveedor  ", "nuevo@proveedor.cr");

        Assert.False(resultado.IsError);
        Assert.Equal("Nuevo Proveedor", proveedor.Nombre);
        Assert.Equal("nuevo@proveedor.cr", proveedor.Correo);
    }

    [Fact]
    public void Actualizar_DebeRetornarError_CuandoNombreVacio()
    {
        var proveedor = Proveedor.Crear(NombreValido).Value;

        var resultado = proveedor.Actualizar(string.Empty);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.NombreRequerido.Code);
    }

    [Fact]
    public void Actualizar_DebeRetornarError_CuandoCorreoInvalido()
    {
        var proveedor = Proveedor.Crear(NombreValido).Value;

        var resultado = proveedor.Actualizar(NombreValido, "malformado");

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.CorreoInvalido.Code);
    }
}
