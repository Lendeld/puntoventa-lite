using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.UnitTests.Domain.Clientes;

public class ClienteTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private const string NombreValido = "Juan Pérez";

    // ──────────────────────────────────────────────
    // Casos exitosos
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarCliente_CuandoSoloNombre()
    {
        var resultado = Cliente.Crear(NombreValido);

        Assert.False(resultado.IsError);
        var cliente = resultado.Value;
        Assert.Equal(NombreValido, cliente.Nombre);
        Assert.Equal(Cliente.NormalizarNombre(NombreValido), cliente.NombreNormalizado);
        Assert.Null(cliente.Identificacion);
        Assert.Null(cliente.Correo);
        Assert.Null(cliente.Telefono);
        Assert.Null(cliente.Observaciones);
        Assert.NotEqual(Guid.Empty, cliente.Id);
    }

    [Fact]
    public void Crear_DebeRetornarCliente_CuandoTodosLosCamposValidos()
    {
        var resultado = Cliente.Crear(NombreValido, "12345678", "juan@mail.com", "8888-0000", "cliente VIP");

        Assert.False(resultado.IsError);
        Assert.Equal("12345678", resultado.Value.Identificacion);
        Assert.Equal("juan@mail.com", resultado.Value.Correo);
        Assert.Equal("8888-0000", resultado.Value.Telefono);
        Assert.Equal("cliente VIP", resultado.Value.Observaciones);
    }

    // ──────────────────────────────────────────────
    // Trim de campos
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeTrimearNombre()
    {
        var resultado = Cliente.Crear("  Juan Pérez  ");

        Assert.False(resultado.IsError);
        Assert.Equal("Juan Pérez", resultado.Value.Nombre);
    }

    [Fact]
    public void Crear_DebeTrimearIdentificacion()
    {
        var resultado = Cliente.Crear(NombreValido, identificacion: "  12345678  ");

        Assert.False(resultado.IsError);
        Assert.Equal("12345678", resultado.Value.Identificacion);
    }

    [Fact]
    public void Crear_DebeTrimearCorreo()
    {
        var resultado = Cliente.Crear(NombreValido, correo: "  juan@mail.com  ");

        Assert.False(resultado.IsError);
        Assert.Equal("juan@mail.com", resultado.Value.Correo);
    }

    [Fact]
    public void Crear_DebeTrimearTelefono()
    {
        var resultado = Cliente.Crear(NombreValido, telefono: "  8888-0000  ");

        Assert.False(resultado.IsError);
        Assert.Equal("8888-0000", resultado.Value.Telefono);
    }

    // ──────────────────────────────────────────────
    // Validaciones — Nombre
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoNombreEsVacioOEspacios(string nombre)
    {
        var resultado = Cliente.Crear(nombre);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ClienteErrors.NombreRequerido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoNombreExcedeLongitudMaxima()
    {
        var nombreLargo = new string('a', Cliente.NombreMaxLength + 1);

        var resultado = Cliente.Crear(nombreLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ClienteErrors.NombreExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebePermitir_CuandoNombreEsExactamenteLongitudMaxima()
    {
        var nombreExacto = new string('a', Cliente.NombreMaxLength);

        var resultado = Cliente.Crear(nombreExacto);

        Assert.False(resultado.IsError);
    }

    // ──────────────────────────────────────────────
    // Validaciones — campos opcionales
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarError_CuandoIdentificacionExcedeLongitudMaxima()
    {
        var idLarga = new string('9', Cliente.IdentificacionMaxLength + 1);

        var resultado = Cliente.Crear(NombreValido, identificacion: idLarga);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ClienteErrors.IdentificacionExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoCorreoExcedeLongitudMaxima()
    {
        var correoLargo = new string('a', Cliente.CorreoMaxLength + 1);

        var resultado = Cliente.Crear(NombreValido, correo: correoLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ClienteErrors.CorreoExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoTelefonoExcedeLongitudMaxima()
    {
        var telLargo = new string('1', Cliente.TelefonoMaxLength + 1);

        var resultado = Cliente.Crear(NombreValido, telefono: telLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ClienteErrors.TelefonoExcedeLongitud.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoObservacionesExcedeLongitudMaxima()
    {
        var obsLargo = new string('x', Cliente.ObservacionesMaxLength + 1);

        var resultado = Cliente.Crear(NombreValido, observaciones: obsLargo);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ClienteErrors.ObservacionesExcedeLongitud.Code);
    }

    // ──────────────────────────────────────────────
    // NombreNormalizado
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeNormalizarNombreEnMayusculas()
    {
        var resultado = Cliente.Crear("juan pérez");

        Assert.False(resultado.IsError);
        Assert.Equal("JUAN PÉREZ", resultado.Value.NombreNormalizado);
    }

    // ──────────────────────────────────────────────
    // Actualizar
    // ──────────────────────────────────────────────

    [Fact]
    public void Actualizar_DebeModificarCampos_CuandoDatosValidos()
    {
        var cliente = Cliente.Crear(NombreValido).Value;

        var resultado = cliente.Actualizar("  Nuevo Nombre  ", "99", "nuevo@mail.com", "7777-0000", "obs");

        Assert.False(resultado.IsError);
        Assert.Equal("Nuevo Nombre", cliente.Nombre);
        Assert.Equal("99", cliente.Identificacion);
        Assert.Equal("nuevo@mail.com", cliente.Correo);
        Assert.Equal("7777-0000", cliente.Telefono);
        Assert.Equal("obs", cliente.Observaciones);
    }

    [Fact]
    public void Actualizar_DebeRetornarError_CuandoNombreVacio()
    {
        var cliente = Cliente.Crear(NombreValido).Value;

        var resultado = cliente.Actualizar(string.Empty);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ClienteErrors.NombreRequerido.Code);
    }
}
