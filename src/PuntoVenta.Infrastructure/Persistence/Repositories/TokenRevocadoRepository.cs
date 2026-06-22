using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Tokens;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class TokenRevocadoRepository(ApplicationDbContext context, IMemoryCache cache) : ITokenRevocadoRepository, IScopedService
{
    private static readonly TimeSpan TtlRevocado = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan TtlValido = TimeSpan.FromSeconds(60);

    private readonly ApplicationDbContext _context = context;
    private readonly IMemoryCache _cache = cache;

    public async Task RevocarAsync(TokenRevocado token, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO "TokensRevocados" ("Id", "Jti", "FechaExpiracion", "FechaRevocacion")
            VALUES ({0}, {1}, {2}, {3})
            ON CONFLICT ("Jti") DO NOTHING
            """;

        await _context.Database.ExecuteSqlRawAsync(
            sql,
            [token.Id, token.Jti, token.FechaExpiracion, token.FechaRevocacion],
            cancellationToken);

        _cache.Set(CacheKey(token.Jti), true, new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TtlRevocado)
            .SetSize(1));
    }

    public async Task<bool> EstaRevocadoAsync(string jti, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKey(jti);
        if (_cache.TryGetValue(cacheKey, out bool cached))
        {
            return cached;
        }

        var revocado = await _context.TokensRevocados
            .AnyAsync(t => t.Jti == jti && t.FechaExpiracion > DateTime.UtcNow, cancellationToken);

        var ttl = revocado ? TtlRevocado : TtlValido;
        _cache.Set(cacheKey, revocado, new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(ttl)
            .SetSize(1));

        return revocado;
    }

    public async Task EliminarExpiradosAsync(CancellationToken cancellationToken = default)
    {
        await _context.TokensRevocados
            .Where(t => t.FechaExpiracion <= DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static string CacheKey(string jti) => $"token_revocado:{jti}";
}
