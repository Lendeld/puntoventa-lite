namespace PuntoVenta.Application.Interfaces;

public interface IOpaqueTokenService
{
    string GenerarToken();
    string CalcularHash(string token);
}
