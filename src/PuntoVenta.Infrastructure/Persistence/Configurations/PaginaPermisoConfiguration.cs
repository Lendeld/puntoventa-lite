using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Paginas;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class PaginaPermisoConfiguration : IEntityTypeConfiguration<PaginaPermiso>
{
    public void Configure(EntityTypeBuilder<PaginaPermiso> builder)
    {
        builder.ToTable("PaginaPermisos");

        builder.HasKey(e => new { e.PaginaId, e.PermisoId });

        builder.HasOne(e => e.Pagina)
            .WithMany(p => p.PaginaPermisos)
            .HasForeignKey(e => e.PaginaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Permiso)
            .WithMany()
            .HasForeignKey(e => e.PermisoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
