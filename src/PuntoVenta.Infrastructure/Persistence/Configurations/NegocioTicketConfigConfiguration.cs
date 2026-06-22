using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public sealed class NegocioTicketConfigConfiguration : BaseAuditableEntityConfiguration<NegocioTicketConfig>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NegocioTicketConfig> builder)
    {
        builder.ToTable("NegocioTicketConfigs");

        builder.Property(e => e.MensajePie)
            .IsRequired(false)
            .HasMaxLength(NegocioTicketConfig.MensajePieMaxLength);

        builder.Property(e => e.MostrarLogo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.AplicaCopiaClienteNegocio)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.MostrarCodigoBarras)
            .IsRequired()
            .HasDefaultValue(true);

        builder.OwnsMany(e => e.Configuraciones, configs =>
        {
            configs.ToJson("Configuraciones");
            configs.Property(c => c.Nombre).HasMaxLength(ConfiguracionPieDocumento.MaxNombreLength);
            configs.Property(c => c.Destino);
            configs.PrimitiveCollection(c => c.TiposDocumento);
            configs.OwnsMany(c => c.Lineas, lineas =>
            {
                lineas.Property(l => l.Texto).HasMaxLength(LineaPieDocumento.MaxTextoLength);
                lineas.Property(l => l.Alineacion);
                lineas.Property(l => l.Negrita);
                lineas.Property(l => l.Orden);
            });
        });

        builder.OwnsMany(e => e.ElementosEncabezado, elementos =>
        {
            elementos.ToJson("ElementosEncabezado");
            elementos.Property(x => x.Tipo);
            elementos.Property(x => x.Orden);
            elementos.Property(x => x.Visible);
            elementos.Property(x => x.TextoLibre).HasMaxLength(ElementoEncabezado.MaxTextoLibreLength);
        });
    }
}
