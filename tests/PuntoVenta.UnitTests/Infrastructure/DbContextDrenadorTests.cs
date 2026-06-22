using Mediator;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Common.Events;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Domain.Entities.Ventas.Eventos;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.UnitTests.Infrastructure;

/// <summary>
/// Verifica que ApplicationDbContext drene los eventos de dominio tras
/// SaveChangesAsync: los eventos se publican al IPublisher y quedan limpios.
///
/// Usa una entidad de test mínima (EntidadPrueba) sin FK dependencias,
/// configurada ad-hoc en el DbContext de test.
/// </summary>
public sealed class DbContextDrenadorTests : IAsyncDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly ApplicationDbContextTest _db;
    private readonly FakePublisher _publisher;

    public DbContextDrenadorTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_conexion)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
            .Options;

        _publisher = new FakePublisher();
        _db = new ApplicationDbContextTest(options, new FakeUsuarioActual(), new FakeFecha(), _publisher);
        _db.Database.EnsureCreated();
    }

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
        await _conexion.DisposeAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_EntidadConEvento_PublicaEventoYLoLimpia()
    {
        var entidad = new EntidadPrueba();
        var evento = new FacturaEmitidaEvento(Guid.NewGuid(), "FAC-001", 1000m, "CRC", null, false);
        entidad.AgregarEventoPublico(evento);

        Assert.Single(entidad.EventosDominio);

        await _db.EntidadesPrueba.AddAsync(entidad, TestContext.Current.CancellationToken);
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, _publisher.PublicadosCount);
        Assert.IsType<FacturaEmitidaEvento>(_publisher.Publicados[0]);
        Assert.Empty(entidad.EventosDominio);
    }

    [Fact]
    public async Task SaveChangesAsync_EntidadSinEventos_NoPublicaNada()
    {
        var entidad = new EntidadPrueba();

        await _db.EntidadesPrueba.AddAsync(entidad, TestContext.Current.CancellationToken);
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, _publisher.PublicadosCount);
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleEventos_PublicaTodosYLimpia()
    {
        var entidad = new EntidadPrueba();
        var evento1 = new FacturaEmitidaEvento(Guid.NewGuid(), "FAC-001", 1000m, "CRC", null, false);
        var evento2 = new FacturaEmitidaEvento(Guid.NewGuid(), "FAC-002", 2000m, "CRC", null, false);
        entidad.AgregarEventoPublico(evento1);
        entidad.AgregarEventoPublico(evento2);

        await _db.EntidadesPrueba.AddAsync(entidad, TestContext.Current.CancellationToken);
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, _publisher.PublicadosCount);
        Assert.Empty(entidad.EventosDominio);
    }

    // ── Entidad de test ────────────────────────────────────────────────────────

    internal sealed class EntidadPrueba : BaseAuditableEntity
    {
        public string Nombre { get; set; } = "Test";

        // Expone RegistrarEvento (protected) para poder llamarlo desde el test.
        public void AgregarEventoPublico(IDomainEvent evento) => RegistrarEvento(evento);
    }

    // ── DbContext extendido para tests ─────────────────────────────────────────

    private sealed class ApplicationDbContextTest(
        DbContextOptions<ApplicationDbContext> options,
        IUsuarioActual usuarioActual,
        IFechaActual fechaActual,
        IPublisher publisher)
        : ApplicationDbContext(options, usuarioActual, fechaActual, publisher)
    {
        public DbSet<EntidadPrueba> EntidadesPrueba => Set<EntidadPrueba>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<EntidadPrueba>(b =>
            {
                b.ToTable("EntidadesPrueba");
                b.HasKey(e => e.Id);
                b.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                b.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                b.Property(e => e.UsuarioCreacionId).IsRequired(false);
                b.Property(e => e.FechaCreacion).IsRequired();
                b.Property(e => e.UsuarioModificacionId).IsRequired(false);
                b.Property(e => e.FechaModificacion).IsRequired(false);
                // Sin FKs a Usuario para simplificar el test.
            });
        }
    }

    // ── Fakes ──────────────────────────────────────────────────────────────────

    private sealed class FakePublisher : IPublisher
    {
        private readonly List<IDomainEvent> _publicados = [];

        public IReadOnlyList<IDomainEvent> Publicados => _publicados;
        public int PublicadosCount => _publicados.Count;

        public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            if (notification is IDomainEvent evento)
            {
                _publicados.Add(evento);
            }
            return ValueTask.CompletedTask;
        }

        public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
        {
            if (notification is IDomainEvent evento)
            {
                _publicados.Add(evento);
            }
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeUsuarioActual : IUsuarioActual
    {
        public Guid UsuarioId => throw new InvalidOperationException("Sin usuario en test.");
        public string NombreUsuario => "test";
        public bool RequiereCambioPassword => false;
    }

    private sealed class FakeFecha : IFechaActual
    {
        private readonly DateTime _ahora = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        public DateTime Ahora => _ahora;
        public DateTime AhoraUtc => _ahora;
        public DateOnly Hoy => DateOnly.FromDateTime(_ahora);
        public DateOnly ALocal(DateTime utc) => DateOnly.FromDateTime(utc);
    }
}
