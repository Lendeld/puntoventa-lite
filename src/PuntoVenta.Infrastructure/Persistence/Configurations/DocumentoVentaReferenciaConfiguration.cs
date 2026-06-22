using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class DocumentoVentaReferenciaConfiguration : IEntityTypeConfiguration<DocumentoVentaReferencia>
{
    public void Configure(EntityTypeBuilder<DocumentoVentaReferencia> builder)
    {
        builder.ToTable("DocumentosVentaReferencias");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TipoDocReferencia).IsRequired().HasMaxLength(DocumentoVentaReferencia.TipoDocReferenciaMaxLength);
        builder.Property(e => e.Razon).IsRequired().HasMaxLength(DocumentoVentaReferencia.RazonMaxLength);
        builder.HasOne(e => e.DocumentoReferencia).WithMany().HasForeignKey(e => e.DocumentoReferenciaId).OnDelete(DeleteBehavior.Restrict);
    }
}
