using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Paginas;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class PaginaConfiguration : BaseAuditableEntityConfiguration<Pagina>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Pagina> builder)
    {
        builder.ToTable("Paginas");

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(Pagina.NombreMaxLength);

        builder.Property(e => e.Ruta)
            .IsRequired()
            .HasMaxLength(Pagina.RutaMaxLength);

        builder.Property(e => e.Icono)
            .IsRequired(false)
            .HasMaxLength(Pagina.IconoMaxLength);

        builder.Property(e => e.Orden)
            .IsRequired();

        builder.HasOne(e => e.PaginaPadre)
            .WithMany()
            .HasForeignKey(e => e.PaginaPadreId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
