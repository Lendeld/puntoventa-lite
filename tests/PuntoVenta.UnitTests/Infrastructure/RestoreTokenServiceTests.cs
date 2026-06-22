using PuntoVenta.Application.Interfaces;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.UnitTests.Infrastructure;

public class RestoreTokenServiceTests
{
    private const string Ruta = "/datos/puntoventa-backup.db";
    private const string Huella = "ABCD1234";

    [Fact]
    public void Consumir_DevuelveTrue_CuandoTokenValidoMismaRutaYHuella()
    {
        var reloj = new FakeFechaActual(new DateTime(2026, 6, 18, 12, 0, 0, DateTimeKind.Utc));
        var service = new RestoreTokenService(reloj);

        var token = service.Generar(Ruta, Huella);

        Assert.True(service.Consumir(token, Ruta, Huella));
    }

    [Fact]
    public void Consumir_DevuelveFalse_EnElSegundoUso()
    {
        var reloj = new FakeFechaActual(new DateTime(2026, 6, 18, 12, 0, 0, DateTimeKind.Utc));
        var service = new RestoreTokenService(reloj);

        var token = service.Generar(Ruta, Huella);

        Assert.True(service.Consumir(token, Ruta, Huella));
        Assert.False(service.Consumir(token, Ruta, Huella));
    }

    [Fact]
    public void Consumir_DevuelveFalse_CuandoRutaDistinta()
    {
        var reloj = new FakeFechaActual(new DateTime(2026, 6, 18, 12, 0, 0, DateTimeKind.Utc));
        var service = new RestoreTokenService(reloj);

        var token = service.Generar(Ruta, Huella);

        Assert.False(service.Consumir(token, "/otra/ruta.db", Huella));
    }

    [Fact]
    public void Consumir_DevuelveFalse_CuandoHuellaDistinta()
    {
        var reloj = new FakeFechaActual(new DateTime(2026, 6, 18, 12, 0, 0, DateTimeKind.Utc));
        var service = new RestoreTokenService(reloj);

        var token = service.Generar(Ruta, Huella);

        // El archivo cambió entre validar y restaurar → la huella no coincide.
        Assert.False(service.Consumir(token, Ruta, "OTRA-HUELLA"));
    }

    [Fact]
    public void Consumir_DevuelveFalse_CuandoHuellaVacia()
    {
        var reloj = new FakeFechaActual(new DateTime(2026, 6, 18, 12, 0, 0, DateTimeKind.Utc));
        var service = new RestoreTokenService(reloj);

        var token = service.Generar(Ruta, Huella);

        // Archivo ya no existe al consumir → huella vacía.
        Assert.False(service.Consumir(token, Ruta, string.Empty));
    }

    [Fact]
    public void Consumir_DevuelveFalse_CuandoTokenExpirado()
    {
        var reloj = new FakeFechaActual(new DateTime(2026, 6, 18, 12, 0, 0, DateTimeKind.Utc));
        var service = new RestoreTokenService(reloj);

        var token = service.Generar(Ruta, Huella);

        // Avanzar más allá de la vigencia (5 min)
        reloj.AhoraUtc = reloj.AhoraUtc.AddMinutes(6);

        Assert.False(service.Consumir(token, Ruta, Huella));
    }

    [Fact]
    public void Consumir_DevuelveFalse_CuandoTokenDesconocido()
    {
        var reloj = new FakeFechaActual(new DateTime(2026, 6, 18, 12, 0, 0, DateTimeKind.Utc));
        var service = new RestoreTokenService(reloj);

        Assert.False(service.Consumir("TOKEN-INEXISTENTE", Ruta, Huella));
    }

    private sealed class FakeFechaActual : IFechaActual
    {
        public FakeFechaActual(DateTime utc) => AhoraUtc = utc;
        public DateTime AhoraUtc { get; set; }
        public DateTime Ahora => AhoraUtc;
        public DateOnly Hoy => DateOnly.FromDateTime(AhoraUtc);
        public DateOnly ALocal(DateTime utc) => DateOnly.FromDateTime(utc);
    }
}
