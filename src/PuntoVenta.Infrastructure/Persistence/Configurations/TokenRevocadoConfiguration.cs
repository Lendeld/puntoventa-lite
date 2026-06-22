using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuntoVenta.Domain.Entities.Tokens;

namespace PuntoVenta.Infrastructure.Persistence.Configurations;

public class TokenRevocadoConfiguration : IEntityTypeConfiguration<TokenRevocado>
{
    public void Configure(EntityTypeBuilder<TokenRevocado> builder)
    {
        builder.ToTable("TokensRevocados");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Jti)
            .IsRequired()
            .HasMaxLength(36); // Formato UUID

        builder.Property(t => t.FechaExpiracion)
            .IsRequired();

        builder.Property(t => t.FechaRevocacion)
            .IsRequired();

        builder.HasIndex(t => t.Jti).IsUnique();
        builder.HasIndex(t => t.FechaExpiracion); // Para limpieza eficiente de expirados
    }
}
