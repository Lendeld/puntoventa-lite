using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.Commands.Negocios;

public sealed record SubirLogoNegocioCommand(
    Stream Contenido,
    string NombreArchivo,
    string ContentType) : IRequest<ErrorOr<string>>;

public sealed class SubirLogoNegocioHandler(
    INegocioRepository negocioRepository,
    IImagenStorageService storageService) : IRequestHandler<SubirLogoNegocioCommand, ErrorOr<string>>
{
    private readonly INegocioRepository _negocioRepository = negocioRepository;
    private readonly IImagenStorageService _storageService = storageService;

    public async ValueTask<ErrorOr<string>> Handle(SubirLogoNegocioCommand command, CancellationToken cancellationToken)
    {
        var negocio = await _negocioRepository.ObtenerEditableAsync(cancellationToken);
        if (negocio is null)
        {
            return NegocioErrors.NoEncontrado;
        }

        var publicId = $"negocios/logos/{negocio.Id}";
        var resultado = await _storageService.SubirAsync(command.Contenido, command.NombreArchivo, command.ContentType, publicId, cancellationToken);
        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        negocio.ActualizarLogo(resultado.Value);
        await _negocioRepository.UpdateAsync(negocio, cancellationToken);
        return resultado.Value;
    }
}
