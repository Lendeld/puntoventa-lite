using ErrorOr;
using PuntoVenta.Application.DTOs.Inventarios;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Domain.Entities.Cajas;
using PuntoVenta.Domain.Entities.CondicionesVenta;
using PuntoVenta.Domain.Entities.MediosPago;
using PuntoVenta.Domain.Entities.MovimientosStock;
using PuntoVenta.Domain.Entities.Negocios;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.Secuencias;
using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;
using PuntoVenta.Domain.Entities.Tokens;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.UnitTests.Application.Ventas.Fakes;

// ── Helpers de construccion de objetos de dominio ─────────────────────────────

internal static class DominioHelper
{
    private static readonly Guid CajaIdDefault = Guid.NewGuid();

    public static Guid CajaId => CajaIdDefault;

    // Fecha fija que coincide con FakeFechaActual.AhoraUtc — evita skew entre docs y validaciones.
    public static readonly DateTime FechaDocumento = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    public static Producto CrearProducto(decimal precio = 1000m)
        => Producto.Crear("PROD01", "Producto Test", TipoItem.Bien, precio,
            categoriaId: null,
            tarifaIvaImpuestoCodigo: "08",
            noAplicaExistencias: true,
            permiteModificarPrecioUnitario: true).Value;

    public static TarifaIvaImpuesto CrearTarifa(string codigo = "08", decimal porcentaje = 13m)
        => TarifaIvaImpuesto.Crear(codigo, "Tarifa prueba", porcentaje).Value;

    public static MedioPago CrearMedioPago(string codigo = "01", string detalle = "Efectivo")
        => MedioPago.Crear(codigo, detalle).Value;

    public static CondicionVenta CrearCondicionVenta(string codigo = "01", string detalle = "Contado")
        => CondicionVenta.Crear(codigo, detalle).Value;

    public static DocumentoVenta CrearBorradorFactura()
    {
        var doc = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            null, null,
            "01", "Contado",
            FechaDocumento,
            "CRC", 1m);
        return doc.Value;
    }

    public static DocumentoVenta CrearFacturaEmitida(Guid cajaId)
    {
        var doc = CrearBorradorFactura();
        AgregarLineaSimple(doc);
        AgregarPagoEfectivo(doc, "01");
        var emitir = doc.Emitir(cajaId, "FAC-000001");
        if (emitir.IsError) throw new InvalidOperationException($"No se pudo emitir factura: {string.Join(", ", emitir.Errors.Select(e => e.Description))}");
        return doc;
    }

    public static DocumentoVenta CrearFacturaCreditoEmitida(Guid cajaId)
    {
        var clienteId = Guid.NewGuid();
        var doc = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            clienteId,
            null,
            "02", "Credito",
            FechaDocumento,
            "CRC", 1m,
            plazoCreditoDias: 30).Value;
        AgregarLineaSimple(doc);
        var emitir = doc.Emitir(cajaId, "FAC-000002");
        if (emitir.IsError) throw new InvalidOperationException($"No se pudo emitir factura crédito: {string.Join(", ", emitir.Errors.Select(e => e.Description))}");
        return doc;
    }

    public static void AgregarLineaSimple(DocumentoVenta doc, decimal precio = 1000m)
    {
        doc.AgregarLinea(
            Guid.NewGuid(),
            TipoItem.Servicio,
            "SRV01",
            "Servicio test",
            "Unidad",
            1m,
            precio);
    }

    public static void AgregarPagoEfectivo(DocumentoVenta doc, string medioCodigo = "01", decimal monto = 1000m)
    {
        doc.AgregarPago("CRC", 1m, medioCodigo, "Efectivo",
            monto, monto, monto, 0m, 0m);
    }

    public static DocumentoVenta CrearApartadoReservado(Guid cajaId)
    {
        var doc = DocumentoVenta.Crear(
            TipoDocumentoVenta.Apartado,
            null, null,
            "01", "Contado",
            FechaDocumento,
            "CRC", 1m,
            fechaVencimiento: FechaDocumento.AddDays(30)).Value;
        AgregarLineaSimple(doc);
        AgregarPagoEfectivo(doc);
        doc.ConfirmarApartado(0, cajaId, "APA-000001");
        return doc;
    }

    public static DocumentoVenta CrearApartadoReservadoConProducto(Guid cajaId, PuntoVenta.Domain.Entities.Productos.Producto producto)
    {
        var doc = DocumentoVenta.Crear(
            TipoDocumentoVenta.Apartado,
            null, null,
            "01", "Contado",
            FechaDocumento,
            "CRC", 1m,
            fechaVencimiento: FechaDocumento.AddDays(30)).Value;
        doc.AgregarLinea(
            producto.Id,
            producto.TipoItem,
            producto.Codigo,
            producto.Nombre,
            "Unid",
            1m,
            producto.PrecioUnitario,
            noAplicaExistencias: producto.NoAplicaExistencias);
        AgregarPagoEfectivo(doc, monto: producto.PrecioUnitario);
        doc.ConfirmarApartado(0, cajaId, "APA-000002");
        return doc;
    }

    public static DocumentoVenta CrearProformaBorrador(Guid cajaId)
    {
        var doc = DocumentoVenta.Crear(
            TipoDocumentoVenta.Proforma,
            null, null,
            "01", "Contado",
            FechaDocumento,
            "CRC", 1m).Value;
        AgregarLineaSimple(doc);
        doc.NumerarProforma(0, cajaId, "PRO-000001");
        return doc;
    }
}

