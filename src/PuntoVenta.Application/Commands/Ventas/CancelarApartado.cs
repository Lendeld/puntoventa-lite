using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record CancelarApartadoCommand(Guid Id) : IRequest<ErrorOr<Guid>>;

public sealed class CancelarApartadoHandler(
    IDocumentoVentaRepository documentoRepository,
    IDocumentoVentaEventoService eventoService) : IRequestHandler<CancelarApartadoCommand, ErrorOr<Guid>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;

    public async ValueTask<ErrorOr<Guid>> Handle(CancelarApartadoCommand command, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.ObtenerEditableAsync(command.Id, cancellationToken);
        if (documento is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        var cancelar = documento.Cancelar();
        if (cancelar.IsError)
        {
            return cancelar.Errors;
        }

        _ = await _eventoService.RegistrarAsync(
            documento.Id,
            "ApartadoCancelado",
            $"Apartado {documento.Consecutivo} cancelado",
            payload: new
            {
                consecutivo = documento.Consecutivo,
                totalPagado = documento.TotalPagado,
                saldoPendiente = documento.SaldoPendiente
            },
            cancellationToken: cancellationToken);
        await _documentoRepository.UpdateAsync(documento, cancellationToken);
        return documento.Id;
    }
}
