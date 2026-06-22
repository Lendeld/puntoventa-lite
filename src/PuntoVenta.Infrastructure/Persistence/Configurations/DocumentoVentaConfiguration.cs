using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class DocumentoVentaConfiguration : BaseAuditableEntityConfiguration<DocumentoVenta>
{
    protected override void ConfigureEntity(EntityTypeBuilder<DocumentoVenta> builder)
    {
        builder.ToTable("DocumentosVenta");

        builder.Property(e => e.TipoDocumento).IsRequired();
        builder.Property(e => e.Estado).IsRequired();
        builder.Property(e => e.CondicionVentaCodigo).IsRequired().HasMaxLength(DocumentoVenta.CondicionVentaCodigoMaxLength);
        builder.Property(e => e.CondicionVentaDetalleSnapshot).IsRequired().HasMaxLength(DocumentoVenta.CondicionVentaDetalleMaxLength);
        builder.Property(e => e.MonedaCodigo).IsRequired().HasMaxLength(DocumentoVenta.MonedaCodigoMaxLength);
        builder.Property(e => e.Consecutivo).IsRequired(false).HasMaxLength(DocumentoVenta.ConsecutivoMaxLength);
        builder.Property(e => e.Observaciones).IsRequired(false).HasMaxLength(DocumentoVenta.ObservacionesMaxLength);

        builder.Property(e => e.TipoCambio).HasColumnType("decimal(18,6)");
        builder.Property(e => e.TotalServiciosGravados).HasColumnType("decimal(18,5)");
        builder.Property(e => e.TotalServiciosExentos).HasColumnType("decimal(18,5)");
        builder.Property(e => e.TotalMercanciasGravadas).HasColumnType("decimal(18,5)");
        builder.Property(e => e.TotalMercanciasExentas).HasColumnType("decimal(18,5)");
        builder.Property(e => e.TotalVenta).HasColumnType("decimal(18,5)");
        builder.Property(e => e.TotalDescuentos).HasColumnType("decimal(18,5)");
        builder.Property(e => e.TotalImpuesto).HasColumnType("decimal(18,5)");
        builder.Property(e => e.TotalComprobante).HasColumnType("decimal(18,5)");
        builder.Property(e => e.TotalPagado).HasColumnType("decimal(18,5)");
        builder.Property(e => e.SaldoPendiente).HasColumnType("decimal(18,5)");
        builder.Property(e => e.FechaCancelacion).IsRequired(false);

        builder.HasIndex(e => new { e.TipoDocumento, e.CajaId, e.NumeroConsecutivo }).IsUnique();
        builder.HasIndex(e => new { e.Estado, e.FechaDocumento });

        builder.HasOne(e => e.Cliente)
            .WithMany()
            .HasForeignKey(e => e.ClienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.Vendedor)
            .WithMany()
            .HasForeignKey(e => e.VendedorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.DocumentoOrigen)
            .WithMany()
            .HasForeignKey(e => e.DocumentoOrigenId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasMany(e => e.Lineas)
            .WithOne(e => e.DocumentoVenta)
            .HasForeignKey(e => e.DocumentoVentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Pagos)
            .WithOne(e => e.DocumentoVenta)
            .HasForeignKey(e => e.DocumentoVentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Referencias)
            .WithOne(e => e.DocumentoVenta)
            .HasForeignKey(e => e.DocumentoVentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Caja)
            .WithMany()
            .HasForeignKey(e => e.CajaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
