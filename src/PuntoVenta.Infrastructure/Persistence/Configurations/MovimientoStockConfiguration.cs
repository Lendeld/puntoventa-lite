using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.MovimientosStock;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public sealed class MovimientoStockConfiguration : BaseAuditableEntityConfiguration<MovimientoStock>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MovimientoStock> builder)
    {
        builder.ToTable("MovimientosStock");

        builder.Property(m => m.ProductoId).IsRequired();
        builder.Property(m => m.FechaUtc).IsRequired();
        builder.Property(m => m.Delta).IsRequired().HasColumnType("TEXT");
        builder.Property(m => m.SaldoResultante).IsRequired().HasColumnType("TEXT");
        builder.Property(m => m.TipoDocumentoOrigen).IsRequired(false);
        builder.Property(m => m.DocumentoVentaId).IsRequired(false);
        builder.Property(m => m.ConsecutivoDocumento).HasMaxLength(20).IsRequired(false);
        builder.Property(m => m.UsuarioId).IsRequired(false);
        builder.Property(m => m.Razon).HasMaxLength(MovimientoStock.RazonMaxLength).IsRequired(false);

        builder.HasIndex(m => m.ProductoId);
        builder.HasIndex(m => m.FechaUtc);
    }
}
