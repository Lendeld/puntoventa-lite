using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record EmitirNotaDebitoCommand(
    Guid DocumentoOrigenId,
    IReadOnlyList<DocumentoVentaLineaCommand> Lineas,
    string? Razon = null,
    DateTime? FechaDocumento = null,
    string? Observaciones = null) : IRequest<ErrorOr<Guid>>;

public sealed class EmitirNotaDebitoHandler(
    IDocumentoVentaRepository documentoRepository,
    ISecuenciaRepository secuenciaRepository,
    IProductoRepository productoRepository,
    ITarifaIvaImpuestoRepository tarifaRepository,
    IDocumentoVentaEventoService eventoService) : IRequestHandler<EmitirNotaDebitoCommand, ErrorOr<Guid>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly ISecuenciaRepository _secuenciaRepository = secuenciaRepository;
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly ITarifaIvaImpuestoRepository _tarifaRepository = tarifaRepository;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;

    public async ValueTask<ErrorOr<Guid>> Handle(EmitirNotaDebitoCommand command, CancellationToken cancellationToken)
    {
        var origen = await _documentoRepository.ObtenerDetalleAsync(command.DocumentoOrigenId, cancellationToken);
        if (origen is null)
        {
            return DocumentoVentaErrors.DocumentoOrigenNoEncontrado;
        }

        if (origen.Estado != EstadoDocumentoVenta.Emitido)
        {
            return DocumentoVentaErrors.DocumentoOrigenNoEmitido;
        }

        if (origen.TipoDocumento is not TipoDocumentoVenta.Factura)
        {
            return DocumentoVentaErrors.NotaDebitoTipoOrigenInvalido;
        }

        if (origen.EsCredito)
        {
            return DocumentoVentaErrors.NotaDebitoSobreFacturaCredito;
        }

        if (command.Lineas.Count == 0)
        {
            return DocumentoVentaErrors.NotaDebitoLineasRequeridas;
        }

        var lineas = await VentasHandlerHelpers.PrepararLineasAsync(
            command.Lineas,
            _productoRepository,
            _tarifaRepository,
            false,
            origen.MonedaCodigo,
            origen.TipoCambio,
            cancellationToken,
            permitirPrecioLibre: true);
        if (lineas.IsError)
        {
            return lineas.Errors;
        }

        var nota = DocumentoVenta.Crear(
            TipoDocumentoVenta.NotaDebito,
            origen.ClienteId,
            origen.VendedorId,
            origen.CondicionVentaCodigo,
            origen.CondicionVentaDetalleSnapshot,
            command.FechaDocumento ?? DateTime.UtcNow,
            origen.MonedaCodigo,
            origen.TipoCambio,
            null,
            command.Observaciones,
            origen.Id);
        if (nota.IsError)
        {
            return nota.Errors;
        }

        VentasHandlerHelpers.AgregarLineas(nota.Value, lineas.Value);

        var tipoDocReferencia = VentasHandlerHelpers.ResolverTipoDocReferencia(origen.TipoDocumento);
        var referencia = nota.Value.AgregarReferencia(origen.Id, tipoDocReferencia, origen.FechaDocumento, command.Razon);
        if (referencia.IsError)
        {
            return referencia.Errors;
        }

        var consecutivo = await VentasHandlerHelpers.SiguienteConsecutivoAsync(
            TipoDocumentoVenta.NotaDebito, _secuenciaRepository, cancellationToken);
        if (consecutivo.IsError)
        {
            return consecutivo.Errors;
        }

        var confirmar = nota.Value.ConfirmarNota(0, null, consecutivo.Value);
        if (confirmar.IsError)
        {
            return confirmar.Errors;
        }

        var correlacionId = Guid.NewGuid();

        _ = await _eventoService.RegistrarAsync(
            nota.Value.Id,
            "NotaDebitoEmitida",
            $"Nota de débito {nota.Value.Consecutivo} emitida contra {origen.Consecutivo}",
            payload: new
            {
                consecutivo = nota.Value.Consecutivo,
                total = nota.Value.TotalComprobante,
                moneda = nota.Value.MonedaCodigo,
                documentoOrigenId = origen.Id,
                origenConsecutivo = origen.Consecutivo,
                razon = command.Razon,
                tipoDocReferencia
            },
            correlacionId: correlacionId,
            cancellationToken: cancellationToken);

        _ = await _eventoService.RegistrarAsync(
            origen.Id,
            "NotaDebitoAplicada",
            $"Se aplicó nota de débito {nota.Value.Consecutivo} por {nota.Value.TotalComprobante:N2} {nota.Value.MonedaCodigo}",
            payload: new
            {
                notaDebitoId = nota.Value.Id,
                notaDebitoConsecutivo = nota.Value.Consecutivo,
                total = nota.Value.TotalComprobante,
                moneda = nota.Value.MonedaCodigo,
                razon = command.Razon
            },
            correlacionId: correlacionId,
            cancellationToken: cancellationToken);

        await _documentoRepository.AddAsync(nota.Value, cancellationToken);
        return nota.Value.Id;
    }
}
