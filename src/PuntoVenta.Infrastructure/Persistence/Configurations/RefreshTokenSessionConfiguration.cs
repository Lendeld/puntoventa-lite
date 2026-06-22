using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Tokens;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenSessionConfiguration : IEntityTypeConfiguration<RefreshTokenSession>
{
    public void Configure(EntityTypeBuilder<RefreshTokenSession> builder)
    {
        builder.ToTable("RefreshTokenSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsuarioId)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.ExpiracionUtc)
            .IsRequired();

        builder.Property(x => x.CreadoEnUtc)
            .IsRequired();

        builder.Property(x => x.ReemplazadoPorTokenHash)
            .HasMaxLength(128);

        builder.Property(x => x.CreadoPorIp)
            .HasMaxLength(64);

        builder.Property(x => x.UltimoUsoPorIp)
            .HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => new { x.UsuarioId, x.ExpiracionUtc });
    }
}
