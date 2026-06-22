using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher, ISingletonService
{
    private const int _workFactor = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.EnhancedHashPassword(password, _workFactor);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
}
