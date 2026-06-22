using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class DocumentoVentaLineaConfiguration : IEntityTypeConfiguration<DocumentoVentaLinea>
{
    public void Configure(EntityTypeBuilder<DocumentoVentaLinea> builder)
    {
        builder.ToTable("DocumentosVentaLineas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Codigo).IsRequired().HasMaxLength(DocumentoVentaLinea.CodigoMaxLength);
        builder.Property(e => e.Descripcion).IsRequired().HasMaxLength(DocumentoVentaLinea.DescripcionMaxLength);
        builder.Property(e => e.UnidadMedidaCodigo).IsRequired().HasMaxLength(DocumentoVentaLinea.UnidadMedidaCodigoMaxLength);
        builder.Property(e => e.TarifaIvaImpuestoCodigo).IsRequired(false).HasMaxLength(DocumentoVentaLinea.TarifaIvaCodigoMaxLength);
        builder.Property(e => e.Cantidad).HasColumnType("decimal(18,3)");
        builder.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,5)");
        builder.Property(e => e.MontoDescuento).HasColumnType("decimal(18,5)");
        builder.Property(e => e.Subtotal).HasColumnType("decimal(18,5)");
        builder.Property(e => e.MontoImpuesto).HasColumnType("decimal(18,5)");
        builder.Property(e => e.TotalLinea).HasColumnType("decimal(18,5)");
        builder.Property(e => e.PorcentajeImpuesto).HasColumnType("decimal(7,4)");
        builder.Property(e => e.NoAplicaExistencias).IsRequired();
        builder.Property(e => e.PermiteModificarPrecioUnitario).IsRequired();

        builder.HasOne(e => e.Producto).WithMany().HasForeignKey(e => e.ProductoId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
    }
}
