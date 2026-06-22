namespace PuntoVenta.Application.DTOs.Auth;

public sealed record AuthFlowResponse
{
    public bool RequiresPasswordChange { get; init; }
    public string? AccessToken { get; init; }
    public DateTime? AccessTokenExpiracionUtc { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? RefreshTokenExpiracionUtc { get; init; }
}
