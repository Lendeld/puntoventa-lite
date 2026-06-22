namespace PuntoVenta.Application.Interfaces;

/// <summary>
/// Acuña y consume tokens de capacidad de un solo uso para autorizar la restauración
/// de un respaldo. El token se acuña tras validar PIN + permiso + versión de esquema
/// (en ValidarBackup) y se consume desde Electron main antes de reemplazar la DB,
/// atado a la ruta exacta del archivo. Así el swap nativo no puede dispararse sin una
/// validación previa real (defensa en profundidad).
/// </summary>
public interface IRestoreTokenService
{
    /// <summary>
    /// Genera un token de un solo uso atado a la ruta canónica y a la huella (hash)
    /// del archivo validado, para que el token no sirva si el archivo cambia luego.
    /// </summary>
    string Generar(string rutaCanonica, string huella);

    /// <summary>
    /// Consume el token: lo invalida y devuelve true solo si existía, no expiró y
    /// corresponde exactamente a la misma ruta canónica y huella con que se generó.
    /// </summary>
    bool Consumir(string token, string rutaCanonica, string huella);
}
