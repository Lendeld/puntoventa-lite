using ErrorOr;
using PuntoVenta.Application.Commands.Productos;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Categorias;
using PuntoVenta.Domain.Entities.MovimientosStock;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.Proveedores;

namespace PuntoVenta.UnitTests.Application.Productos;

public class CrearProductoHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeCrearProducto_CuandoDatosMinimosValidos()
    {
        var repo = new FakeProductoRepository();
        var categoriaRepo = new FakeCategoriaRepository();
        var usuarioActual = new FakeSuperAdmin();
        var permisoCache = new FakePermisoCache();
        var handler = new CrearProductoHandler(usuarioActual, repo, categoriaRepo, new FakeProveedorRepository(), new FakeMovimientoStockRepository(), new FakeFechaActual(), permisoCache);

        var command = new CrearProductoCommand("P001", "Producto", TipoItem.Bien, 1000m,
            TarifaIvaImpuestoCodigo: "08");
        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.NotEqual(Guid.Empty, resultado.Value);
        Assert.Single(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Stock inicial
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeCargarStockInicial_YRegistrarMovimiento_CuandoExistenciaInicialPositiva()
    {
        var repo = new FakeProductoRepository();
        var categoriaRepo = new FakeCategoriaRepository();
        var usuarioActual = new FakeSuperAdmin();
        var permisoCache = new FakePermisoCache();
        var movimientoRepo = new FakeMovimientoStockRepository();
        var handler = new CrearProductoHandler(usuarioActual, repo, categoriaRepo, new FakeProveedorRepository(), movimientoRepo, new FakeFechaActual(), permisoCache);

        var command = new CrearProductoCommand("P010", "Producto", TipoItem.Bien, 1000m,
            TarifaIvaImpuestoCodigo: "08", ExistenciaInicial: 25m);
        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Equal(25m, repo.Guardados.Single().Existencia);
        var movimiento = Assert.Single(movimientoRepo.Guardados);
        Assert.Equal(25m, movimiento.Delta);
        Assert.Equal(25m, movimiento.SaldoResultante);
    }

    [Fact]
    public async Task Handle_NoDebeRegistrarMovimiento_CuandoSinExistenciaInicial()
    {
        var repo = new FakeProductoRepository();
        var categoriaRepo = new FakeCategoriaRepository();
        var usuarioActual = new FakeSuperAdmin();
        var permisoCache = new FakePermisoCache();
        var movimientoRepo = new FakeMovimientoStockRepository();
        var handler = new CrearProductoHandler(usuarioActual, repo, categoriaRepo, new FakeProveedorRepository(), movimientoRepo, new FakeFechaActual(), permisoCache);

        var resultado = await handler.Handle(
            new CrearProductoCommand("P011", "Producto", TipoItem.Bien, 1000m,
                TarifaIvaImpuestoCodigo: "08"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Equal(0m, repo.Guardados.Single().Existencia);
        Assert.Empty(movimientoRepo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Conflicto — código duplicado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoCodigoYaExiste()
    {
        var repo = new FakeProductoRepository();
        repo.CodigosExistentes.Add("P001");
        var categoriaRepo = new FakeCategoriaRepository();
        var usuarioActual = new FakeSuperAdmin();
        var permisoCache = new FakePermisoCache();
        var handler = new CrearProductoHandler(usuarioActual, repo, categoriaRepo, new FakeProveedorRepository(), new FakeMovimientoStockRepository(), new FakeFechaActual(), permisoCache);

        var resultado = await handler.Handle(
            new CrearProductoCommand("P001", "Producto", TipoItem.Bien, 1000m),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.CodigoYaExiste.Code);
        Assert.Empty(repo.Guardados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoCodigoBarrasYaExiste()
    {
        var repo = new FakeProductoRepository();
        repo.CodigosBarrasExistentes.Add("123456");
        var categoriaRepo = new FakeCategoriaRepository();
        var usuarioActual = new FakeSuperAdmin();
        var permisoCache = new FakePermisoCache();
        var handler = new CrearProductoHandler(usuarioActual, repo, categoriaRepo, new FakeProveedorRepository(), new FakeMovimientoStockRepository(), new FakeFechaActual(), permisoCache);

        var resultado = await handler.Handle(
            new CrearProductoCommand("P002", "Producto", TipoItem.Bien, 1000m, CodigoBarras: "123456"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.CodigoBarrasYaExiste.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Error de dominio — no persiste
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarErrorDominio_CuandoNombreVacio()
    {
        var repo = new FakeProductoRepository();
        var categoriaRepo = new FakeCategoriaRepository();
        var usuarioActual = new FakeSuperAdmin();
        var permisoCache = new FakePermisoCache();
        var handler = new CrearProductoHandler(usuarioActual, repo, categoriaRepo, new FakeProveedorRepository(), new FakeMovimientoStockRepository(), new FakeFechaActual(), permisoCache);

        var resultado = await handler.Handle(
            new CrearProductoCommand("P001", string.Empty, TipoItem.Bien, 1000m,
                TarifaIvaImpuestoCodigo: "08"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.NombreRequerido.Code);
        Assert.Empty(repo.Guardados);
    }

    [Fact]
    public async Task Handle_DebeRetornarErrorDominio_CuandoTarifaIvaVacia()
    {
        var repo = new FakeProductoRepository();
        var categoriaRepo = new FakeCategoriaRepository();
        var usuarioActual = new FakeSuperAdmin();
        var permisoCache = new FakePermisoCache();
        var handler = new CrearProductoHandler(usuarioActual, repo, categoriaRepo, new FakeProveedorRepository(), new FakeMovimientoStockRepository(), new FakeFechaActual(), permisoCache);

        var resultado = await handler.Handle(
            new CrearProductoCommand("P001", "Producto", TipoItem.Bien, 1000m),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.TarifaIvaRequerida.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

    private sealed class FakeProductoRepository : IProductoRepository
    {
        public List<Producto> Guardados { get; } = [];
        public HashSet<string> CodigosExistentes { get; } = [];
        public HashSet<string> CodigosBarrasExistentes { get; } = [];

        public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
            => Task.FromResult(CodigosExistentes.Contains(codigo));

        public Task<bool> ExisteCodigoExcluyendoAsync(string codigo, Guid excludeId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default)
            => Task.FromResult(CodigosBarrasExistentes.Contains(codigoBarras));

        public Task<bool> ExisteCodigoBarrasExcluyendoAsync(string codigoBarras, Guid excludeId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default)
            => Task.FromResult<Producto?>(null);

        public Task<Producto?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Producto?>(null);

        public Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Producto>>([]);

        public Task<IReadOnlyList<Producto>> ObtenerPorIdsEditablesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Producto>>([]);

        public Task<Producto?> ObtenerEditableAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Producto?>(null);

        public Task<(IReadOnlyList<Producto> Items, int Total)> ObtenerListaPaginadoAsync(
            int pagina, int tamano, string? filtroDinamico, TipoItem? tipoItem, Guid? categoriaId, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Producto>, int)>(([], 0));

        public Task<IReadOnlyList<PuntoVenta.Application.DTOs.Inventarios.InventarioReporteProyeccionDto>> ObtenerReporteInventarioProyectadoAsync(
            string? codigo, Guid? categoriaId, int maxFilas, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PuntoVenta.Application.DTOs.Inventarios.InventarioReporteProyeccionDto>>([]);

        public Task<Producto> AddAsync(Producto entity, CancellationToken cancellationToken = default)
        {
            Guardados.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<Producto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Producto?>(null);

        public Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Producto>>([]);

        public Task UpdateAsync(Producto entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Producto entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeCategoriaRepository : ICategoriaRepository
    {
        public Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<IReadOnlyList<Categoria>> ObtenerActivosAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Categoria>>([]);
        public Task<Categoria?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Categoria?>(null);
        public Task<(IReadOnlyList<Categoria> Items, int Total)> ObtenerListaPaginadoAsync(int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default) => Task.FromResult<(IReadOnlyList<Categoria>, int)>(([], 0));
        public Task<Categoria> AddAsync(Categoria entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task<Categoria?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Categoria?>(null);
        public Task<IReadOnlyList<Categoria>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Categoria>>([]);
        public Task UpdateAsync(Categoria entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Categoria entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeMovimientoStockRepository : IMovimientoStockRepository
    {
        public List<MovimientoStock> Guardados { get; } = [];

        public Task<MovimientoStock> AddAsync(MovimientoStock entity, CancellationToken cancellationToken = default)
        {
            Guardados.Add(entity);
            return Task.FromResult(entity);
        }

        public Task AgregarRangoSinPersistirAsync(IReadOnlyList<MovimientoStock> movimientos, CancellationToken cancellationToken = default)
        {
            Guardados.AddRange(movimientos);
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<(MovimientoStock Movimiento, string NombreProducto)> Items, int Total)> ObtenerPaginadoAsync(
            Guid? productoId, int pagina, int tamano, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<(MovimientoStock, string)>, int)>(([], 0));

        public Task<MovimientoStock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<MovimientoStock?>(null);

        public Task<IReadOnlyList<MovimientoStock>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MovimientoStock>>([]);

        public Task UpdateAsync(MovimientoStock entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(MovimientoStock entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeFechaActual : IFechaActual
    {
        public DateTime Ahora => DateTime.UtcNow;
        public DateTime AhoraUtc => DateTime.UtcNow;
        public DateOnly Hoy => DateOnly.FromDateTime(DateTime.UtcNow);
        public DateOnly ALocal(DateTime utc) => DateOnly.FromDateTime(utc);
    }

    private sealed class FakeSuperAdmin : IUsuarioActual
    {
        public Guid UsuarioId => Guid.NewGuid();
        public string NombreUsuario => "admin";
        public bool RequiereCambioPassword => false;
    }

    private sealed class FakePermisoCache : IPermisoCache
    {
        public Task<IReadOnlyList<string>> ObtenerPermisosAsync(Guid usuarioId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>([]);

        public void Invalidar(Guid usuarioId) { }
        public void InvalidarTodos() { }
    }

    private sealed class FakeProveedorRepository : IProveedorRepository
    {
        public Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<IReadOnlyList<Proveedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Proveedor>>([]);
        public Task<Proveedor?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Proveedor?>(null);
        public Task<(IReadOnlyList<Proveedor> Items, int Total)> ObtenerListaPaginadoAsync(int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default) => Task.FromResult<(IReadOnlyList<Proveedor>, int)>(([], 0));
        public Task<Proveedor> AddAsync(Proveedor entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task<Proveedor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Proveedor?>(null);
        public Task<IReadOnlyList<Proveedor>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Proveedor>>([]);
        public Task UpdateAsync(Proveedor entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Proveedor entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
