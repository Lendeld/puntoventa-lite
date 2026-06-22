using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Permisos;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class PermisoConfiguration : BaseAuditableEntityConfiguration<Permiso>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Permiso> builder)
    {
        builder.ToTable("Permisos");

        builder.Property(e => e.Clave)
            .IsRequired()
            .HasMaxLength(Permiso.ClaveMaxLength);

        builder.Property(e => e.Descripcion)
            .IsRequired()
            .HasMaxLength(Permiso.DescripcionMaxLength);

        builder.Property(e => e.Modulo)
            .IsRequired()
            .HasMaxLength(Permiso.ModuloMaxLength);

        builder.HasIndex(e => e.Clave)
            .IsUnique();

        builder.HasIndex(e => e.Modulo);
    }
}
