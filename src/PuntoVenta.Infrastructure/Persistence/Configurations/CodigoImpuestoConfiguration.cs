using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.CodigosImpuesto;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class CodigoImpuestoConfiguration : BaseAuditableEntityConfiguration<CodigoImpuesto>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CodigoImpuesto> builder)
    {
        builder.ToTable("CodigosImpuesto");

        builder.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(CodigoImpuesto.CodigoMaxLength);

        builder.Property(e => e.Detalle)
            .IsRequired()
            .HasMaxLength(CodigoImpuesto.DetalleMaxLength);

        builder.Property(e => e.Comentario)
            .IsRequired(false)
            .HasMaxLength(CodigoImpuesto.ComentarioMaxLength);

        builder.HasIndex(e => e.Codigo).IsUnique();
    }
}
