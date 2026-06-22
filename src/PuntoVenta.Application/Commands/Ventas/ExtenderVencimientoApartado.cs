using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ExtenderVencimientoApartadoCommand(Guid Id, DateTime FechaVencimiento) : IRequest<ErrorOr<Guid>>;

public sealed class ExtenderVencimientoApartadoHandler(
    IDocumentoVentaRepository documentoRepository,
    IFechaActual fechaActual,
    IDocumentoVentaEventoService eventoService) : IRequestHandler<ExtenderVencimientoApartadoCommand, ErrorOr<Guid>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;

    public async ValueTask<ErrorOr<Guid>> Handle(ExtenderVencimientoApartadoCommand command, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.ObtenerEditableAsync(command.Id, cancellationToken);
        if (documento is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        var fechaNueva = VentasHandlerHelpers.NormalizarFechaUtc(command.FechaVencimiento);
        var extender = documento.ExtenderVencimientoApartado(fechaNueva, _fechaActual.AhoraUtc);
        if (extender.IsError)
        {
            return extender.Errors;
        }

        _ = await _eventoService.RegistrarAsync(
            documento.Id,
            "VencimientoExtendido",
            $"Vencimiento del apartado extendido al {fechaNueva:yyyy-MM-dd}",
            payload: new
            {
                fechaVencimiento = fechaNueva,
                consecutivo = documento.Consecutivo
            },
            cancellationToken: cancellationToken);
        await _documentoRepository.UpdateAsync(documento, cancellationToken);
        return documento.Id;
    }
}
