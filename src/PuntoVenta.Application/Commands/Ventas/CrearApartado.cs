using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record CrearApartadoCommand(
    Guid? ClienteId,
    string CondicionVentaCodigo,
    IReadOnlyList<DocumentoVentaLineaCommand> Lineas,
    IReadOnlyList<DocumentoVentaPagoCommand> Pagos,
    Guid? CajaId = null,
    DateTime? FechaDocumento = null,
    DateTime? FechaVencimiento = null,
    string MonedaCodigo = "CRC",
    decimal TipoCambio = 1m,
    string? Observaciones = null) : IRequest<ErrorOr<Guid>>;

public sealed class CrearApartadoHandler(
    IUsuarioActual usuarioActual,
    IFechaActual fechaActual,
    IDocumentoVentaRepository documentoRepository,
    ISecuenciaRepository secuenciaRepository,
    ICondicionVentaRepository condicionVentaRepository,
    IClienteRepository clienteRepository,
    IMedioPagoRepository medioPagoRepository,
    IProductoRepository productoRepository,
    ITarifaIvaImpuestoRepository tarifaRepository,
    IDocumentoVentaEventoService eventoService,
    INegocioRepository negocioRepository) : IRequestHandler<CrearApartadoCommand, ErrorOr<Guid>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly ISecuenciaRepository _secuenciaRepository = secuenciaRepository;
    private readonly ICondicionVentaRepository _condicionVentaRepository = condicionVentaRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IMedioPagoRepository _medioPagoRepository = medioPagoRepository;
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly ITarifaIvaImpuestoRepository _tarifaRepository = tarifaRepository;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;
    private readonly INegocioRepository _negocioRepository = negocioRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearApartadoCommand command, CancellationToken cancellationToken)
    {
        var condicion = await VentasHandlerHelpers.ObtenerCondicionVentaAsync(command.CondicionVentaCodigo, _condicionVentaRepository, cancellationToken);
        if (condicion.IsError)
        {
            return condicion.Errors;
        }

        if (command.ClienteId.HasValue && await _clienteRepository.GetByIdAsync(command.ClienteId.Value, cancellationToken) is null)
        {
            return Error.Validation("DocumentoVenta_ClienteId", "El cliente indicado no existe.");
        }

        var lineas = await VentasHandlerHelpers.PrepararLineasAsync(
            command.Lineas,
            _productoRepository,
            _tarifaRepository,
            false,
            command.MonedaCodigo,
            command.TipoCambio,
            cancellationToken);
        if (lineas.IsError)
        {
            return lineas.Errors;
        }

        var pagos = await VentasHandlerHelpers.PrepararPagosAsync(command.Pagos, _medioPagoRepository, cancellationToken);
        if (pagos.IsError)
        {
            return pagos.Errors;
        }

        var fechaDocumento = VentasHandlerHelpers.NormalizarFechaUtc(command.FechaDocumento);
        var fechaVencimiento = command.FechaVencimiento.HasValue
            ? VentasHandlerHelpers.NormalizarFechaUtc(command.FechaVencimiento)
            : fechaDocumento.Date.AddDays(30);

        var negocio = await _negocioRepository.ObtenerAsync(cancellationToken);
        Guid? cajaId = negocio?.AplicaCajas == true ? command.CajaId : null;

        var consecutivo = await VentasHandlerHelpers.SiguienteConsecutivoAsync(
            TipoDocumentoVenta.Apartado, _secuenciaRepository, cancellationToken);
        if (consecutivo.IsError)
        {
            return consecutivo.Errors;
        }

        var documento = DocumentoVenta.Crear(
            TipoDocumentoVenta.Apartado,
            command.ClienteId,
            null,
            condicion.Value.Codigo,
            condicion.Value.Detalle,
            fechaDocumento,
            command.MonedaCodigo,
            command.TipoCambio,
            null,
            command.Observaciones,
            fechaVencimiento: fechaVencimiento);

        if (documento.IsError)
        {
            return documento.Errors;
        }

        VentasHandlerHelpers.AgregarLineas(documento.Value, lineas.Value);
        VentasHandlerHelpers.AgregarPagos(documento.Value, pagos.Value, fechaDocumento, _usuarioActual.UsuarioId, _fechaActual.AhoraUtc);

        var confirmar = documento.Value.ConfirmarApartado(0, cajaId, consecutivo.Value);
        if (confirmar.IsError)
        {
            return confirmar.Errors;
        }

        _ = await _eventoService.RegistrarAsync(
            documento.Value.Id,
            "ApartadoCreado",
            $"Apartado {documento.Value.Consecutivo} creado por {documento.Value.TotalComprobante:N2} {documento.Value.MonedaCodigo}",
            payload: new
            {
                consecutivo = documento.Value.Consecutivo,
                total = documento.Value.TotalComprobante,
                moneda = documento.Value.MonedaCodigo,
                clienteId = documento.Value.ClienteId,
                fechaVencimiento = documento.Value.FechaVencimiento,
                pagos = documento.Value.Pagos.Select(p => new
                {
                    pagoId = p.Id,
                    monto = p.MontoAplicadoDocumento,
                    medioPago = p.MedioPagoCodigo,
                    fechaPago = p.FechaPago,
                    fechaRegistroUtc = p.FechaRegistroUtc
                }).ToList()
            },
            cancellationToken: cancellationToken);
        await _documentoRepository.AddAsync(documento.Value, cancellationToken);
        return documento.Value.Id;
    }
}
