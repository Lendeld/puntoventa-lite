using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : BaseAuditableEntityConfiguration<Cliente>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(Cliente.NombreMaxLength);

        builder.Property(e => e.NombreNormalizado)
            .IsRequired()
            .HasMaxLength(Cliente.NombreMaxLength);

        builder.Property(e => e.Identificacion)
            .IsRequired(false)
            .HasMaxLength(Cliente.IdentificacionMaxLength);

        builder.Property(e => e.Correo)
            .IsRequired(false)
            .HasMaxLength(Cliente.CorreoMaxLength);

        builder.Property(e => e.Telefono)
            .IsRequired(false)
            .HasMaxLength(Cliente.TelefonoMaxLength);

        builder.Property(e => e.Observaciones)
            .IsRequired(false)
            .HasMaxLength(Cliente.ObservacionesMaxLength);

        builder.HasIndex(e => e.NombreNormalizado);

        builder.HasIndex(e => e.Identificacion)
            .IsUnique()
            .HasFilter("\"Identificacion\" IS NOT NULL");
    }
}
