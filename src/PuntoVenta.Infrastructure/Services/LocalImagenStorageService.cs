using ErrorOr;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Services;

// Lite es offline/SQLite-only: no hay storage externo (Cloudinary) ni servidor
// de archivos estáticos. El logo se persiste como data URI (`data:<mime>;base64,...`)
// directamente en `Negocio.LogoUrl`. Lo consume el <img> del frontend (la CSP
// permite `data:`) y el PDF (decodifica el base64 sin red).
public sealed class LocalImagenStorageService : IImagenStorageService
{
    public async Task<ErrorOr<string>> SubirAsync(
        Stream contenido,
        string nombreArchivo,
        string contentType,
        string publicId,
        CancellationToken cancellationToken = default)
    {
        using var memoria = new MemoryStream();
        await contenido.CopyToAsync(memoria, cancellationToken);
        var base64 = Convert.ToBase64String(memoria.ToArray());
        return $"data:{contentType};base64,{base64}";
    }
}
