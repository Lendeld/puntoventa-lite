using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Impresion;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public sealed class PerfilImpresoraTicketConfiguration : BaseAuditableEntityConfiguration<PerfilImpresoraTicket>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PerfilImpresoraTicket> builder)
    {
        builder.ToTable("PerfilesImpresoraTicket");

        builder.Property(e => e.Clave)
            .IsRequired()
            .HasMaxLength(PerfilImpresoraTicket.ClaveMaxLength);

        builder.HasIndex(e => e.Clave).IsUnique();

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(PerfilImpresoraTicket.NombreMaxLength);

        builder.Property(e => e.AnchoMm).IsRequired();
        builder.Property(e => e.CharsPorLinea).IsRequired();

        builder.Property(e => e.Codepage)
            .IsRequired()
            .HasMaxLength(PerfilImpresoraTicket.CodepageMaxLength);

        builder.Property(e => e.DrawerPin).IsRequired();
        builder.Property(e => e.ComandoCorte).IsRequired().HasConversion<int>();
        builder.Property(e => e.Densidad).IsRequired();
    }
}
