using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : BaseAuditableEntityConfiguration<Usuario>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.Property(e => e.NombreUsuario)
            .IsRequired()
            .HasMaxLength(Usuario.NombreUsuarioMaxLength);

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(Usuario.NombreMaxLength);

        builder.Property(e => e.Correo)
            .IsRequired(false)
            .HasMaxLength(Usuario.CorreoMaxLength);

        builder.Property(e => e.Identificacion)
            .IsRequired()
            .HasMaxLength(Usuario.IdentificacionMaxLength);

        builder.Property(e => e.Telefono)
            .IsRequired(false)
            .HasMaxLength(Usuario.TelefonoMaxLength);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.PinHash)
            .IsRequired(false)
            .HasMaxLength(512);

        builder.Property(e => e.DebeCambiarPassword)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.PasswordTemporalExpiraEnUtc)
            .IsRequired(false);

        builder.Property(e => e.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.EsPropietario)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(e => e.NombreUsuario)
            .IsUnique();
    }
}
