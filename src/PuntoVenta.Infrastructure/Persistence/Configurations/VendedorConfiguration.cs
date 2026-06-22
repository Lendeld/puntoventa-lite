using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Vendedores;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class VendedorConfiguration : BaseAuditableEntityConfiguration<Vendedor>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Vendedor> builder)
    {
        builder.ToTable("Vendedores");

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(Vendedor.NombreMaxLength);

        builder.Property(e => e.NombreNormalizado)
            .IsRequired()
            .HasMaxLength(Vendedor.NombreMaxLength);

        builder.Property(e => e.IsPrincipal)
            .IsRequired();

        builder.HasIndex(e => e.NombreNormalizado).IsUnique();
    }
}
