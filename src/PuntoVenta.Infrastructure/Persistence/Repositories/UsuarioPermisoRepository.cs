using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.DTOs.Paginas;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class UsuarioPermisoRepository(ApplicationDbContext context) : IUsuarioPermisoRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IReadOnlyList<string>> ObtenerClavesPermisosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        if (usuario is null) return [];

        if (usuario.RolId is null) return [];

        var esPrincipal = await _context.Roles
            .AnyAsync(r => r.Id == usuario.RolId.Value && r.IsPrincipal && r.Activo, cancellationToken);

        if (esPrincipal)
        {
            return await _context.Permisos
                .Where(p => p.Activo)
                .Select(p => p.Clave)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        return await _context.RolPermisos
            .Where(rp => rp.RolId == usuario.RolId.Value && rp.Permiso!.Activo)
            .Select(rp => rp.Permiso!.Clave)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaginaMenuDto>> ObtenerPaginasMenuAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        if (usuario is null || usuario.RolId is null) return [];

        var todasLasPaginas = await _context.Paginas
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .Select(p => new PaginaMenuDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Ruta = p.Ruta,
                Icono = p.Icono,
                Orden = p.Orden,
                PaginaPadreId = p.PaginaPadreId
            })
            .ToListAsync(cancellationToken);

        var esPrincipal = await _context.Roles
            .AnyAsync(r => r.Id == usuario.RolId.Value && r.IsPrincipal && r.Activo, cancellationToken);

        if (esPrincipal) return todasLasPaginas;

        var permisosIds = await _context.RolPermisos
            .Where(rp => rp.RolId == usuario.RolId.Value && rp.Permiso!.Activo)
            .Select(rp => rp.PermisoId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context.Paginas
            .AsNoTracking()
            .Where(p => p.Activo && p.PaginaPermisos.Any(pp => permisosIds.Contains(pp.PermisoId)))
            .OrderBy(p => p.Orden)
            .Select(p => new PaginaMenuDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Ruta = p.Ruta,
                Icono = p.Icono,
                Orden = p.Orden,
                PaginaPadreId = p.PaginaPadreId
            })
            .ToListAsync(cancellationToken);
    }
}
