using ErrorOr;
using PuntoVenta.Domain.Common;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Domain.Entities.Ventas;

public sealed class DocumentoVentaLinea
{
    public const int CodigoMaxLength = 50;
    public const int DescripcionMaxLength = 500;
    public const int UnidadMedidaCodigoMaxLength = 10;
    public const int TarifaIvaCodigoMaxLength = 2;

    private DocumentoVentaLinea() { }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DocumentoVentaId { get; private set; }
    public DocumentoVenta? DocumentoVenta { get; private set; }
    public Guid? ProductoId { get; private set; }
    public Producto? Producto { get; private set; }
    public TipoItem TipoItem { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string UnidadMedidaCodigo { get; private set; } = "Unidad";
    public string? TarifaIvaImpuestoCodigo { get; private set; }
    public decimal PorcentajeImpuesto { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal MontoDescuento { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal MontoImpuesto { get; private set; }
    public decimal TotalLinea { get; private set; }
    public bool DevuelveInventario { get; private set; }
    public bool NoAplicaExistencias { get; private set; }
    public bool PermiteModificarPrecioUnitario { get; private set; }

    public static ErrorOr<DocumentoVentaLinea> Crear(
        Guid documentoVentaId,
        Guid? productoId,
        TipoItem tipoItem,
        string codigo,
        string descripcion,
        string unidadMedidaCodigo,
        decimal cantidad,
        decimal precioUnitario,
        decimal montoDescuento = 0,
        string? tarifaIvaImpuestoCodigo = null,
        decimal porcentajeImpuesto = 0,
        bool devuelveInventario = false,
        bool noAplicaExistencias = false,
        bool permiteModificarPrecioUnitario = false)
    {
        var errores = Validar(
            productoId,
            tipoItem,
            codigo,
            descripcion,
            unidadMedidaCodigo,
            cantidad,
            precioUnitario,
            montoDescuento,
            tarifaIvaImpuestoCodigo,
            porcentajeImpuesto);

        if (errores.Count > 0)
        {
            return errores;
        }

        var (precio, subtotal, impuesto, total) = CalcularMontos(
            cantidad, precioUnitario, montoDescuento, porcentajeImpuesto);

        return new DocumentoVentaLinea
        {
            DocumentoVentaId = documentoVentaId,
            ProductoId = productoId,
            TipoItem = tipoItem,
            Codigo = codigo.Trim(),
            Descripcion = descripcion.Trim(),
            UnidadMedidaCodigo = string.IsNullOrWhiteSpace(unidadMedidaCodigo) ? "Unidad" : unidadMedidaCodigo.Trim(),
            TarifaIvaImpuestoCodigo = string.IsNullOrWhiteSpace(tarifaIvaImpuestoCodigo) ? null : tarifaIvaImpuestoCodigo.Trim(),
            PorcentajeImpuesto = porcentajeImpuesto,
            Cantidad = cantidad,
            PrecioUnitario = precio,
            MontoDescuento = montoDescuento,
            Subtotal = subtotal,
            MontoImpuesto = impuesto,
            TotalLinea = total,
            DevuelveInventario = devuelveInventario,
            NoAplicaExistencias = noAplicaExistencias,
            PermiteModificarPrecioUnitario = permiteModificarPrecioUnitario
        };
    }

    public ErrorOr<Success> Actualizar(
        Guid? productoId,
        TipoItem tipoItem,
        string codigo,
        string descripcion,
        string unidadMedidaCodigo,
        decimal cantidad,
        decimal precioUnitario,
        decimal montoDescuento = 0,
        string? tarifaIvaImpuestoCodigo = null,
        decimal porcentajeImpuesto = 0,
        bool devuelveInventario = false,
        bool noAplicaExistencias = false,
        bool permiteModificarPrecioUnitario = false)
    {
        var errores = Validar(
            productoId,
            tipoItem,
            codigo,
            descripcion,
            unidadMedidaCodigo,
            cantidad,
            precioUnitario,
            montoDescuento,
            tarifaIvaImpuestoCodigo,
            porcentajeImpuesto);

        if (errores.Count > 0)
        {
            return errores;
        }

        var (precio, subtotal, impuesto, total) = CalcularMontos(
            cantidad, precioUnitario, montoDescuento, porcentajeImpuesto);

        ProductoId = productoId;
        TipoItem = tipoItem;
        Codigo = codigo.Trim();
        Descripcion = descripcion.Trim();
        UnidadMedidaCodigo = string.IsNullOrWhiteSpace(unidadMedidaCodigo) ? "Unidad" : unidadMedidaCodigo.Trim();
        TarifaIvaImpuestoCodigo = string.IsNullOrWhiteSpace(tarifaIvaImpuestoCodigo) ? null : tarifaIvaImpuestoCodigo.Trim();
        PorcentajeImpuesto = porcentajeImpuesto;
        Cantidad = cantidad;
        PrecioUnitario = precio;
        MontoDescuento = montoDescuento;
        Subtotal = subtotal;
        MontoImpuesto = impuesto;
        TotalLinea = total;
        DevuelveInventario = devuelveInventario;
        NoAplicaExistencias = noAplicaExistencias;
        PermiteModificarPrecioUnitario = permiteModificarPrecioUnitario;

        return Result.Success;
    }

    private static (decimal Precio, decimal Subtotal, decimal Impuesto, decimal Total) CalcularMontos(
        decimal cantidad,
        decimal precioUnitario,
        decimal montoDescuento,
        decimal porcentajeImpuesto)
    {
        var precio = Dinero.Redondear(precioUnitario);
        var bruto = Dinero.Redondear(cantidad * precio);
        var subtotal = Dinero.Redondear(bruto - montoDescuento);
        var impuesto = Dinero.Redondear(subtotal * porcentajeImpuesto / 100m);
        return (precio, subtotal, impuesto, subtotal + impuesto);
    }

    private static List<Error> Validar(
        Guid? productoId,
        TipoItem tipoItem,
        string codigo,
        string descripcion,
        string unidadMedidaCodigo,
        decimal cantidad,
        decimal precioUnitario,
        decimal montoDescuento,
        string? tarifaIvaImpuestoCodigo,
        decimal porcentajeImpuesto)
    {
        var errores = new List<Error>();

        if (productoId.HasValue && productoId.Value == Guid.Empty)
        {
            errores.Add(DocumentoVentaLineaErrors.ProductoRequerido);
        }

        if (!Enum.IsDefined(typeof(TipoItem), tipoItem))
        {
            errores.Add(DocumentoVentaLineaErrors.TipoItemInvalido);
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            errores.Add(DocumentoVentaLineaErrors.CodigoRequerido);
        }
        else if (codigo.Trim().Length > CodigoMaxLength)
        {
            errores.Add(DocumentoVentaLineaErrors.CodigoExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            errores.Add(DocumentoVentaLineaErrors.DescripcionRequerida);
        }
        else if (descripcion.Trim().Length > DescripcionMaxLength)
        {
            errores.Add(DocumentoVentaLineaErrors.DescripcionExcedeLongitud);
        }

        if (!string.IsNullOrWhiteSpace(unidadMedidaCodigo) && unidadMedidaCodigo.Trim().Length > UnidadMedidaCodigoMaxLength)
        {
            errores.Add(DocumentoVentaLineaErrors.UnidadMedidaExcedeLongitud);
        }

        if (cantidad <= 0)
        {
            errores.Add(DocumentoVentaLineaErrors.CantidadInvalida);
        }

        if (precioUnitario < 0)
        {
            errores.Add(DocumentoVentaLineaErrors.PrecioInvalido);
        }

        var bruto = cantidad * precioUnitario;
        if (montoDescuento < 0 || montoDescuento > bruto)
        {
            errores.Add(DocumentoVentaLineaErrors.DescuentoInvalido);
        }

        if (porcentajeImpuesto < 0)
        {
            errores.Add(DocumentoVentaLineaErrors.ImpuestoInvalido);
        }

        if (tarifaIvaImpuestoCodigo is not null && tarifaIvaImpuestoCodigo.Trim().Length > TarifaIvaCodigoMaxLength)
        {
            errores.Add(Error.Validation("DocumentoVentaLinea_TarifaIvaImpuestoCodigo", $"El código de tarifa IVA no puede exceder {TarifaIvaCodigoMaxLength} caracteres."));
        }

        return errores;
    }
}
