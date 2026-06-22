using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record CrearProformaCommand(
    Guid? ClienteId,
    Guid? VendedorId,
    string CondicionVentaCodigo,
    IReadOnlyList<DocumentoVentaLineaCommand> Lineas,
    int? PlazoCreditoDias = null,
    DateTime? FechaDocumento = null,
    string MonedaCodigo = "CRC",
    decimal TipoCambio = 1m,
    string? Observaciones = null) : IRequest<ErrorOr<Guid>>;

public sealed record ActualizarProformaCommand(
    Guid Id,
    Guid? ClienteId,
    Guid? VendedorId,
    string CondicionVentaCodigo,
    IReadOnlyList<DocumentoVentaLineaCommand> Lineas,
    int? PlazoCreditoDias = null,
    DateTime? FechaDocumento = null,
    string MonedaCodigo = "CRC",
    decimal TipoCambio = 1m,
    string? Observaciones = null) : IRequest<ErrorOr<Guid>>;

public sealed record FacturarProformaCommand(
    Guid Id,
    IReadOnlyList<DocumentoVentaPagoCommand> Pagos,
    Guid? CajaId = null) : IRequest<ErrorOr<Guid>>;

public sealed class CrearProformaHandler(
    IDocumentoVentaRepository documentoRepository,
    ICondicionVentaRepository condicionVentaRepository,
    IClienteRepository clienteRepository,
    IVendedorRepository vendedorRepository,
    IProductoRepository productoRepository,
    ITarifaIvaImpuestoRepository tarifaRepository,
    ISecuenciaRepository secuenciaRepository) : IRequestHandler<CrearProformaCommand, ErrorOr<Guid>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly ICondicionVentaRepository _condicionVentaRepository = condicionVentaRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IVendedorRepository _vendedorRepository = vendedorRepository;
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly ITarifaIvaImpuestoRepository _tarifaRepository = tarifaRepository;
    private readonly ISecuenciaRepository _secuenciaRepository = secuenciaRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearProformaCommand command, CancellationToken cancellationToken)
    {
        var condicion = await VentasHandlerHelpers.ObtenerCondicionVentaAsync(
            command.CondicionVentaCodigo, _condicionVentaRepository, cancellationToken);
        if (condicion.IsError) return condicion.Errors;

        if (command.ClienteId.HasValue && await _clienteRepository.GetByIdAsync(command.ClienteId.Value, cancellationToken) is null)
            return Error.Validation("DocumentoVenta_ClienteId", "El cliente indicado no existe.");

        if (command.VendedorId.HasValue && await _vendedorRepository.GetByIdAsync(command.VendedorId.Value, cancellationToken) is null)
            return Error.Validation("DocumentoVenta_VendedorId", "El vendedor indicado no existe.");

        var lineas = await VentasHandlerHelpers.PrepararLineasAsync(
            command.Lineas, _productoRepository, _tarifaRepository,
            false, command.MonedaCodigo, command.TipoCambio, cancellationToken);
        if (lineas.IsError) return lineas.Errors;

        var documento = DocumentoVenta.Crear(
            TipoDocumentoVenta.Proforma,
            command.ClienteId,
            command.VendedorId,
            condicion.Value.Codigo,
            condicion.Value.Detalle,
            VentasHandlerHelpers.NormalizarFechaUtc(command.FechaDocumento),
            command.MonedaCodigo,
            command.TipoCambio,
            command.PlazoCreditoDias,
            command.Observaciones);
        if (documento.IsError) return documento.Errors;

        VentasHandlerHelpers.AgregarLineas(documento.Value, lineas.Value);

        var numConsecutivo = await VentasHandlerHelpers.SiguienteConsecutivoAsync(
            TipoDocumentoVenta.Proforma, _secuenciaRepository, cancellationToken);
        if (numConsecutivo.IsError) return numConsecutivo.Errors;

        var numerar = documento.Value.NumerarProforma(0, null, numConsecutivo.Value);
        if (numerar.IsError) return numerar.Errors;

        await _documentoRepository.AddAsync(documento.Value, cancellationToken);
        return documento.Value.Id;
    }
}

