using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Services;

public sealed class TokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<TokenCleanupService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Intervalo, stoppingToken);

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var tokenRevocadoRepo = scope.ServiceProvider.GetRequiredService<ITokenRevocadoRepository>();
                var refreshSessionRepo = scope.ServiceProvider.GetRequiredService<IRefreshTokenSessionRepository>();
                await tokenRevocadoRepo.EliminarExpiradosAsync(stoppingToken);
                await refreshSessionRepo.EliminarExpiradosAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar tokens expirados.");
            }
        }
    }
}
