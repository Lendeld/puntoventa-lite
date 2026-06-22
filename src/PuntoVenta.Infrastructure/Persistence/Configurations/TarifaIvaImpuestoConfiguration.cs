using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class TarifaIvaImpuestoConfiguration : BaseAuditableEntityConfiguration<TarifaIvaImpuesto>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TarifaIvaImpuesto> builder)
    {
        builder.ToTable("TarifasIvaImpuesto");

        builder.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(TarifaIvaImpuesto.CodigoMaxLength);

        builder.Property(e => e.Detalle)
            .IsRequired()
            .HasMaxLength(TarifaIvaImpuesto.DetalleMaxLength);

        builder.Property(e => e.Porcentaje)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.Comentario)
            .IsRequired(false)
            .HasMaxLength(TarifaIvaImpuesto.ComentarioMaxLength);

        builder.HasIndex(e => e.Codigo).IsUnique();
    }
}
