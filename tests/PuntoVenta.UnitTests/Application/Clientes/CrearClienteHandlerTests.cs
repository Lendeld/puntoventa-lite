using ErrorOr;
using PuntoVenta.Application.Commands.Clientes;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.UnitTests.Application.Clientes;

public class CrearClienteHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeCrearCliente_CuandoDatosValidos()
    {
        var repo = new FakeClienteRepository();
        var handler = new CrearClienteHandler(repo);

        var command = new CrearClienteCommand("Juan Pérez", "12345678");
        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.NotEqual(Guid.Empty, resultado.Value);
        Assert.Single(repo.Guardados);
    }

    [Fact]
    public async Task Handle_DebeCrearCliente_SinIdentificacion()
    {
        var repo = new FakeClienteRepository();
        var handler = new CrearClienteHandler(repo);

        var resultado = await handler.Handle(
            new CrearClienteCommand("Ana López"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Conflicto — identificación duplicada
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoIdentificacionYaExiste()
    {
        var repo = new FakeClienteRepository();
        repo.IdentificacionesExistentes.Add("12345678");
        var handler = new CrearClienteHandler(repo);

        var resultado = await handler.Handle(
            new CrearClienteCommand("Otro Cliente", "12345678"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ClienteErrors.IdentificacionYaExiste.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Error de dominio — no persiste
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarErrorDominio_CuandoNombreVacio()
    {
        var repo = new FakeClienteRepository();
        var handler = new CrearClienteHandler(repo);

        var resultado = await handler.Handle(
            new CrearClienteCommand(string.Empty),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ClienteErrors.NombreRequerido.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

    private sealed class FakeClienteRepository : IClienteRepository
    {
        public List<Cliente> Guardados { get; } = [];
        public HashSet<string> IdentificacionesExistentes { get; } = [];

        public Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
            => Task.FromResult(IdentificacionesExistentes.Contains(identificacion));

        public Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<Cliente?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Cliente?>(null);

        public Task<(IReadOnlyList<Cliente> Items, int Total)> ObtenerListaPaginadoAsync(
            int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Cliente>, int)>(([], 0));

        public Task<Cliente> AddAsync(Cliente entity, CancellationToken cancellationToken = default)
        {
            Guardados.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<Cliente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Cliente?>(null);

        public Task<IReadOnlyList<Cliente>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Cliente>>([]);

        public Task UpdateAsync(Cliente entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Cliente entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
