using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class RolRepository(ApplicationDbContext context) : Repository<Rol>(context), IRolRepository
{
    public async Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(r => r.Nombre == nombre, cancellationToken);

    public async Task<bool> ExisteNombreExcluyendoAsync(string nombre, Guid excludeId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(r => r.Nombre == nombre && r.Id != excludeId, cancellationToken);

    public async Task<IReadOnlyList<Rol>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(r => r.Activo)
            .OrderBy(r => r.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PaginaPermisosRolTabDto>> ObtenerPaginasConPermisosAsync(
        CancellationToken cancellationToken = default)
        => await Context.Paginas
            .AsNoTracking()
            .Where(p => p.Activo && p.PaginaPermisos.Any(pp => pp.Permiso!.Activo))
            .OrderBy(p => p.Nombre)
            .Select(p => new PaginaPermisosRolTabDto
            {
                PaginaId = p.Id,
                Nombre = p.Nombre,
                Orden = p.Orden
            })
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Rol> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (activo.HasValue)
            query = query.Where(r => r.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(filtroDinamico))
        {
            var term = filtroDinamico.Trim().ToLower();
            query = query.Where(r =>
                r.Nombre.ToLower().Contains(term) ||
                (r.Descripcion != null && r.Descripcion.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.FechaCreacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<PermisosRolPorPaginaDto?> ObtenerPermisosAgrupadosPorPaginaAsync(
        Guid rolId,
        Guid paginaId,
        CancellationToken cancellationToken = default)
    {
        var rolExiste = await DbSet.AnyAsync(r => r.Id == rolId, cancellationToken);
        if (!rolExiste) return null;

        var rolPermisoIds = await Context.RolPermisos
            .Where(rp => rp.RolId == rolId)
            .Select(rp => rp.PermisoId)
            .ToListAsync(cancellationToken);

        return await Context.Paginas
            .AsNoTracking()
            .Where(p => p.Id == paginaId && p.Activo)
            .Select(p => new PermisosRolPorPaginaDto
            {
                PaginaId = p.Id,
                Nombre = p.Nombre,
                Permisos = p.PaginaPermisos
                    .Where(pp => pp.Permiso!.Activo)
                    .OrderBy(pp => pp.Permiso!.Descripcion)
                    .Select(pp => new PermisoPaginaDto
                    {
                        PermisoId = pp.PermisoId,
                        Clave = pp.Permiso!.Clave,
                        Descripcion = pp.Permiso!.Descripcion,
                        Asignado = rolPermisoIds.Contains(pp.PermisoId)
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task ActualizarPermisosAsync(
        Guid rolId,
        Guid paginaId,
        IReadOnlyList<Guid> permisosIds,
        CancellationToken cancellationToken = default)
    {
        var rol = await DbSet.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rolId, cancellationToken);
        if (rol is null) return;

        var permisoIdsDeLaPagina = await Context.PaginaPermisos
            .Where(pp => pp.PaginaId == paginaId && pp.Permiso!.Activo)
            .Select(pp => pp.PermisoId)
            .ToListAsync(cancellationToken);

        var existentes = await Context.RolPermisos
            .Where(rp => rp.RolId == rol.Id && permisoIdsDeLaPagina.Contains(rp.PermisoId))
            .ToListAsync(cancellationToken);

        Context.RolPermisos.RemoveRange(existentes);

        var nuevos = permisosIds
            .Where(id => permisoIdsDeLaPagina.Contains(id))
            .Select(id => RolPermiso.Crear(rol.Id, id));

        await Context.RolPermisos.AddRangeAsync(nuevos, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
