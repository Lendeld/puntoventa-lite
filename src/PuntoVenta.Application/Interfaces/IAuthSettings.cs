namespace PuntoVenta.Application.Interfaces;

public interface IAuthSettings
{
    string Issuer { get; }
    int RefreshExpiracionDias { get; }
}