public sealed class ActualizarProformaHandler(
    IDocumentoVentaRepository documentoRepository,
    ICondicionVentaRepository condicionVentaRepository,
    IClienteRepository clienteRepository,
    IVendedorRepository vendedorRepository,
    IProductoRepository productoRepository,
    ITarifaIvaImpuestoRepository tarifaRepository) : IRequestHandler<ActualizarProformaCommand, ErrorOr<Guid>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly ICondicionVentaRepository _condicionVentaRepository = condicionVentaRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IVendedorRepository _vendedorRepository = vendedorRepository;
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly ITarifaIvaImpuestoRepository _tarifaRepository = tarifaRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(ActualizarProformaCommand command, CancellationToken cancellationToken)
    {
        var proforma = await _documentoRepository.ObtenerEditableAsync(command.Id, cancellationToken);
        if (proforma is null) return DocumentoVentaErrors.NoEncontrado;

        if (proforma.TipoDocumento != TipoDocumentoVenta.Proforma || proforma.Estado != EstadoDocumentoVenta.Borrador)
            return DocumentoVentaErrors.DocumentoNoEditable;

        if (command.ClienteId.HasValue && await _clienteRepository.GetByIdAsync(command.ClienteId.Value, cancellationToken) is null)
            return Error.Validation("DocumentoVenta_ClienteId", "El cliente indicado no existe.");

        if (command.VendedorId.HasValue && await _vendedorRepository.GetByIdAsync(command.VendedorId.Value, cancellationToken) is null)
            return Error.Validation("DocumentoVenta_VendedorId", "El vendedor indicado no existe.");

        var condicion = await VentasHandlerHelpers.ObtenerCondicionVentaAsync(
            command.CondicionVentaCodigo, _condicionVentaRepository, cancellationToken);
        if (condicion.IsError) return condicion.Errors;

        var lineas = await VentasHandlerHelpers.PrepararLineasAsync(
            command.Lineas, _productoRepository, _tarifaRepository,
            false, command.MonedaCodigo, command.TipoCambio, cancellationToken);
        if (lineas.IsError) return lineas.Errors;

        var encabezado = proforma.ActualizarEncabezado(
            command.ClienteId,
            command.VendedorId,
            condicion.Value.Codigo,
            condicion.Value.Detalle,
            VentasHandlerHelpers.NormalizarFechaUtc(command.FechaDocumento),
            command.MonedaCodigo,
            command.TipoCambio,
            command.PlazoCreditoDias,
            command.Observaciones);
        if (encabezado.IsError) return encabezado.Errors;

        var idsExistentesAntesDeEditar = proforma.Lineas.Select(l => l.Id).ToHashSet();
        var idsVigentes = new HashSet<Guid>();

        for (var index = 0; index < lineas.Value.Count; index++)
        {
            var commandLinea = command.Lineas[index];
            var linea = lineas.Value[index];

            if (commandLinea.Id.HasValue && idsExistentesAntesDeEditar.Contains(commandLinea.Id.Value))
            {
                var actualizar = proforma.ActualizarLinea(
                    commandLinea.Id.Value,
                    linea.ProductoId,
                    linea.TipoItem,
                    linea.Codigo,
                    linea.Descripcion,
                    linea.UnidadMedidaCodigo,
                    linea.Cantidad,
                    linea.PrecioUnitario,
                    linea.MontoDescuento,
                    linea.TarifaIvaImpuestoCodigo,
                    linea.PorcentajeImpuesto,
                    linea.DevuelveInventario,
                    linea.NoAplicaExistencias,
                    linea.PermiteModificarPrecioUnitario);
                if (actualizar.IsError) return actualizar.Errors;
                idsVigentes.Add(commandLinea.Id.Value);
            }
            else
            {
                var idsAntes = proforma.Lineas.Select(l => l.Id).ToHashSet();
                var agregar = proforma.AgregarLinea(
                    linea.ProductoId,
                    linea.TipoItem,
                    linea.Codigo,
                    linea.Descripcion,
                    linea.UnidadMedidaCodigo,
                    linea.Cantidad,
                    linea.PrecioUnitario,
                    linea.MontoDescuento,
                    linea.TarifaIvaImpuestoCodigo,
                    linea.PorcentajeImpuesto,
                    linea.DevuelveInventario,
                    linea.NoAplicaExistencias,
                    linea.PermiteModificarPrecioUnitario);
                if (agregar.IsError) return agregar.Errors;
                var nuevaId = proforma.Lineas.First(l => !idsAntes.Contains(l.Id)).Id;
                idsVigentes.Add(nuevaId);
            }
        }

        var remover = proforma.RemoverLineasExcepto(idsVigentes);
        if (remover.IsError) return remover.Errors;

        await _documentoRepository.UpdateAsync(proforma, cancellationToken);
        return proforma.Id;
    }
}

