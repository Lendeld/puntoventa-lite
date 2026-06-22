using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Categorias;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : BaseAuditableEntityConfiguration<Producto>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");

        builder.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(Producto.CodigoMaxLength);

        builder.Property(e => e.CodigoBarras)
            .IsRequired(false)
            .HasMaxLength(Producto.CodigoBarrasMaxLength);

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(Producto.NombreMaxLength);

        builder.Property(e => e.Descripcion)
            .IsRequired(false)
            .HasMaxLength(Producto.DescripcionMaxLength);

        builder.Property(e => e.TipoItem)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.ImagenUrl)
            .IsRequired(false)
            .HasMaxLength(Producto.ImagenUrlMaxLength);

        builder.Property(e => e.PrecioUnitario)
            .IsRequired()
            .HasColumnType("decimal(18,5)");

        builder.Property(e => e.PrecioCosto)
            .IsRequired(false)
            .HasColumnType("decimal(18,5)");

        builder.Property(e => e.TarifaIvaImpuestoCodigo)
            .IsRequired(false)
            .HasMaxLength(Producto.TarifaIvaCodigoMaxLength);

        builder.Property(e => e.NoAplicaExistencias)
            .IsRequired();

        builder.Property(e => e.PermiteModificarPrecioUnitario)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(e => e.Codigo).IsUnique();

        builder.HasIndex(e => e.Nombre);

        builder.HasIndex(e => e.CodigoBarras)
            .IsUnique()
            .HasFilter("\"CodigoBarras\" IS NOT NULL");

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(e => e.CategoriaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TarifaIvaImpuesto>()
            .WithMany()
            .HasForeignKey(e => e.TarifaIvaImpuestoCodigo)
            .HasPrincipalKey(e => e.Codigo)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
