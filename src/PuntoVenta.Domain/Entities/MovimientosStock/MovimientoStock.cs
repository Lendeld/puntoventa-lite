using ErrorOr;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Domain.Entities.MovimientosStock;

public sealed class MovimientoStock : BaseAuditableEntity
{
    public const int RazonMaxLength = 255;

    private MovimientoStock() { }

    public Guid ProductoId { get; private set; }
    public DateTime FechaUtc { get; private set; }

    // Referencia al documento origen (puede ser null en ajustes manuales)
    public TipoDocumentoVenta? TipoDocumentoOrigen { get; private set; }
    public Guid? DocumentoVentaId { get; private set; }
    public string? ConsecutivoDocumento { get; private set; }

    // Cuánto cambió el stock (positivo = ingreso, negativo = salida)
    public decimal Delta { get; private set; }

    // Saldo tras aplicar el delta
    public decimal SaldoResultante { get; private set; }

    public Guid? UsuarioId { get; private set; }

    public string? Razon { get; private set; }

    public static ErrorOr<MovimientoStock> Crear(
        Guid productoId,
        DateTime fechaUtc,
        decimal delta,
        decimal saldoResultante,
        Guid? usuarioId,
        TipoDocumentoVenta? tipoDocumentoOrigen = null,
        Guid? documentoVentaId = null,
        string? consecutivoDocumento = null,
        string? razon = null)
    {
        var errores = new List<Error>();

        if (productoId == Guid.Empty)
            errores.Add(MovimientoStockErrors.ProductoRequerido);

        if (delta == 0)
            errores.Add(MovimientoStockErrors.DeltaCero);

        if (razon is not null && razon.Trim().Length > RazonMaxLength)
            errores.Add(MovimientoStockErrors.RazonExcedeLongitud);

        if (errores.Count > 0) return errores;

        return new MovimientoStock
        {
            ProductoId = productoId,
            FechaUtc = fechaUtc,
            Delta = delta,
            SaldoResultante = saldoResultante,
            UsuarioId = usuarioId,
            TipoDocumentoOrigen = tipoDocumentoOrigen,
            DocumentoVentaId = documentoVentaId,
            ConsecutivoDocumento = consecutivoDocumento?.Trim(),
            Razon = string.IsNullOrWhiteSpace(razon) ? null : razon.Trim()
        };
    }
}
