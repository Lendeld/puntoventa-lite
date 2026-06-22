using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record CrearFacturaCommand(
    Guid? ClienteId,
    Guid? VendedorId,
    string CondicionVentaCodigo,
    IReadOnlyList<DocumentoVentaLineaCommand> Lineas,
    IReadOnlyList<DocumentoVentaPagoCommand> Pagos,
    int? PlazoCreditoDias = null,
    DateTime? FechaDocumento = null,
    string MonedaCodigo = "CRC",
    decimal TipoCambio = 1m,
    string? Observaciones = null) : IRequest<ErrorOr<Guid>>;

/// <summary>
/// Crea y emite una factura en una sola operación: arma el documento con sus
/// líneas y pagos, asigna consecutivo, descuenta stock y lo deja Emitido.
/// Lite no tiene borrador editable, así que no hay paso intermedio.
/// </summary>
public sealed class CrearFacturaHandler(
    IFechaActual fechaActual,
    IUsuarioActual usuarioActual,
    IUnitOfWork unitOfWork,
    IDocumentoVentaRepository documentoRepository,
    ICondicionVentaRepository condicionVentaRepository,
    IClienteRepository clienteRepository,
    IVendedorRepository vendedorRepository,
    IMedioPagoRepository medioPagoRepository,
    IProductoRepository productoRepository,
    ITarifaIvaImpuestoRepository tarifaRepository,
    ISecuenciaRepository secuenciaRepository,
    IMovimientoStockRepository movimientoStockRepository,
    IDocumentoVentaEventoService eventoService) : IRequestHandler<CrearFacturaCommand, ErrorOr<Guid>>
{
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly ICondicionVentaRepository _condicionVentaRepository = condicionVentaRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IVendedorRepository _vendedorRepository = vendedorRepository;
    private readonly IMedioPagoRepository _medioPagoRepository = medioPagoRepository;
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly ITarifaIvaImpuestoRepository _tarifaRepository = tarifaRepository;
    private readonly ISecuenciaRepository _secuenciaRepository = secuenciaRepository;
    private readonly IMovimientoStockRepository _movimientoStockRepository = movimientoStockRepository;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearFacturaCommand command, CancellationToken cancellationToken)
    {
        if (!command.ClienteId.HasValue && CondicionVentaCodigos.EsCredito(command.CondicionVentaCodigo))
        {
            return DocumentoVentaErrors.CreditoRequiereCliente;
        }

        var condicion = await VentasHandlerHelpers.ObtenerCondicionVentaAsync(command.CondicionVentaCodigo, _condicionVentaRepository, cancellationToken);
        if (condicion.IsError)
        {
            return condicion.Errors;
        }

        if (command.ClienteId.HasValue && await _clienteRepository.GetByIdAsync(command.ClienteId.Value, cancellationToken) is null)
        {
            return Error.Validation("DocumentoVenta_ClienteId", "El cliente indicado no existe.");
        }

        if (command.VendedorId.HasValue && await _vendedorRepository.GetByIdAsync(command.VendedorId.Value, cancellationToken) is null)
        {
            return Error.Validation("DocumentoVenta_VendedorId", "El vendedor indicado no existe.");
        }

        var lineas = await VentasHandlerHelpers.PrepararLineasAsync(
            command.Lineas,
            _productoRepository,
            _tarifaRepository,
            true,
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

        var documentoResult = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            command.ClienteId,
            command.VendedorId,
            condicion.Value.Codigo,
            condicion.Value.Detalle,
            VentasHandlerHelpers.NormalizarFechaUtc(command.FechaDocumento),
            command.MonedaCodigo,
            command.TipoCambio,
            command.PlazoCreditoDias,
            command.Observaciones);
        if (documentoResult.IsError)
        {
            return documentoResult.Errors;
        }

        var documento = documentoResult.Value;
        VentasHandlerHelpers.AgregarLineas(documento, lineas.Value);
        VentasHandlerHelpers.AgregarPagos(documento, pagos.Value);

        // El consecutivo persiste su propia secuencia, así que envolvemos el alta
        // del documento + la emisión en una transacción para mantener atomicidad.
        await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        await _documentoRepository.AddAsync(documento, cancellationToken);

        var consecutivo = await VentasHandlerHelpers.SiguienteConsecutivoAsync(
            TipoDocumentoVenta.Factura,
            _secuenciaRepository,
            cancellationToken);
        if (consecutivo.IsError)
        {
            await tx.RollbackAsync(cancellationToken);
            return consecutivo.Errors;
        }

        var emitir = documento.Emitir(cajaId: null, consecutivo.Value);
        if (emitir.IsError)
        {
            await tx.RollbackAsync(cancellationToken);
            return emitir.Errors;
        }

        _ = await _eventoService.RegistrarAsync(
            documento.Id,
            "FacturaEmitida",
            $"Factura {documento.Consecutivo} emitida por {documento.TotalComprobante:N2} {documento.MonedaCodigo}",
            payload: new
            {
                consecutivo = documento.Consecutivo,
                total = documento.TotalComprobante,
                moneda = documento.MonedaCodigo,
                clienteId = documento.ClienteId,
                esCredito = documento.EsCredito,
                pagos = documento.Pagos.Select(p => new
                {
                    pagoId = p.Id,
                    monto = p.MontoAplicadoDocumento,
                    medioPago = p.MedioPagoCodigo,
                    fechaPago = p.FechaPago,
                    fechaRegistroUtc = p.FechaRegistroUtc
                }).ToList()
            },
            cancellationToken: cancellationToken);

        await VentasHandlerHelpers.AplicarMovimientosStockAsync(
            documento.Lineas,
            documento,
            deltaEsNegativo: true,
            _productoRepository,
            _movimientoStockRepository,
            _fechaActual.AhoraUtc,
            _usuarioActual.UsuarioId,
            cancellationToken);

        await _documentoRepository.UpdateAsync(documento, cancellationToken);

        await tx.CommitAsync(cancellationToken);
        return documento.Id;
    }
}
