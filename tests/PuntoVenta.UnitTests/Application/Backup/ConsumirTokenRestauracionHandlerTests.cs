using ErrorOr;
using PuntoVenta.Application.Commands.Backup.ConsumirTokenRestauracion;
using PuntoVenta.Application.Common.Errors;
using PuntoVenta.Application.DTOs.Backup;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.UnitTests.Application.Backup;

public class ConsumirTokenRestauracionHandlerTests
{
    [Fact]
    public async Task Consumir_DevuelveSuccess_CuandoServicioAcepta()
    {
        var tokenService = new FakeRestoreTokenService(consumirResultado: true);
        var handler = new ConsumirTokenRestauracionHandler(tokenService, new FakeBackupService());

        var resultado = await handler.Handle(
            new ConsumirTokenRestauracionCommand("TOKEN", "/datos/backup.db"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.True(tokenService.ConsumirLlamado);
        // El handler ata el consumo a la huella recomputada del archivo.
        Assert.Equal("HUELLA-FAKE", tokenService.UltimaHuella);
    }

    [Fact]
    public async Task Consumir_DevuelveTokenInvalido_CuandoServicioRechaza()
    {
        var tokenService = new FakeRestoreTokenService(consumirResultado: false);
        var handler = new ConsumirTokenRestauracionHandler(tokenService, new FakeBackupService());

        var resultado = await handler.Handle(
            new ConsumirTokenRestauracionCommand("TOKEN", "/datos/backup.db"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == BackupErrors.TokenInvalido.Code);
    }

    [Theory]
    [InlineData("", "/datos/backup.db")]
    [InlineData("TOKEN", "")]
    [InlineData("TOKEN", "ruta/relativa.db")]
    public async Task Consumir_DevuelveTokenInvalido_CuandoEntradaInvalida(string token, string ruta)
    {
        var tokenService = new FakeRestoreTokenService(consumirResultado: true);
        var handler = new ConsumirTokenRestauracionHandler(tokenService, new FakeBackupService());

        var resultado = await handler.Handle(
            new ConsumirTokenRestauracionCommand(token, ruta),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == BackupErrors.TokenInvalido.Code);
        // No debe llegar al servicio cuando la entrada es inválida.
        Assert.False(tokenService.ConsumirLlamado);
    }

    private sealed class FakeRestoreTokenService : IRestoreTokenService
    {
        private readonly bool _consumirResultado;
        public bool ConsumirLlamado { get; private set; }
        public string? UltimaHuella { get; private set; }

        public FakeRestoreTokenService(bool consumirResultado) => _consumirResultado = consumirResultado;

        public string Generar(string rutaCanonica, string huella) => "TOKEN";

        public bool Consumir(string token, string rutaCanonica, string huella)
        {
            ConsumirLlamado = true;
            UltimaHuella = huella;
            return _consumirResultado;
        }
    }

    private sealed class FakeBackupService : IBackupService
    {
        public Task<ErrorOr<BackupGeneradoDto>> GenerarAsync(string rutaDestino, CancellationToken cancellationToken = default)
            => Task.FromResult<ErrorOr<BackupGeneradoDto>>(new BackupGeneradoDto());

        public Task<ErrorOr<BackupValidacionDto>> ValidarAsync(string rutaBackup, CancellationToken cancellationToken = default)
            => Task.FromResult<ErrorOr<BackupValidacionDto>>(new BackupValidacionDto());

        public Task<string> ObtenerVersionEsquemaAsync(CancellationToken cancellationToken = default)
            => Task.FromResult("v1");

        public Task<string> CalcularHuellaAsync(string ruta, CancellationToken cancellationToken = default)
            => Task.FromResult("HUELLA-FAKE");
    }
}
