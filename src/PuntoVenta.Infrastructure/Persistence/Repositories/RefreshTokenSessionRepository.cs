using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Tokens;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenSessionRepository(ApplicationDbContext context) : IRefreshTokenSessionRepository, IScopedService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<RefreshTokenSession?> ObtenerPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokenSessions
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<RefreshTokenSession?> ObtenerReemplazoPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokenSessions
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public async Task AgregarAsync(RefreshTokenSession session, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokenSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(RefreshTokenSession session, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokenSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevocarSesionesUsuarioAsync(
        Guid usuarioId,
        DateTime revocadoEnUtc,
        CancellationToken cancellationToken = default)
    {
        var sesiones = await _context.RefreshTokenSessions
            .AsTracking()
            .Where(x => x.UsuarioId == usuarioId && x.RevocadoEnUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var sesion in sesiones)
        {
            sesion.Revocar(revocadoEnUtc);
        }

        if (sesiones.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task EliminarExpiradosAsync(CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokenSessions
            .Where(x => x.ExpiracionUtc <= DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
