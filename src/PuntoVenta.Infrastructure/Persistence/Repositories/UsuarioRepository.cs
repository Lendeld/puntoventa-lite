using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(ApplicationDbContext context) : Repository<Usuario>(context), IUsuarioRepository
{
    public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(u => u.NombreUsuario == nombreUsuario, cancellationToken);

    public async Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(u => u.Identificacion == identificacion, cancellationToken);

    public async Task<bool> ExisteNombreUsuarioExcluyendoAsync(string nombreUsuario, Guid excludeId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(u => u.NombreUsuario == nombreUsuario && u.Id != excludeId, cancellationToken);

    public async Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(u => u.Identificacion == identificacion && u.Id != excludeId, cancellationToken);

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, cancellationToken);

    public async Task<Usuario?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Usuario> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (activo.HasValue)
            query = query.Where(u => u.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(filtroDinamico))
        {
            var term = filtroDinamico.Trim().ToLower();
            query = query.Where(u =>
                u.Nombre.ToLower().Contains(term) ||
                u.NombreUsuario.ToLower().Contains(term) ||
                u.Identificacion.ToLower().Contains(term) ||
                (u.Correo != null && u.Correo.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.FechaCreacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
