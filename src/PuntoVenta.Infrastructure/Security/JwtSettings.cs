namespace PuntoVenta.Infrastructure.Security;

public sealed class JwtSettings
{
    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiracionMinutos { get; init; } = 30;
    public int RefreshExpiracionDias { get; init; } = 7;
}
