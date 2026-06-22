using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Security;

public sealed class PermisoCache(IMemoryCache cache, IServiceScopeFactory scopeFactory) : IPermisoCache, ISingletonService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);
    private const string Prefijo = "permisos:";

    private readonly IMemoryCache _cache = cache;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private CancellationTokenSource _resetCts = new();

    public async Task<IReadOnlyList<string>> ObtenerPermisosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var clave = $"{Prefijo}{usuarioId}";

        if (_cache.TryGetValue(clave, out IReadOnlyList<string>? permisos) && permisos is not null)
        {
            return permisos;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUsuarioPermisoRepository>();
        permisos = await repo.ObtenerClavesPermisosAsync(usuarioId, cancellationToken);

        var opciones = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(Ttl)
            .SetSize(1)
            .AddExpirationToken(new CancellationChangeToken(_resetCts.Token));

        _cache.Set(clave, permisos, opciones);
        return permisos;
    }

    public void Invalidar(Guid usuarioId)
        => InvalidarTodos();

    public void InvalidarTodos()
    {
        var anterior = Interlocked.Exchange(ref _resetCts, new CancellationTokenSource());
        anterior.Cancel();
        anterior.Dispose();
    }
}
