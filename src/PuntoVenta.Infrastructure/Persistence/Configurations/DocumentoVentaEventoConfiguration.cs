using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class DocumentoVentaEventoConfiguration : BaseAuditableEntityConfiguration<DocumentoVentaEvento>
{
    protected override void ConfigureEntity(EntityTypeBuilder<DocumentoVentaEvento> builder)
    {
        builder.ToTable("DocumentosVentaEventos");

        builder.Property(e => e.DocumentoVentaId)
            .IsRequired();

        builder.Property(e => e.TipoEventoCodigo)
            .IsRequired()
            .HasMaxLength(DocumentoVentaEvento.TipoCodigoMaxLength);

        builder.Property(e => e.OcurridoEn)
            .IsRequired();

        builder.Property(e => e.UsuarioId)
            .IsRequired(false);

        builder.Property(e => e.Resumen)
            .IsRequired()
            .HasMaxLength(DocumentoVentaEvento.ResumenMaxLength);

        builder.Property(e => e.Payload)
            .IsRequired(false);

        builder.Property(e => e.CorrelacionId)
            .IsRequired(false);

        builder.HasOne(e => e.DocumentoVenta)
            .WithMany()
            .HasForeignKey(e => e.DocumentoVentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.DocumentoVentaId, e.OcurridoEn });
        builder.HasIndex(e => new { e.TipoEventoCodigo, e.OcurridoEn });
    }
}
