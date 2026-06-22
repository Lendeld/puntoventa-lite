using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public abstract class BaseAuditableEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : BaseAuditableEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.UsuarioCreacionId)
            .IsRequired(false);

        builder.Property(e => e.FechaCreacion)
            .IsRequired();

        builder.Property(e => e.UsuarioModificacionId)
            .IsRequired(false);

        builder.Property(e => e.FechaModificacion)
            .IsRequired(false);

        builder.HasOne(e => e.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
}
