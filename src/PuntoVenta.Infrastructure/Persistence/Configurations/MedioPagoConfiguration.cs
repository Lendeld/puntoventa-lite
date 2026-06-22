using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.MediosPago;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class MedioPagoConfiguration : BaseAuditableEntityConfiguration<MedioPago>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MedioPago> builder)
    {
        builder.ToTable("MediosPago");

        builder.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(MedioPago.CodigoMaxLength);

        builder.Property(e => e.Detalle)
            .IsRequired()
            .HasMaxLength(MedioPago.DetalleMaxLength);

        builder.Property(e => e.Comentario)
            .IsRequired(false)
            .HasMaxLength(MedioPago.ComentarioMaxLength);

        builder.HasIndex(e => e.Codigo).IsUnique();
    }
}
