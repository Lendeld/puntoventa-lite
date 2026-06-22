using PuntoVenta.Application.Commands.Cajas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Cajas;

namespace PuntoVenta.UnitTests.Application.Cajas;

public class CrearCajaHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeCrearCaja_CuandoDatosValidos()
    {
        var repo = new FakeCajaRepository();
        var handler = new CrearCajaHandler(repo);

        var resultado = await handler.Handle(
            new CrearCajaCommand("CAJA01", "Caja Principal"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.NotEqual(Guid.Empty, resultado.Value);
        Assert.Single(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Conflicto — código duplicado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoCodigoYaExiste()
    {
        var repo = new FakeCajaRepository();
        repo.CodigosExistentes.Add("CAJA01");
        var handler = new CrearCajaHandler(repo);

        var resultado = await handler.Handle(
            new CrearCajaCommand("caja01", "Otra Caja"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CajaErrors.CodigoYaExiste.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Error de dominio — no persiste
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarErrorDominio_CuandoCodigoVacio()
    {
        var repo = new FakeCajaRepository();
        var handler = new CrearCajaHandler(repo);

        var resultado = await handler.Handle(
            new CrearCajaCommand(null, "Caja Principal"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CajaErrors.CodigoRequerido.Code);
        Assert.Empty(repo.Guardados);
    }

    [Fact]
    public async Task Handle_DebeRetornarErrorDominio_CuandoNombreVacio()
    {
        var repo = new FakeCajaRepository();
        var handler = new CrearCajaHandler(repo);

        var resultado = await handler.Handle(
            new CrearCajaCommand("CAJA01", null),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CajaErrors.NombreRequerido.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

    private sealed class FakeCajaRepository : ICajaRepository
    {
        public List<Caja> Guardados { get; } = [];
        public HashSet<string> CodigosExistentes { get; } = [];

        public Task<bool> ExisteCodigoAsync(string codigoNormalizado, CancellationToken cancellationToken = default)
            => Task.FromResult(CodigosExistentes.Contains(codigoNormalizado));

        public Task<Caja?> ObtenerPorCodigoAsync(string codigoNormalizado, CancellationToken cancellationToken = default)
            => Task.FromResult<Caja?>(null);

        public Task<Caja?> ObtenerEditableAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Caja?>(null);

        public Task<IReadOnlyList<Caja>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Caja>>([]);

        public Task<Caja> AddAsync(Caja entity, CancellationToken cancellationToken = default)
        {
            Guardados.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<Caja?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Caja?>(null);

        public Task<IReadOnlyList<Caja>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Caja>>([]);

        public Task UpdateAsync(Caja entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Caja entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
