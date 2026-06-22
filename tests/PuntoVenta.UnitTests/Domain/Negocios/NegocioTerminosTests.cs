using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.UnitTests.Domain.Negocios;

public class NegocioTerminosTests
{
    [Fact]
    public void AceptarTerminos_DebeGuardarVersionTrimYFecha()
    {
        var negocio = Negocio.Crear("Mi Negocio").Value;
        var fecha = new DateTime(2026, 6, 13, 10, 0, 0, DateTimeKind.Utc);

        negocio.AceptarTerminos("  2026-06-13  ", fecha);

        Assert.Equal("2026-06-13", negocio.TerminosAceptadosVersion);
        Assert.Equal(fecha, negocio.TerminosAceptadosFechaUtc);
    }

    [Fact]
    public void Crear_DebeNacerSinTerminosAceptados()
    {
        var negocio = Negocio.Crear("Mi Negocio").Value;

        Assert.Null(negocio.TerminosAceptadosVersion);
        Assert.Null(negocio.TerminosAceptadosFechaUtc);
    }
}
