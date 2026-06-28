using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Proveedores;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class ProveedorConfiguration : BaseAuditableEntityConfiguration<Proveedor>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("Proveedores");

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(Proveedor.NombreMaxLength);

        builder.Property(e => e.NombreNormalizado)
            .IsRequired()
            .HasMaxLength(Proveedor.NombreMaxLength);

        builder.Property(e => e.Correo)
            .IsRequired(false)
            .HasMaxLength(Proveedor.CorreoMaxLength);

        builder.Property(e => e.Telefono)
            .IsRequired(false)
            .HasMaxLength(Proveedor.TelefonoMaxLength);

        builder.Property(e => e.Observacion)
            .IsRequired(false)
            .HasMaxLength(Proveedor.ObservacionMaxLength);

        builder.HasIndex(e => e.NombreNormalizado).IsUnique();
    }
}
