using ErrorOr;
using PuntoVenta.Application.Commands.Backup.GenerarBackup;
using PuntoVenta.Application.Commands.Backup.ValidarBackup;
using PuntoVenta.Application.Common.Errors;
using PuntoVenta.Application.DTOs.Backup;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.UnitTests.Application.Backup;

public class BackupHandlerTests
{
    // ──────────────────────────────────────────────
    // GenerarBackupHandler
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GenerarBackup_DebeRetornarError_CuandoPinIncorrecto()
    {
        var usuarioId = Guid.NewGuid();
        var backupService = new FakeBackupService();
        var handler = new GenerarBackupHandler(
            new FakeUsuarioActual(usuarioId),
            new FakePinValidator(UsuarioErrors.PinIncorrecto),
            backupService);

        var resultado = await handler.Handle(
            new GenerarBackupCommand("000000", "/tmp/backup.db"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PinIncorrecto.Code);
        Assert.False(backupService.GenerarLlamado);
    }

    [Fact]
    public async Task GenerarBackup_DebeRetornarDto_CuandoPinCorrecto()
    {
        var usuarioId = Guid.NewGuid();
        var backupService = new FakeBackupService();
        var handler = new GenerarBackupHandler(
            new FakeUsuarioActual(usuarioId),
            new FakePinValidator(Result.Success),
            backupService);

        var resultado = await handler.Handle(
            new GenerarBackupCommand("123456", "/tmp/backup.db"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.True(backupService.GenerarLlamado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ruta/relativa/backup.db")]
    public async Task GenerarBackup_DebeRetornarError_CuandoRutaInvalidaORelativa(string ruta)
    {
        var usuarioId = Guid.NewGuid();
        var backupService = new FakeBackupService();
        var handler = new GenerarBackupHandler(
            new FakeUsuarioActual(usuarioId),
            new FakePinValidator(Result.Success),
            backupService);

        var resultado = await handler.Handle(
            new GenerarBackupCommand("123456", ruta),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == BackupErrors.RutaInvalida.Code);
        Assert.False(backupService.GenerarLlamado);
    }

    // ──────────────────────────────────────────────
    // ValidarBackupHandler
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ruta/relativa/backup.db")]
    public async Task ValidarBackup_DebeRetornarError_CuandoRutaInvalidaORelativa(string ruta)
    {
        var usuarioId = Guid.NewGuid();
        var backupService = new FakeBackupService();
        var handler = new ValidarBackupHandler(
            new FakeUsuarioActual(usuarioId),
            new FakePinValidator(Result.Success),
            backupService,
            new FakeRestoreTokenService());

        var resultado = await handler.Handle(
            new ValidarBackupCommand(ruta, "123456"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == BackupErrors.RutaInvalida.Code);
        Assert.False(backupService.ValidarLlamado);
    }

    [Fact]
    public async Task ValidarBackup_DebeRetornarError_CuandoPinNoConfigurado()
    {
        var usuarioId = Guid.NewGuid();
        var backupService = new FakeBackupService();
        var handler = new ValidarBackupHandler(
            new FakeUsuarioActual(usuarioId),
            new FakePinValidator(UsuarioErrors.PinNoConfigurado),
            backupService,
            new FakeRestoreTokenService());

        var resultado = await handler.Handle(
            new ValidarBackupCommand("/tmp/backup.db", "123456"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PinNoConfigurado.Code);
        Assert.False(backupService.ValidarLlamado);
    }

    [Fact]
    public async Task ValidarBackup_DebeRetornarVersionIncompatible_CuandoBackupService()
    {
        var usuarioId = Guid.NewGuid();
        var backupService = new FakeBackupService(
            validarResultado: BackupErrors.VersionIncompatible);
        var handler = new ValidarBackupHandler(
            new FakeUsuarioActual(usuarioId),
            new FakePinValidator(Result.Success),
            backupService,
            new FakeRestoreTokenService());

        var resultado = await handler.Handle(
            new ValidarBackupCommand("/tmp/backup.db", "123456"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == BackupErrors.VersionIncompatible.Code);
        Assert.True(backupService.ValidarLlamado);
    }

    [Fact]
    public async Task ValidarBackup_DebeRetornarDto_CuandoCompatible()
    {
        var usuarioId = Guid.NewGuid();
        var dto = new BackupValidacionDto { EsCompatible = true, VersionBackup = "v1", VersionApp = "v1" };
        var backupService = new FakeBackupService(validarResultado: dto);
        var handler = new ValidarBackupHandler(
            new FakeUsuarioActual(usuarioId),
            new FakePinValidator(Result.Success),
            backupService,
            new FakeRestoreTokenService());

        var resultado = await handler.Handle(
            new ValidarBackupCommand("/tmp/backup.db", "123456"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.True(resultado.Value.EsCompatible);
        // Cuando es compatible, se acuña el token de capacidad.
        Assert.False(string.IsNullOrEmpty(resultado.Value.TokenRestauracion));
    }

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

    private sealed class FakeUsuarioActual : IUsuarioActual
    {
        public FakeUsuarioActual(Guid id) => UsuarioId = id;
        public Guid UsuarioId { get; }
        public string NombreUsuario => "admin";
        public bool RequiereCambioPassword => false;
    }

    private sealed class FakePinValidator : IPinValidator
    {
        private readonly ErrorOr<Success> _resultado;
        public FakePinValidator(ErrorOr<Success> resultado) => _resultado = resultado;
        public ValueTask<ErrorOr<Success>> ValidarAsync(Guid usuarioId, string pin, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_resultado);
    }

    private sealed class FakeRestoreTokenService : IRestoreTokenService
    {
        public string Generar(string rutaCanonica, string huella) => "TOKEN-FAKE";
        public bool Consumir(string token, string rutaCanonica, string huella) => true;
    }

    private sealed class FakeBackupService : IBackupService
    {
        private readonly ErrorOr<BackupValidacionDto> _validarResultado;
        public bool GenerarLlamado { get; private set; }
        public bool ValidarLlamado { get; private set; }

        public FakeBackupService(ErrorOr<BackupValidacionDto>? validarResultado = null)
        {
            _validarResultado = validarResultado ?? new BackupValidacionDto
            {
                EsCompatible = true,
                VersionBackup = "20260618_v1",
                VersionApp = "20260618_v1",
            };
        }

        public Task<ErrorOr<BackupGeneradoDto>> GenerarAsync(string rutaDestino, CancellationToken cancellationToken = default)
        {
            GenerarLlamado = true;
            return Task.FromResult<ErrorOr<BackupGeneradoDto>>(new BackupGeneradoDto
            {
                RutaArchivo = rutaDestino,
                VersionEsquema = "20260618_v1",
                FechaUtc = DateTime.UtcNow,
            });
        }

        public Task<ErrorOr<BackupValidacionDto>> ValidarAsync(string rutaBackup, CancellationToken cancellationToken = default)
        {
            ValidarLlamado = true;
            return Task.FromResult(_validarResultado);
        }

        public Task<string> ObtenerVersionEsquemaAsync(CancellationToken cancellationToken = default)
            => Task.FromResult("20260618_v1");

        public Task<string> CalcularHuellaAsync(string ruta, CancellationToken cancellationToken = default)
            => Task.FromResult("HUELLA-FAKE");
    }
}
