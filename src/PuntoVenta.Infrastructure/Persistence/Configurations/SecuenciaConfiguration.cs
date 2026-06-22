using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Secuencias;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class SecuenciaConfiguration : BaseAuditableEntityConfiguration<Secuencia>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Secuencia> builder)
    {
        builder.ToTable("Secuencias");
        builder.Property(s => s.TipoDocumento).IsRequired();
        builder.Property(s => s.UltimoNumero).IsRequired();
        builder.HasIndex(s => s.TipoDocumento).IsUnique();
    }
}
