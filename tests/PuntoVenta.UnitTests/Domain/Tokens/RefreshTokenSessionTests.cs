using PuntoVenta.Domain.Entities.Tokens;

namespace PuntoVenta.UnitTests.Domain.Tokens;

public class RefreshTokenSessionTests
{
    private static readonly Guid UsuarioId = Guid.NewGuid();
    private static readonly DateTime Ahora = DateTime.UtcNow;

    private static RefreshTokenSession CrearSesion(
        DateTime? expiracion = null,
        string? ip = null)
        => RefreshTokenSession.Crear(
            UsuarioId,
            "hash123",
            Ahora,
            expiracion ?? Ahora.AddDays(7),
            ip);

    // ──────────────────────────────────────────────
    // EstaActivo
    // ──────────────────────────────────────────────

    [Fact]
    public void EstaActivo_DebeRetornarTrue_CuandoNoRevocadaYNoExpirada()
    {
        var sesion = CrearSesion(expiracion: Ahora.AddDays(7));

        Assert.True(sesion.EstaActivo(Ahora));
    }

    [Fact]
    public void EstaActivo_DebeRetornarFalse_CuandoExpirada()
    {
        var sesion = CrearSesion(expiracion: Ahora.AddDays(-1));

        Assert.False(sesion.EstaActivo(Ahora));
    }

    [Fact]
    public void EstaActivo_DebeRetornarFalse_CuandoRevocada()
    {
        var sesion = CrearSesion();
        sesion.Revocar(Ahora);

        Assert.False(sesion.EstaActivo(Ahora));
    }

    // ──────────────────────────────────────────────
    // Revocar
    // ──────────────────────────────────────────────

    [Fact]
    public void Revocar_DebeAsignarRevocadoEnUtc()
    {
        var sesion = CrearSesion();

        sesion.Revocar(Ahora);

        Assert.Equal(Ahora, sesion.RevocadoEnUtc);
    }

    [Fact]
    public void Revocar_DebeAsignarReemplazadoPor_CuandoSeProvee()
    {
        var sesion = CrearSesion();

        sesion.Revocar(Ahora, "nuevo_hash");

        Assert.Equal("nuevo_hash", sesion.ReemplazadoPorTokenHash);
    }

    [Fact]
    public void Revocar_NoDebeRevocarDosVeces()
    {
        var sesion = CrearSesion();
        var primeraRevocacion = Ahora;
        sesion.Revocar(primeraRevocacion);

        var segundaRevocacion = Ahora.AddMinutes(5);
        sesion.Revocar(segundaRevocacion);

        Assert.Equal(primeraRevocacion, sesion.RevocadoEnUtc);
    }

    // ──────────────────────────────────────────────
    // FueRotado
    // ──────────────────────────────────────────────

    [Fact]
    public void FueRotado_DebeRetornarFalse_CuandoNoTieneReemplazo()
    {
        var sesion = CrearSesion();

        Assert.False(sesion.FueRotado);
    }

    [Fact]
    public void FueRotado_DebeRetornarTrue_CuandoTieneReemplazo()
    {
        var sesion = CrearSesion();
        sesion.Revocar(Ahora, "nuevo_hash");

        Assert.True(sesion.FueRotado);
    }

    // ──────────────────────────────────────────────
    // MarcarUso
    // ──────────────────────────────────────────────

    [Fact]
    public void MarcarUso_DebeActualizarUltimoUso()
    {
        var sesion = CrearSesion();

        sesion.MarcarUso(Ahora, "127.0.0.1");

        Assert.Equal(Ahora, sesion.UltimoUsoEnUtc);
        Assert.Equal("127.0.0.1", sesion.UltimoUsoPorIp);
    }

    // ──────────────────────────────────────────────
    // Crear
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeAsignarCamposCorrectamente()
    {
        var expiracion = Ahora.AddDays(7);

        var sesion = RefreshTokenSession.Crear(UsuarioId, "hash_tok", Ahora, expiracion, "10.0.0.1");

        Assert.Equal(UsuarioId, sesion.UsuarioId);
        Assert.Equal("hash_tok", sesion.TokenHash);
        Assert.Equal(Ahora, sesion.CreadoEnUtc);
        Assert.Equal(expiracion, sesion.ExpiracionUtc);
        Assert.Equal("10.0.0.1", sesion.CreadoPorIp);
        Assert.Null(sesion.RevocadoEnUtc);
        Assert.Null(sesion.ReemplazadoPorTokenHash);
    }
}
