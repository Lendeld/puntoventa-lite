using ErrorOr;

namespace PuntoVenta.Application.Interfaces;

public interface IImagenStorageService
{
    Task<ErrorOr<string>> SubirAsync(
        Stream contenido,
        string nombreArchivo,
        string contentType,
        string publicId,
        CancellationToken cancellationToken = default);
}
