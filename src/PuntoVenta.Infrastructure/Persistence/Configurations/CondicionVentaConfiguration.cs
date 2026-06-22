using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.CondicionesVenta;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class CondicionVentaConfiguration : BaseAuditableEntityConfiguration<CondicionVenta>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CondicionVenta> builder)
    {
        builder.ToTable("CondicionesVenta");

        builder.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(CondicionVenta.CodigoMaxLength);

        builder.Property(e => e.Detalle)
            .IsRequired()
            .HasMaxLength(CondicionVenta.DetalleMaxLength);

        builder.Property(e => e.Comentario)
            .IsRequired(false)
            .HasMaxLength(CondicionVenta.ComentarioMaxLength);

        builder.HasIndex(e => e.Codigo).IsUnique();
    }
}
