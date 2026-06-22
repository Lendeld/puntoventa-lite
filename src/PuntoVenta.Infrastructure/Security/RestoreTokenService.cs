using System.Collections.Concurrent;
using System.Security.Cryptography;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Security;

/// <summary>
/// Store en memoria de tokens de restauración de un solo uso (singleton).
/// Cada token (256 bits aleatorios) queda atado a una ruta canónica y expira pronto.
/// Consumir lo elimina, garantizando un único uso. Sin persistencia: si el proceso
/// reinicia, los tokens se pierden (aceptable — el flujo de restore es inmediato).
/// </summary>
public sealed class RestoreTokenService(IFechaActual fechaActual) : IRestoreTokenService, ISingletonService
{
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(5);

    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly ConcurrentDictionary<string, (string Ruta, string Huella, DateTime ExpiraUtc)> _tokens = new();

    public string Generar(string rutaCanonica, string huella)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _tokens[token] = (rutaCanonica, huella, _fechaActual.AhoraUtc.Add(Vigencia));
        return token;
    }

    public bool Consumir(string token, string rutaCanonica, string huella)
    {
        if (string.IsNullOrEmpty(token) || !_tokens.TryRemove(token, out var entrada))
        {
            return false;
        }

        if (entrada.ExpiraUtc < _fechaActual.AhoraUtc)
        {
            return false;
        }

        // Debe coincidir tanto la ruta como la huella del archivo validado: si el .db
        // cambió entre validar y restaurar, la huella no coincide y el token no sirve.
        return string.Equals(entrada.Ruta, rutaCanonica, StringComparison.Ordinal)
            && !string.IsNullOrEmpty(huella)
            && string.Equals(entrada.Huella, huella, StringComparison.Ordinal);
    }
}