// ── Fakes de infraestructura ──────────────────────────────────────────────────

internal sealed class FakeFechaActual : IFechaActual
{
    private readonly DateTime _ahora;

    public FakeFechaActual(DateTime? ahora = null)
        => _ahora = ahora ?? new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    public DateTime Ahora => _ahora;
    public DateTime AhoraUtc => _ahora;
    public DateOnly Hoy => DateOnly.FromDateTime(_ahora);
    public DateOnly ALocal(DateTime utc) => DateOnly.FromDateTime(utc);
}

internal sealed class FakeUsuarioActualVentas : IUsuarioActual
{
    public FakeUsuarioActualVentas(Guid? usuarioId = null, string nombreUsuario = "cajero")
    {
        UsuarioId = usuarioId ?? Guid.NewGuid();
        NombreUsuario = nombreUsuario;
    }

    public Guid UsuarioId { get; }
    public string NombreUsuario { get; }
    public bool RequiereCambioPassword => false;
}

internal sealed class FakeDocumentoVentaEventoService : IDocumentoVentaEventoService
{
    public List<EventoDocumentoRegistrado> EventosRegistrados { get; } = [];

    public Task<ErrorOr<Success>> RegistrarAsync(
        Guid documentoVentaId, string tipoEventoCodigo, string resumen,
        object? payload = null, Guid? correlacionId = null, DateTime? ocurridoEn = null,
        CancellationToken cancellationToken = default)
    {
        EventosRegistrados.Add(new EventoDocumentoRegistrado(
            documentoVentaId,
            tipoEventoCodigo,
            resumen,
            payload,
            correlacionId,
            ocurridoEn));
        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }

    public Task<ErrorOr<Success>> RegistrarSistemaAsync(
        Guid negocioId, Guid documentoVentaId, string tipoEventoCodigo, string resumen,
        object? payload = null, Guid? correlacionId = null, DateTime? ocurridoEn = null,
        CancellationToken cancellationToken = default)
    {
        EventosRegistrados.Add(new EventoDocumentoRegistrado(
            documentoVentaId,
            tipoEventoCodigo,
            resumen,
            payload,
            correlacionId,
            ocurridoEn));
        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }
}

internal sealed record EventoDocumentoRegistrado(
    Guid DocumentoVentaId,
    string TipoEventoCodigo,
    string Resumen,
    object? Payload,
    Guid? CorrelacionId,
    DateTime? OcurridoEn);

internal sealed class FakeDocumentoVentaRepository : IDocumentoVentaRepository
{
    private readonly DocumentoVenta? _editable;
    private readonly DocumentoVenta? _detalle;

