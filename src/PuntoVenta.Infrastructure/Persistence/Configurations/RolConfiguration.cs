using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class RolConfiguration : BaseAuditableEntityConfiguration<Rol>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(Rol.NombreMaxLength);

        builder.Property(e => e.Descripcion)
            .IsRequired(false)
            .HasMaxLength(Rol.DescripcionMaxLength);

        builder.Property(e => e.IsPrincipal)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(e => e.Nombre).IsUnique();
    }
}
