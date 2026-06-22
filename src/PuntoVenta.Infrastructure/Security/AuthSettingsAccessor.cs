using Microsoft.Extensions.Options;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Security;

public sealed class AuthSettingsAccessor(IOptions<JwtSettings> options) : IAuthSettings, IScopedService
{
    private readonly JwtSettings _settings = options.Value;

    public string Issuer => _settings.Issuer;
    public int RefreshExpiracionDias => _settings.RefreshExpiracionDias;
}
