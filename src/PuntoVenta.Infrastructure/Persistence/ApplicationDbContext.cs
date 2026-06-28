using Mediator;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Common.Events;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Domain.Entities.Cajas;
using PuntoVenta.Domain.Entities.Categorias;
using PuntoVenta.Domain.Entities.Secuencias;
using PuntoVenta.Domain.Entities.Clientes;
using PuntoVenta.Domain.Entities.MovimientosStock;
using PuntoVenta.Domain.Entities.CodigosImpuesto;
using PuntoVenta.Domain.Entities.CondicionesVenta;
using PuntoVenta.Domain.Entities.MediosPago;
using PuntoVenta.Domain.Entities.Impresion;
using PuntoVenta.Domain.Entities.Negocios;
using PuntoVenta.Domain.Entities.Paginas;
using PuntoVenta.Domain.Entities.Permisos;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.Roles;
using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;
using PuntoVenta.Domain.Entities.TiposIdentificacion;
using PuntoVenta.Domain.Entities.Tokens;
using PuntoVenta.Domain.Entities.Usuarios;
using PuntoVenta.Domain.Entities.Proveedores;
using PuntoVenta.Domain.Entities.Vendedores;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUsuarioActual usuarioActual, IFechaActual fechaActual, IPublisher publisher) : DbContext(options), IDataProtectionKeyContext
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IPublisher _publisher = publisher;

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TokenRevocado> TokensRevocados => Set<TokenRevocado>();
    public DbSet<RefreshTokenSession> RefreshTokenSessions => Set<RefreshTokenSession>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<Pagina> Paginas => Set<Pagina>();
    public DbSet<PaginaPermiso> PaginaPermisos => Set<PaginaPermiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<Negocio> Negocios => Set<Negocio>();
    public DbSet<TipoIdentificacion> TiposIdentificacion => Set<TipoIdentificacion>();
    public DbSet<CondicionVenta> CondicionesVenta => Set<CondicionVenta>();
    public DbSet<MedioPago> MediosPago => Set<MedioPago>();
    public DbSet<CodigoImpuesto> CodigosImpuesto => Set<CodigoImpuesto>();
    public DbSet<TarifaIvaImpuesto> TarifasIvaImpuesto => Set<TarifaIvaImpuesto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Vendedor> Vendedores => Set<Vendedor>();
    public DbSet<DocumentoVenta> DocumentosVenta => Set<DocumentoVenta>();
    public DbSet<DocumentoVentaLinea> DocumentosVentaLineas => Set<DocumentoVentaLinea>();
    public DbSet<DocumentoVentaPago> DocumentosVentaPagos => Set<DocumentoVentaPago>();
    public DbSet<DocumentoVentaReferencia> DocumentosVentaReferencias => Set<DocumentoVentaReferencia>();
    public DbSet<DocumentoVentaEvento> DocumentosVentaEventos => Set<DocumentoVentaEvento>();
    public DbSet<NegocioTicketConfig> NegocioTicketConfigs => Set<NegocioTicketConfig>();
    public DbSet<Caja> Cajas => Set<Caja>();
    public DbSet<Secuencia> Secuencias => Set<Secuencia>();
    public DbSet<PerfilImpresoraTicket> PerfilesImpresoraTicket => Set<PerfilImpresoraTicket>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = _fechaActual.AhoraUtc;
        Guid? usuarioId = null;
        try { usuarioId = _usuarioActual.UsuarioId; } catch { /* contexto sin autenticación (seeder, jobs) */ }

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.FechaCreacion = now;
                entry.Entity.UsuarioCreacionId = usuarioId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.FechaModificacion = now;
                entry.Entity.UsuarioModificacionId = usuarioId;
                entry.Property(e => e.FechaCreacion).IsModified = false;
                entry.Property(e => e.UsuarioCreacionId).IsModified = false;
            }
        }

        var resultado = await base.SaveChangesAsync(cancellationToken);

        // Drenado de domain events: POST-SaveChanges, NO post-commit de transacción.
        // Flujos como CrearFactura envuelven los saves en una transacción explícita (IUnitOfWork)
        // y hacen CommitAsync después → estos eventos se publican ANTES del commit.
        // Hoy es inofensivo (cero handlers → Publish es no-op). Si se registran handlers reales,
        // conviene mover la publicación a post-commit (o a un outbox durable).
        var entidades = ChangeTracker
            .Entries<BaseAuditableEntity>()
            .Where(e => e.Entity.EventosDominio.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var eventos = entidades
            .SelectMany(e => e.EventosDominio)
            .ToList();

        foreach (var entidad in entidades)
        {
            entidad.LimpiarEventos();
        }

        foreach (var evento in eventos)
        {
            await _publisher.Publish(evento, cancellationToken);
        }

        return resultado;
    }
}
