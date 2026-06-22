using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Cajas;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class CajaConfiguration : BaseAuditableEntityConfiguration<Caja>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Caja> builder)
    {
        builder.ToTable("Cajas");

        builder.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(Caja.CodigoMaxLength);

        builder.Property(e => e.CodigoNormalizado)
            .IsRequired()
            .HasMaxLength(Caja.CodigoMaxLength);

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(Caja.NombreMaxLength);

        builder.HasIndex(e => e.CodigoNormalizado).IsUnique();
    }
}
