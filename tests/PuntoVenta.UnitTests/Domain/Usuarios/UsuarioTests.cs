using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.UnitTests.Domain.Usuarios;

public class UsuarioTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private const string NombreUsuarioValido = "jdoe";
    private const string NombreValido = "John Doe";
    private const string IdentificacionValida = "12345678";
    private const string PasswordHashValido = "$2a$11$hash_valido_bcrypt";

    // ──────────────────────────────────────────────
    // Casos exitosos
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarUsuario_CuandoDatosMinimosValidos()
    {
        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido);

        Assert.False(resultado.IsError);
        var usuario = resultado.Value;
        Assert.Equal(NombreUsuarioValido, usuario.NombreUsuario);
        Assert.Equal(NombreValido, usuario.Nombre);
        Assert.Equal(IdentificacionValida, usuario.Identificacion);
        Assert.Equal(PasswordHashValido, usuario.PasswordHash);
        Assert.Null(usuario.Correo);
        Assert.Null(usuario.Telefono);
        Assert.True(usuario.Activo);
        Assert.NotEqual(Guid.Empty, usuario.Id);
    }

    [Fact]
    public void Crear_DebeRetornarUsuario_CuandoTodosLosCamposValidos()
    {
        var correo = "jdoe@example.com";
        var telefono = "+506 8888-1234";

        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido, correo, telefono);

        Assert.False(resultado.IsError);
        Assert.Equal(correo, resultado.Value.Correo);
        Assert.Equal(telefono, resultado.Value.Telefono);
    }

    // ──────────────────────────────────────────────
    // Trim de campos
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeTrimearNombreUsuario()
    {
        var resultado = Usuario.Crear("  jdoe  ", NombreValido, IdentificacionValida, PasswordHashValido);

        Assert.False(resultado.IsError);
        Assert.Equal("jdoe", resultado.Value.NombreUsuario);
    }

    [Fact]
    public void Crear_DebeTrimearNombre()
    {
        var resultado = Usuario.Crear(NombreUsuarioValido, "  John Doe  ", IdentificacionValida, PasswordHashValido);

        Assert.False(resultado.IsError);
        Assert.Equal("John Doe", resultado.Value.Nombre);
    }

    [Fact]
    public void Crear_DebeTrimearIdentificacion()
    {
        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, "  12345678  ", PasswordHashValido);

        Assert.False(resultado.IsError);
        Assert.Equal("12345678", resultado.Value.Identificacion);
    }

    [Fact]
    public void Crear_DebeTrimearCorreo_CuandoSeProvee()
    {
        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido, correo: "  jdoe@example.com  ");

        Assert.False(resultado.IsError);
        Assert.Equal("jdoe@example.com", resultado.Value.Correo);
    }

    [Fact]
    public void Crear_DebeTrimearTelefono_CuandoSeProvee()
    {
        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido, telefono: "  8888-1234  ");

        Assert.False(resultado.IsError);
        Assert.Equal("8888-1234", resultado.Value.Telefono);
    }

    // ──────────────────────────────────────────────
    // Validaciones — NombreUsuario
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoNombreUsuarioEsVacioOEspacios(string nombreUsuario)
    {
        var resultado = Usuario.Crear(nombreUsuario, NombreValido, IdentificacionValida, PasswordHashValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreUsuarioRequerido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoNombreUsuarioExcedeLongitudMaxima()
    {
        var nombreLargo = new string('a', Usuario.NombreUsuarioMaxLength + 1);

        var resultado = Usuario.Crear(nombreLargo, NombreValido, IdentificacionValida, PasswordHashValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreUsuarioExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebePermitir_CuandoNombreUsuarioEsExactamenteLongitudMaxima()
    {
        var nombreExacto = new string('a', Usuario.NombreUsuarioMaxLength);

        var resultado = Usuario.Crear(nombreExacto, NombreValido, IdentificacionValida, PasswordHashValido);

        Assert.False(resultado.IsError);
    }

    // ──────────────────────────────────────────────
    // Validaciones — Nombre
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoNombreEsVacioOEspacios(string nombre)
    {
        var resultado = Usuario.Crear(NombreUsuarioValido, nombre, IdentificacionValida, PasswordHashValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreRequerido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoNombreExcedeLongitudMaxima()
    {
        var nombreLargo = new string('a', Usuario.NombreMaxLength + 1);

        var resultado = Usuario.Crear(NombreUsuarioValido, nombreLargo, IdentificacionValida, PasswordHashValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreExcedeLongitud.Code);
    }

    // ──────────────────────────────────────────────
    // Validaciones — Identificacion
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebePermitir_IdentificacionVaciaOEspacios(string identificacion)
    {
        // La identificación es opcional al crear (alta rápida de usuarios POS).
        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, identificacion, PasswordHashValido);

        Assert.False(resultado.IsError);
        Assert.Equal(string.Empty, resultado.Value.Identificacion);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoIdentificacionExcedeLongitudMaxima()
    {
        var idLarga = new string('9', Usuario.IdentificacionMaxLength + 1);

        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, idLarga, PasswordHashValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.IdentificacionExcedeLongitud.Code);
    }

    // ──────────────────────────────────────────────
    // Validaciones — PasswordHash
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoPasswordHashEsVacioOEspacios(string passwordHash)
    {
        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, passwordHash);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PasswordRequerido.Code);
    }

    // ──────────────────────────────────────────────
    // Validaciones — Correo (opcional)
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarError_CuandoCorreoExcedeLongitudMaxima()
    {
        var correoLargo = new string('a', Usuario.CorreoMaxLength) + "@x.com";

        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido, correo: correoLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.CorreoExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebePermitirCorreoNulo()
    {
        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido, correo: null);

        Assert.False(resultado.IsError);
        Assert.Null(resultado.Value.Correo);
    }

    // ──────────────────────────────────────────────
    // Validaciones — Telefono (opcional)
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarError_CuandoTelefonoExcedeLongitudMaxima()
    {
        var telefonoLargo = new string('1', Usuario.TelefonoMaxLength + 1);

        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido, telefono: telefonoLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.TelefonoExcedeLongitud.Code);
    }

    [Fact]
    public void CambiarPassword_DebeActualizarPasswordHash_CuandoValorEsValido()
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;

        var resultado = usuario.CambiarPassword("nuevo_hash");

        Assert.False(resultado.IsError);
        Assert.Equal("nuevo_hash", usuario.PasswordHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CambiarPassword_DebeRetornarError_CuandoPasswordHashEsInvalido(string passwordHash)
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;

        var resultado = usuario.CambiarPassword(passwordHash);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PasswordRequerido.Code);
        Assert.Equal(PasswordHashValido, usuario.PasswordHash);
    }

    [Fact]
    public void Crear_DebePermitirTelefonoNulo()
    {
        var resultado = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido, telefono: null);

        Assert.False(resultado.IsError);
        Assert.Null(resultado.Value.Telefono);
    }

    // ──────────────────────────────────────────────
    // Acumulación de múltiples errores
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarTodosLosErrores_CuandoMultiplesCamposInvalidos()
    {
        var resultado = Usuario.Crear(string.Empty, string.Empty, string.Empty, string.Empty);

        Assert.True(resultado.IsError);
        Assert.True(resultado.Errors.Count >= 3);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreUsuarioRequerido.Code);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreRequerido.Code);
        // Identificación es opcional al crear: no debe acumular error por estar vacía.
        Assert.DoesNotContain(resultado.Errors, e => e.Code == UsuarioErrors.IdentificacionRequerida.Code);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PasswordRequerido.Code);
    }

    // ──────────────────────────────────────────────
    // Actualizar
    // ──────────────────────────────────────────────

    [Fact]
    public void Actualizar_DebeModificarCampos_CuandoDatosValidos()
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;

        var resultado = usuario.Actualizar("  nuevo  ", "  Nuevo Nombre  ", "  99999  ", "  a@b.com  ", "  123  ");

        Assert.False(resultado.IsError);
        Assert.Equal("nuevo", usuario.NombreUsuario);
        Assert.Equal("Nuevo Nombre", usuario.Nombre);
        Assert.Equal("99999", usuario.Identificacion);
        Assert.Equal("a@b.com", usuario.Correo);
        Assert.Equal("123", usuario.Telefono);
    }

    [Fact]
    public void Actualizar_NoDebeModificarPasswordHash()
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;

        usuario.Actualizar("otro", "Otro", "1", null, null);

        Assert.Equal(PasswordHashValido, usuario.PasswordHash);
    }

    [Fact]
    public void Actualizar_DebeRetornarErrores_CuandoMultiplesCamposInvalidos()
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;

        var resultado = usuario.Actualizar(string.Empty, string.Empty, string.Empty);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreUsuarioRequerido.Code);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreRequerido.Code);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.IdentificacionRequerida.Code);
    }

    [Fact]
    public void Actualizar_DebeRetornarError_CuandoLongitudExcedida()
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;

        var resultado = usuario.Actualizar(
            new string('x', Usuario.NombreUsuarioMaxLength + 1),
            NombreValido,
            IdentificacionValida);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreUsuarioExcedeLongitud.Code);
    }

    // ──────────────────────────────────────────────
    // PIN — EstablecerPin y TienePin
    // ──────────────────────────────────────────────

    [Fact]
    public void TienePin_EsFalse_CuandoUsuarioRecienCreado()
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;

        Assert.False(usuario.TienePin);
        Assert.Null(usuario.PinHash);
    }

    [Fact]
    public void EstablecerPin_SetaElHash_CuandoSeInvoca()
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;
        const string pinHash = "$2a$12$hash_pin_bcrypt_ejemplo";

        usuario.EstablecerPin(pinHash);

        Assert.Equal(pinHash, usuario.PinHash);
    }

    [Fact]
    public void TienePin_EsTrue_DespuesDeEstablecerPin()
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;

        usuario.EstablecerPin("$2a$12$algún_hash");

        Assert.True(usuario.TienePin);
    }

    [Fact]
    public void EstablecerPin_SobreescribeElHash_CuandoYaTenia()
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;
        usuario.EstablecerPin("hash_original");

        usuario.EstablecerPin("hash_nuevo");

        Assert.Equal("hash_nuevo", usuario.PinHash);
        Assert.True(usuario.TienePin);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EstablecerPin_DebeRetornarError_CuandoPinHashEsVacioOEspacios(string pinHash)
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;

        var resultado = usuario.EstablecerPin(pinHash);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PinRequerido.Code);
        Assert.Null(usuario.PinHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EstablecerPin_NoMutaPinHash_CuandoHashInvalidoYaHabiaPin(string pinHash)
    {
        var usuario = Usuario.Crear(NombreUsuarioValido, NombreValido, IdentificacionValida, PasswordHashValido).Value;
        const string pinHashOriginal = "$2a$12$hash_pin_original";
        usuario.EstablecerPin(pinHashOriginal);

        var resultado = usuario.EstablecerPin(pinHash);

        Assert.True(resultado.IsError);
        Assert.Equal(pinHashOriginal, usuario.PinHash);
    }

}
