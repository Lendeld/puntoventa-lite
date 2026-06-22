using PuntoVenta.Application.Commands.Negocios;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.UnitTests.Application.Negocios;

public class TerminosHandlersTests
{
    // ── AceptarTerminos ──────────────────────────────

    [Fact]
    public async Task Aceptar_DebeRegistrar_CuandoVersionVigente()
    {
        var negocio = Negocio.Crear("Mi Negocio").Value;
        var repo = new FakeNegocioRepository(negocio);
        var fecha = new FakeFechaActual(new DateTime(2026, 6, 13, 12, 0, 0, DateTimeKind.Utc));
        var handler = new AceptarTerminosHandler(repo, fecha);

        var resultado = await handler.Handle(
            new AceptarTerminosCommand(TerminosConstants.VersionVigente),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Equal(TerminosConstants.VersionVigente, negocio.TerminosAceptadosVersion);
        Assert.Equal(fecha.AhoraUtc, negocio.TerminosAceptadosFechaUtc);
        Assert.True(repo.Actualizado);
    }

    [Fact]
    public async Task Aceptar_DebeFallar_CuandoVersionNoVigente()
    {
        var negocio = Negocio.Crear("Mi Negocio").Value;
        var repo = new FakeNegocioRepository(negocio);
        var handler = new AceptarTerminosHandler(repo, new FakeFechaActual(DateTime.UtcNow));

        var resultado = await handler.Handle(
            new AceptarTerminosCommand("version-vieja"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == NegocioErrors.TerminosVersionInvalida.Code);
        Assert.Null(negocio.TerminosAceptadosVersion);
        Assert.False(repo.Actualizado);
    }

    [Fact]
    public async Task Aceptar_DebeFallar_CuandoNoHayNegocio()
    {
        var repo = new FakeNegocioRepository(null);
        var handler = new AceptarTerminosHandler(repo, new FakeFechaActual(DateTime.UtcNow));

        var resultado = await handler.Handle(
            new AceptarTerminosCommand(TerminosConstants.VersionVigente),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == NegocioErrors.NoEncontrado.Code);
        Assert.False(repo.Actualizado);
    }

    // ── ObtenerEstadoTerminos ────────────────────────

    [Fact]
    public async Task Estado_DebeSerAceptado_CuandoVersionCoincide()
    {
        var negocio = Negocio.Crear("Mi Negocio").Value;
        negocio.AceptarTerminos(TerminosConstants.VersionVigente, DateTime.UtcNow);
        var handler = new ObtenerEstadoTerminosHandler(new FakeNegocioRepository(negocio));

        var resultado = await handler.Handle(new ObtenerEstadoTerminosQuery(), CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.True(resultado.Value.Aceptado);
        Assert.Equal(TerminosConstants.VersionVigente, resultado.Value.VersionVigente);
    }

    [Fact]
    public async Task Estado_DebeSerNoAceptado_CuandoNuncaAcepto()
    {
        var negocio = Negocio.Crear("Mi Negocio").Value;
        var handler = new ObtenerEstadoTerminosHandler(new FakeNegocioRepository(negocio));

        var resultado = await handler.Handle(new ObtenerEstadoTerminosQuery(), CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.False(resultado.Value.Aceptado);
    }

    // ── Fakes ────────────────────────────────────────

    private sealed class FakeNegocioRepository(Negocio? negocio) : INegocioRepository
    {
        private readonly Negocio? _negocio = negocio;
        public bool Actualizado { get; private set; }

        public Task<Negocio?> ObtenerAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_negocio);

        public Task<Negocio?> ObtenerEditableAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_negocio);

        public Task<Negocio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_negocio);

        public Task<IReadOnlyList<Negocio>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Negocio>>([]);

        public Task<Negocio> AddAsync(Negocio entity, CancellationToken cancellationToken = default)
            => Task.FromResult(entity);

        public Task UpdateAsync(Negocio entity, CancellationToken cancellationToken = default)
        {
            Actualizado = true;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Negocio entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeFechaActual(DateTime utc) : IFechaActual
    {
        private readonly DateTime _utc = utc;
        public DateTime Ahora => _utc;
        public DateTime AhoraUtc => _utc;
        public DateOnly Hoy => DateOnly.FromDateTime(_utc);
        public DateOnly ALocal(DateTime utc) => DateOnly.FromDateTime(utc);
    }
}
