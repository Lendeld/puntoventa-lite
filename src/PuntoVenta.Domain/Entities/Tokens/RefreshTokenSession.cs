namespace PuntoVenta.Domain.Entities.Tokens;

public sealed class RefreshTokenSession : Entity
{
    private RefreshTokenSession() { }

    public Guid UsuarioId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiracionUtc { get; private set; }

    public DateTime CreadoEnUtc { get; private set; }

    public DateTime? RevocadoEnUtc { get; private set; }

    public string? ReemplazadoPorTokenHash { get; private set; }

    public DateTime? UltimoUsoEnUtc { get; private set; }

    public string? CreadoPorIp { get; private set; }

    public string? UltimoUsoPorIp { get; private set; }

    public bool EstaActivo(DateTime ahoraUtc) =>
        RevocadoEnUtc is null && ExpiracionUtc > ahoraUtc;

    public bool FueRotado => !string.IsNullOrWhiteSpace(ReemplazadoPorTokenHash);

    public static RefreshTokenSession Crear(
        Guid usuarioId,
        string tokenHash,
        DateTime creadoEnUtc,
        DateTime expiracionUtc,
        string? creadoPorIp = null) =>
        new()
        {
            UsuarioId = usuarioId,
            TokenHash = tokenHash,
            CreadoEnUtc = creadoEnUtc,
            ExpiracionUtc = expiracionUtc,
            CreadoPorIp = creadoPorIp
        };

    public void MarcarUso(DateTime usadoEnUtc, string? ultimoUsoPorIp = null)
    {
        UltimoUsoEnUtc = usadoEnUtc;
        UltimoUsoPorIp = ultimoUsoPorIp;
    }

    public void Revocar(DateTime revocadoEnUtc, string? reemplazadoPorTokenHash = null)
    {
        if (RevocadoEnUtc is not null)
        {
            return;
        }

        RevocadoEnUtc = revocadoEnUtc;
        ReemplazadoPorTokenHash = reemplazadoPorTokenHash;
    }
}
