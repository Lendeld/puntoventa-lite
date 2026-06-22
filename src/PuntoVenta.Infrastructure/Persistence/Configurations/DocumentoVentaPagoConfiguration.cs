using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class DocumentoVentaPagoConfiguration : IEntityTypeConfiguration<DocumentoVentaPago>
{
    public void Configure(EntityTypeBuilder<DocumentoVentaPago> builder)
    {
        builder.ToTable("DocumentosVentaPagos");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.MonedaCodigo).IsRequired().HasMaxLength(DocumentoVentaPago.MonedaCodigoMaxLength);
        builder.Property(e => e.TipoCambioAplicado).HasColumnType("numeric(18,6)");
        builder.Property(e => e.MedioPagoCodigo).IsRequired().HasMaxLength(DocumentoVentaPago.MedioPagoCodigoMaxLength);
        builder.Property(e => e.MedioPagoDetalleSnapshot).IsRequired().HasMaxLength(DocumentoVentaPago.MedioPagoDetalleSnapshotMaxLength);
        builder.Property(e => e.MontoEntregado).HasColumnType("numeric(18,5)");
        builder.Property(e => e.MontoAplicadoMonedaPago).HasColumnType("numeric(18,5)");
        builder.Property(e => e.MontoAplicadoDocumento).HasColumnType("numeric(18,5)");
        builder.Property(e => e.MontoVueltoMonedaPago).HasColumnType("numeric(18,5)");
        builder.Property(e => e.MontoVueltoDocumento).HasColumnType("numeric(18,5)");
        builder.Property(e => e.FechaPago).IsRequired();
        builder.Property(e => e.FechaRegistroUtc).IsRequired();
        builder.Property(e => e.NumeroAbono).IsRequired();
        builder.Property(e => e.Anulado).IsRequired();
        builder.Property(e => e.FechaAnulacionUtc).IsRequired(false);
        builder.Property(e => e.UsuarioRegistroId).IsRequired(false);
        builder.Property(e => e.UsuarioAnulaId).IsRequired(false);
        builder.Property(e => e.Referencia).IsRequired(false).HasMaxLength(DocumentoVentaPago.ReferenciaMaxLength);
        builder.Property(e => e.Observacion).IsRequired(false).HasMaxLength(DocumentoVentaPago.ObservacionMaxLength);
        builder.Property(e => e.MotivoAnulacion).IsRequired(false).HasMaxLength(DocumentoVentaPago.MotivoAnulacionMaxLength);
        builder.Property(e => e.ClaveHaciendaREP).IsRequired(false).HasMaxLength(DocumentoVentaPago.ClaveHaciendaMaxLength);
        builder.Property(e => e.ConsecutivoHaciendaREP).IsRequired(false).HasMaxLength(DocumentoVentaPago.ConsecutivoHaciendaMaxLength);
        builder.Property(e => e.EstadoElectronicoREP).IsRequired(false).HasMaxLength(DocumentoVentaPago.EstadoElectronicoMaxLength);
        builder.Property(e => e.FechaAceptacionREP).IsRequired(false);

        builder.HasOne(e => e.UsuarioRegistro)
            .WithMany()
            .HasForeignKey(e => e.UsuarioRegistroId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.UsuarioAnula)
            .WithMany()
            .HasForeignKey(e => e.UsuarioAnulaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