    public List<DocumentoVenta> Guardados { get; } = [];
    public List<DocumentoVenta> Actualizados { get; } = [];
    public List<DocumentoVentaPago> AbonosRegistrados { get; } = [];
    public List<DocumentoVentaPago> AbonosAnulados { get; } = [];
    public decimal MontoNotasEmitidas { get; set; }
    public IReadOnlyDictionary<Guid, ConsumoNotaCreditoPorProductoDto> ConsumoNotasCreditoPorProducto { get; set; }
        = new Dictionary<Guid, ConsumoNotaCreditoPorProductoDto>();
    public IReadOnlyList<string> NotasDebitoVigentes { get; set; } = [];
    public ReporteMovimientosDineroResultadoDto ReporteMovimientosDinero { get; set; }
        = new([], [], 0m, 0m, 0m);

    public FakeDocumentoVentaRepository(DocumentoVenta? editable = null, DocumentoVenta? detalle = null)
    {
        _editable = editable;
        _detalle = detalle ?? editable;
    }

    public Task<DocumentoVenta?> ObtenerEditableAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_editable);

    public Task<DocumentoVenta?> ObtenerDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_detalle);

    public Task<DocumentoVenta> AddAsync(DocumentoVenta entity, CancellationToken cancellationToken = default)
    {
        Guardados.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(DocumentoVenta entity, CancellationToken cancellationToken = default)
    {
        Actualizados.Add(entity);
        return Task.CompletedTask;
    }

    public Task RegistrarAbonoAsync(DocumentoVenta documento, DocumentoVentaPago pagoNuevo, CancellationToken cancellationToken = default)
    {
        AbonosRegistrados.Add(pagoNuevo);
        return Task.CompletedTask;
    }

    public Task AnularAbonoAsync(DocumentoVenta documento, DocumentoVentaPago pagoAnulado, CancellationToken cancellationToken = default)
    {
        AbonosAnulados.Add(pagoAnulado);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<DocumentoVenta>> ObtenerDocumentosGeneradosAsync(Guid documentoOrigenId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DocumentoVenta>>([]);

    public Task<decimal> ObtenerMontoNotasEmitidasAsync(
        Guid documentoOrigenId,
        TipoDocumentoVenta tipoNota,
        CancellationToken cancellationToken = default)
        => Task.FromResult(MontoNotasEmitidas);

    public Task<IReadOnlyDictionary<Guid, ConsumoNotaCreditoPorProductoDto>> ObtenerConsumoNotasCreditoPorProductoAsync(
        Guid documentoOrigenId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(ConsumoNotasCreditoPorProducto);

    public Task<IReadOnlyList<string>> ObtenerNotasDebitoVigentesAsync(
        Guid documentoOrigenId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(NotasDebitoVigentes);

    public Task<IReadOnlyList<DocumentoVenta>> ObtenerApartadosReservadosVencidosAsync(DateTime ahora, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DocumentoVenta>>([]);

    public Task<IReadOnlyList<DocumentoVenta>> ObtenerFacturasCreditoClienteAsync(Guid clienteId, bool? soloConSaldo, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DocumentoVenta>>([]);

    public Task<(decimal, decimal, int, int)> ObtenerSaldosCreditoClienteAsync(Guid clienteId, DateTime ahora, CancellationToken cancellationToken = default)
        => Task.FromResult((0m, 0m, 0, 0));

    public Task<(IReadOnlyList<DocumentoVenta> Items, int Total)> ObtenerListaCreditoAsync(
        int pagina, int tamano, string? filtroDinamico, Guid? clienteId, bool? soloVencidas,
        DateTime ahora, CancellationToken cancellationToken = default)
        => Task.FromResult<(IReadOnlyList<DocumentoVenta>, int)>(([], 0));

    public Task<ReporteMovimientosDineroResultadoDto> ObtenerReporteMovimientosDineroAsync(
        DateTime fechaDesdeUtc,
        DateTime fechaHastaUtc,
        Guid? cajaId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(ReporteMovimientosDinero);

    public Task<(IReadOnlyList<DocumentoVenta> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina, int tamano, string? filtroDinamico, TipoDocumentoVenta? tipoDocumento,
        EstadoDocumentoVenta? estado, Guid? clienteId, DateTime? fechaDesde, DateTime? fechaHasta,
        CancellationToken cancellationToken = default)
        => Task.FromResult<(IReadOnlyList<DocumentoVenta>, int)>(([], 0));

    public Task<DocumentoVenta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<DocumentoVenta?>(null);

    public Task<IReadOnlyList<DocumentoVenta>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DocumentoVenta>>([]);

    public Task DeleteAsync(DocumentoVenta entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class FakeSecuenciaRepository : ISecuenciaRepository
{
    private readonly Dictionary<TipoDocumentoVenta, Secuencia> _secuencias = [];

    public Task<Secuencia> ObtenerOCrearEditableAsync(TipoDocumentoVenta tipoDocumento, CancellationToken cancellationToken = default)
    {
        if (!_secuencias.TryGetValue(tipoDocumento, out var sec))
        {
            sec = Secuencia.Crear(tipoDocumento).Value;
            _secuencias[tipoDocumento] = sec;
        }
        return Task.FromResult(sec);
    }

    public Task<Secuencia> AddAsync(Secuencia entity, CancellationToken cancellationToken = default)
        => Task.FromResult(entity);

    public Task UpdateAsync(Secuencia entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<Secuencia?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<Secuencia?>(null);

    public Task<IReadOnlyList<Secuencia>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Secuencia>>([]);

    public Task DeleteAsync(Secuencia entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class FakeNegocioRepository : INegocioRepository
{
    private readonly bool _aplicaCajas;

    public FakeNegocioRepository(bool aplicaCajas = false) => _aplicaCajas = aplicaCajas;

    public Task<Negocio?> ObtenerAsync(CancellationToken cancellationToken = default)
    {
        var negocio = Negocio.Crear("Negocio Test", aplicaGestionCajas: _aplicaCajas).Value;
        return Task.FromResult<Negocio?>(negocio);
    }

    public Task<Negocio?> ObtenerEditableAsync(CancellationToken cancellationToken = default)
        => ObtenerAsync(cancellationToken);

    public Task<Negocio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<Negocio?>(null);

    public Task<IReadOnlyList<Negocio>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Negocio>>([]);

    public Task<Negocio> AddAsync(Negocio entity, CancellationToken cancellationToken = default)
        => Task.FromResult(entity);

    public Task UpdateAsync(Negocio entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteAsync(Negocio entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class FakeNegocioTicketConfigRepository : INegocioTicketConfigRepository
{
    private readonly NegocioTicketConfig? _config;

    public FakeNegocioTicketConfigRepository(NegocioTicketConfig? config = null)
        => _config = config;

    public Task<NegocioTicketConfig?> ObtenerAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_config);

    public Task<NegocioTicketConfig?> ObtenerEditableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_config);

    public Task<NegocioTicketConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<NegocioTicketConfig?>(_config);

    public Task<IReadOnlyList<NegocioTicketConfig>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NegocioTicketConfig>>(_config is null ? [] : [_config]);

    public Task<NegocioTicketConfig> AddAsync(NegocioTicketConfig entity, CancellationToken cancellationToken = default)
        => Task.FromResult(entity);

    public Task UpdateAsync(NegocioTicketConfig entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteAsync(NegocioTicketConfig entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class FakeMedioPagoRepository : IMedioPagoRepository
{
    private readonly List<MedioPago> _medios;

    public FakeMedioPagoRepository(params MedioPago[] medios)
        => _medios = [.. medios];

    public Task<IReadOnlyList<MedioPago>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<MedioPago>>(_medios);

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => Task.FromResult(_medios.Any(m => m.Codigo == codigo));

    public Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_medios.Select(m => m.Codigo).ToHashSet());

    public Task<MedioPago?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<MedioPago?>(null);

    public Task<IReadOnlyList<MedioPago>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<MedioPago>>(_medios);

    public Task<MedioPago> AddAsync(MedioPago entity, CancellationToken cancellationToken = default)
        => Task.FromResult(entity);

    public Task UpdateAsync(MedioPago entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteAsync(MedioPago entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class FakeProductoRepository : IProductoRepository
{
    private readonly List<Producto> _productos;

    public FakeProductoRepository(params Producto[] productos)
        => _productos = [.. productos];

    public Task<Producto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = _productos.FirstOrDefault(p => p.Id == id);
        if (producto is null && _productos.Count == 1)
        {
            producto = _productos[0];
        }

        return Task.FromResult(producto);
    }

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<bool> ExisteCodigoExcluyendoAsync(string codigo, Guid excludeId, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<bool> ExisteCodigoBarrasExcluyendoAsync(string codigoBarras, Guid excludeId, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default)
        => Task.FromResult<Producto?>(null);

    public Task<Producto?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<Producto?>(null);

    public Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Producto>>(_productos.Where(p => ids.Contains(p.Id)).ToList());

    public Task<IReadOnlyList<Producto>> ObtenerPorIdsEditablesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Producto>>(_productos.Where(p => ids.Contains(p.Id)).ToList());

    public Task<Producto?> ObtenerEditableAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_productos.FirstOrDefault(p => p.Id == id));

    public Task<(IReadOnlyList<Producto> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina, int tamano, string? filtroDinamico, TipoItem? tipoItem, Guid? categoriaId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<(IReadOnlyList<Producto>, int)>(([], 0));

    public Task<IReadOnlyList<InventarioReporteProyeccionDto>> ObtenerReporteInventarioProyectadoAsync(
        string? codigo, Guid? categoriaId, Guid? proveedorId, int maxFilas, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<InventarioReporteProyeccionDto>>([]);

    public Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Producto>>(_productos);

    public Task<Producto> AddAsync(Producto entity, CancellationToken cancellationToken = default)
        => Task.FromResult(entity);

    public Task UpdateAsync(Producto entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteAsync(Producto entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class FakeTarifaRepository : ITarifaIvaImpuestoRepository
{
    private readonly List<TarifaIvaImpuesto> _tarifas;

    public FakeTarifaRepository(params TarifaIvaImpuesto[] tarifas)
        => _tarifas = [.. tarifas];

    public Task<IReadOnlyList<TarifaIvaImpuesto>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TarifaIvaImpuesto>>(_tarifas);

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => Task.FromResult(_tarifas.Any(t => t.Codigo == codigo));

    public Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_tarifas.Select(t => t.Codigo).ToHashSet());

    public Task<TarifaIvaImpuesto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<TarifaIvaImpuesto?>(null);

    public Task<IReadOnlyList<TarifaIvaImpuesto>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TarifaIvaImpuesto>>(_tarifas);

    public Task<TarifaIvaImpuesto> AddAsync(TarifaIvaImpuesto entity, CancellationToken cancellationToken = default)
        => Task.FromResult(entity);

    public Task UpdateAsync(TarifaIvaImpuesto entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteAsync(TarifaIvaImpuesto entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class FakeCondicionVentaRepository : ICondicionVentaRepository
{
    private readonly List<CondicionVenta> _condiciones;

    public FakeCondicionVentaRepository(params CondicionVenta[] condiciones)
        => _condiciones = [.. condiciones];

    public Task<IReadOnlyList<CondicionVenta>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<CondicionVenta>>(_condiciones);

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => Task.FromResult(_condiciones.Any(c => c.Codigo == codigo));

    public Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_condiciones.Select(c => c.Codigo).ToHashSet());

    public Task<CondicionVenta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<CondicionVenta?>(null);

    public Task<IReadOnlyList<CondicionVenta>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<CondicionVenta>>(_condiciones);

    public Task<CondicionVenta> AddAsync(CondicionVenta entity, CancellationToken cancellationToken = default)
        => Task.FromResult(entity);

    public Task UpdateAsync(CondicionVenta entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteAsync(CondicionVenta entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class FakeClienteRepositoryVentas : IClienteRepository
{
    private readonly bool _clienteExiste;

    public FakeClienteRepositoryVentas(bool clienteExiste = true)
        => _clienteExiste = clienteExiste;

    public Task<PuntoVenta.Domain.Entities.Clientes.Cliente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_clienteExiste ? PuntoVenta.Domain.Entities.Clientes.Cliente.Crear("Juan Pérez").Value : null);

    public Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<PuntoVenta.Domain.Entities.Clientes.Cliente?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<PuntoVenta.Domain.Entities.Clientes.Cliente?>(null);

    public Task<(IReadOnlyList<PuntoVenta.Domain.Entities.Clientes.Cliente> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default)
        => Task.FromResult<(IReadOnlyList<PuntoVenta.Domain.Entities.Clientes.Cliente>, int)>(([], 0));

    public Task<IReadOnlyList<PuntoVenta.Domain.Entities.Clientes.Cliente>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PuntoVenta.Domain.Entities.Clientes.Cliente>>([]);

    public Task<PuntoVenta.Domain.Entities.Clientes.Cliente> AddAsync(PuntoVenta.Domain.Entities.Clientes.Cliente entity, CancellationToken cancellationToken = default)
        => Task.FromResult(entity);

    public Task UpdateAsync(PuntoVenta.Domain.Entities.Clientes.Cliente entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteAsync(PuntoVenta.Domain.Entities.Clientes.Cliente entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class FakeMovimientoStockRepository : IMovimientoStockRepository
{
    public List<MovimientoStock> Guardados { get; } = [];

    public Task<MovimientoStock> AddAsync(MovimientoStock entity, CancellationToken cancellationToken = default)
    {
        Guardados.Add(entity);
        return Task.FromResult(entity);
    }

    public Task AgregarRangoSinPersistirAsync(IReadOnlyList<MovimientoStock> movimientos, CancellationToken cancellationToken = default)
    {
        Guardados.AddRange(movimientos);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(MovimientoStock entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteAsync(MovimientoStock entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<MovimientoStock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<MovimientoStock?>(null);

    public Task<IReadOnlyList<MovimientoStock>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<MovimientoStock>>([]);

    public Task<(IReadOnlyList<(MovimientoStock Movimiento, string NombreProducto)> Items, int Total)> ObtenerPaginadoAsync(
        Guid? productoId, int pagina, int tamano, CancellationToken cancellationToken = default)
        => Task.FromResult<(IReadOnlyList<(MovimientoStock, string)>, int)>(([], 0));
}

internal sealed class FakeTransactionScope : ITransactionScope
{
    public bool Committed { get; private set; }
    public bool RolledBack { get; private set; }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Committed = true;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        RolledBack = true;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public FakeTransactionScope LastTransaction { get; private set; } = new();

    public Task<ITransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        LastTransaction = new FakeTransactionScope();
        return Task.FromResult<ITransactionScope>(LastTransaction);
    }
}

internal sealed class FakeVendedorRepositoryVentas : IVendedorRepository
{
    public Task<PuntoVenta.Domain.Entities.Vendedores.Vendedor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<PuntoVenta.Domain.Entities.Vendedores.Vendedor?>(null);

    public Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<IReadOnlyList<PuntoVenta.Domain.Entities.Vendedores.Vendedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PuntoVenta.Domain.Entities.Vendedores.Vendedor>>([]);

    public Task<PuntoVenta.Domain.Entities.Vendedores.Vendedor?> ObtenerPrincipalAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<PuntoVenta.Domain.Entities.Vendedores.Vendedor?>(null);

    public Task<PuntoVenta.Domain.Entities.Vendedores.Vendedor?> ObtenerPrincipalEditableAsync(Guid? excludeId = null, CancellationToken cancellationToken = default)
        => Task.FromResult<PuntoVenta.Domain.Entities.Vendedores.Vendedor?>(null);

    public Task<PuntoVenta.Domain.Entities.Vendedores.Vendedor?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<PuntoVenta.Domain.Entities.Vendedores.Vendedor?>(null);

    public Task<(IReadOnlyList<PuntoVenta.Domain.Entities.Vendedores.Vendedor> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default)
        => Task.FromResult<(IReadOnlyList<PuntoVenta.Domain.Entities.Vendedores.Vendedor>, int)>(([], 0));

    public Task<IReadOnlyList<PuntoVenta.Domain.Entities.Vendedores.Vendedor>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PuntoVenta.Domain.Entities.Vendedores.Vendedor>>([]);

    public Task<PuntoVenta.Domain.Entities.Vendedores.Vendedor> AddAsync(PuntoVenta.Domain.Entities.Vendedores.Vendedor entity, CancellationToken cancellationToken = default)
        => Task.FromResult(entity);

    public Task UpdateAsync(PuntoVenta.Domain.Entities.Vendedores.Vendedor entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteAsync(PuntoVenta.Domain.Entities.Vendedores.Vendedor entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
