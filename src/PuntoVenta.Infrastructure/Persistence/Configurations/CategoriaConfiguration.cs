using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Categorias;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : BaseAuditableEntityConfiguration<Categoria>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(Categoria.NombreMaxLength);

        builder.Property(e => e.NombreNormalizado)
            .IsRequired()
            .HasMaxLength(Categoria.NombreMaxLength);

        builder.Property(e => e.Descripcion)
            .IsRequired(false)
            .HasMaxLength(Categoria.DescripcionMaxLength);

        builder.HasIndex(e => e.NombreNormalizado).IsUnique();
    }
}
