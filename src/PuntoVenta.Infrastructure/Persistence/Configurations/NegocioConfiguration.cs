using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class NegocioConfiguration : BaseAuditableEntityConfiguration<Negocio>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Negocio> builder)
    {
        builder.ToTable("Negocios");

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(Negocio.NombreMaxLength);

        builder.Property(e => e.NombreComercial)
            .IsRequired(false)
            .HasMaxLength(Negocio.NombreComercialMaxLength);

        builder.Property(e => e.Direccion)
            .IsRequired(false)
            .HasMaxLength(Negocio.DireccionMaxLength);

        builder.Property(e => e.Identificacion)
            .IsRequired(false)
            .HasMaxLength(Negocio.IdentificacionMaxLength);

        builder.Property(e => e.Correo)
            .IsRequired(false)
            .HasMaxLength(Negocio.CorreoMaxLength);

        builder.Property(e => e.Telefono)
            .IsRequired(false)
            .HasMaxLength(Negocio.TelefonoMaxLength);

        builder.Property(e => e.AplicaVendedores)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.AplicaCajas)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.LogoUrl)
            .IsRequired(false)
            .HasMaxLength(Negocio.LogoUrlMaxLength);

        builder.Property(e => e.TipoCambioPredeterminado)
            .IsRequired()
            .HasColumnType("decimal(18,5)")
            .HasDefaultValue(Negocio.TipoCambioPredeterminadoDefault);

        builder.Property(e => e.TerminosAceptadosVersion)
            .IsRequired(false)
            .HasMaxLength(Negocio.TerminosVersionMaxLength);

        builder.Property(e => e.TerminosAceptadosFechaUtc)
            .IsRequired(false);
    }
}