public sealed class FacturarProformaHandler(
    IDocumentoVentaRepository documentoRepository,
    IMedioPagoRepository medioPagoRepository,
    ISecuenciaRepository secuenciaRepository,
    INegocioRepository negocioRepository,
    IDocumentoVentaEventoService eventoService) : IRequestHandler<FacturarProformaCommand, ErrorOr<Guid>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly IMedioPagoRepository _medioPagoRepository = medioPagoRepository;
    private readonly ISecuenciaRepository _secuenciaRepository = secuenciaRepository;
    private readonly INegocioRepository _negocioRepository = negocioRepository;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;

    public async ValueTask<ErrorOr<Guid>> Handle(FacturarProformaCommand command, CancellationToken cancellationToken)
    {
        var proforma = await _documentoRepository.ObtenerEditableAsync(command.Id, cancellationToken);
        if (proforma is null) return DocumentoVentaErrors.NoEncontrado;

        if (proforma.TipoDocumento != TipoDocumentoVenta.Proforma || proforma.Estado != EstadoDocumentoVenta.Borrador)
            return DocumentoVentaErrors.DocumentoNoConvertible;

        var pagos = await VentasHandlerHelpers.PrepararPagosAsync(command.Pagos, _medioPagoRepository, cancellationToken);
        if (pagos.IsError) return pagos.Errors;

        var factura = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            proforma.ClienteId,
            proforma.VendedorId,
            proforma.CondicionVentaCodigo,
            proforma.CondicionVentaDetalleSnapshot,
            proforma.FechaDocumento,
            proforma.MonedaCodigo,
            proforma.TipoCambio,
            proforma.PlazoCreditoDias,
            proforma.Observaciones,
            proforma.Id);
        if (factura.IsError) return factura.Errors;

        var lineasSnapshot = VentasHandlerHelpers.PrepararLineasDesdeSnapshot(proforma);
        VentasHandlerHelpers.AgregarLineas(factura.Value, lineasSnapshot);
        VentasHandlerHelpers.AgregarPagos(factura.Value, pagos.Value);

        var negocio = await _negocioRepository.ObtenerAsync(cancellationToken);
        Guid? cajaId = negocio?.AplicaCajas == true ? command.CajaId : null;

        var consecutivo = await VentasHandlerHelpers.SiguienteConsecutivoAsync(
            TipoDocumentoVenta.Factura, _secuenciaRepository, cancellationToken);
        if (consecutivo.IsError) return consecutivo.Errors;

        var emitir = factura.Value.Emitir(cajaId, consecutivo.Value);
        if (emitir.IsError) return emitir.Errors;

        var convertir = proforma.MarcarConvertido();
        if (convertir.IsError) return convertir.Errors;

        _ = await _eventoService.RegistrarAsync(
            factura.Value.Id,
            "FacturaEmitidaDesdeProforma",
            $"Factura {factura.Value.Consecutivo} emitida desde proforma {proforma.Consecutivo}",
            payload: new
            {
                consecutivo = factura.Value.Consecutivo,
                origenProformaId = proforma.Id,
                origenProformaConsecutivo = proforma.Consecutivo,
                total = factura.Value.TotalComprobante,
                moneda = factura.Value.MonedaCodigo,
                pagos = factura.Value.Pagos.Select(p => new
                {
                    pagoId = p.Id,
                    monto = p.MontoAplicadoDocumento,
                    medioPago = p.MedioPagoCodigo,
                    fechaPago = p.FechaPago,
                    fechaRegistroUtc = p.FechaRegistroUtc
                }).ToList()
            },
            cancellationToken: cancellationToken);

        await _documentoRepository.AddAsync(factura.Value, cancellationToken);
        await _documentoRepository.UpdateAsync(proforma, cancellationToken);
        return factura.Value.Id;
    }
}
