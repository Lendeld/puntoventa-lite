using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.Infrastructure.Services;

public sealed class ApartadosVencimientoService(IServiceScopeFactory scopeFactory, ILogger<ApartadosVencimientoService> logger) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<ApartadosVencimientoService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarVencimientosAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar apartados vencidos.");
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }

    private async Task ProcesarVencimientosAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IDocumentoVentaRepository>();
        var fechaActual = scope.ServiceProvider.GetRequiredService<IFechaActual>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var ahora = fechaActual.AhoraUtc;
        var vencidos = await repo.ObtenerApartadosReservadosVencidosAsync(ahora, cancellationToken);
        if (vencidos.Count == 0)
        {
            return;
        }

        var marcados = 0;
        foreach (var apartado in vencidos)
        {
            var resultado = apartado.MarcarVencido(ahora);
            if (resultado.IsError)
            {
                continue;
            }

            marcados++;
        }

        if (marcados > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Se marcaron {Cantidad} apartados como vencidos.", marcados);
        }
    }
}
