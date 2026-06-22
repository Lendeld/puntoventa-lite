using ErrorOr;

namespace PuntoVenta.Domain.Entities.Productos;

public sealed class Producto : BaseAuditableEntity
{
    public const int CodigoMaxLength = 20;
    public const int CodigoBarrasMaxLength = 50;
    public const int NombreMaxLength = 150;
    public const int DescripcionMaxLength = 500;
    public const int ImagenUrlMaxLength = 500;
    public const int TarifaIvaCodigoMaxLength = 2;

    private Producto() { }

    public string Codigo { get; private set; } = string.Empty;
    public string? CodigoBarras { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public TipoItem TipoItem { get; private set; }
    public string? ImagenUrl { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal? PrecioCosto { get; private set; }
    public Guid? CategoriaId { get; private set; }
    public string? TarifaIvaImpuestoCodigo { get; private set; }
    public bool NoAplicaExistencias { get; private set; }
    public bool PermiteModificarPrecioUnitario { get; private set; }
    public decimal Existencia { get; private set; }

    public static ErrorOr<Producto> Crear(
        string codigo,
        string nombre,
        TipoItem tipoItem,
        decimal precioUnitario,
        string? codigoBarras = null,
        string? descripcion = null,
        string? imagenUrl = null,
        decimal? precioCosto = null,
        Guid? categoriaId = null,
        string? tarifaIvaImpuestoCodigo = null,
        bool noAplicaExistencias = false,
        bool permiteModificarPrecioUnitario = false)
    {
        var errores = Validar(
            codigo, nombre, tipoItem, precioUnitario,
            codigoBarras, descripcion, imagenUrl, precioCosto,
            noAplicaExistencias);

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Producto
        {
            Codigo = codigo.Trim(),
            CodigoBarras = string.IsNullOrWhiteSpace(codigoBarras) ? null : codigoBarras.Trim(),
            Nombre = nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim(),
            TipoItem = tipoItem,
            ImagenUrl = string.IsNullOrWhiteSpace(imagenUrl) ? null : imagenUrl.Trim(),
            PrecioUnitario = precioUnitario,
            PrecioCosto = precioCosto,
            CategoriaId = categoriaId,
            TarifaIvaImpuestoCodigo = string.IsNullOrWhiteSpace(tarifaIvaImpuestoCodigo) ? null : tarifaIvaImpuestoCodigo.Trim(),
            NoAplicaExistencias = noAplicaExistencias,
            PermiteModificarPrecioUnitario = permiteModificarPrecioUnitario
        };
    }

    public ErrorOr<Success> Actualizar(
        string codigo,
        string nombre,
        TipoItem tipoItem,
        decimal precioUnitario,
        string? codigoBarras = null,
        string? descripcion = null,
        string? imagenUrl = null,
        decimal? precioCosto = null,
        Guid? categoriaId = null,
        string? tarifaIvaImpuestoCodigo = null,
        bool noAplicaExistencias = false,
        bool permiteModificarPrecioUnitario = false)
    {
        var errores = Validar(
            codigo, nombre, tipoItem, precioUnitario,
            codigoBarras, descripcion, imagenUrl, precioCosto,
            noAplicaExistencias);

        if (errores.Count > 0)
        {
            return errores;
        }

        Codigo = codigo.Trim();
        CodigoBarras = string.IsNullOrWhiteSpace(codigoBarras) ? null : codigoBarras.Trim();
        Nombre = nombre.Trim();
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        TipoItem = tipoItem;
        ImagenUrl = string.IsNullOrWhiteSpace(imagenUrl) ? null : imagenUrl.Trim();
        PrecioUnitario = precioUnitario;
        PrecioCosto = precioCosto;
        CategoriaId = categoriaId;
        TarifaIvaImpuestoCodigo = string.IsNullOrWhiteSpace(tarifaIvaImpuestoCodigo) ? null : tarifaIvaImpuestoCodigo.Trim();
        NoAplicaExistencias = noAplicaExistencias;
        PermiteModificarPrecioUnitario = permiteModificarPrecioUnitario;

        return Result.Success;
    }

    /// <summary>
    /// Aplica un delta al stock (positivo = ingreso, negativo = salida).
    /// Puede quedar en negativo — la venta nunca se bloquea por stock.
    /// </summary>
    public decimal AplicarMovimientoStock(decimal delta)
    {
        Existencia += delta;
        return Existencia;
    }

    private static List<Error> Validar(
        string codigo,
        string nombre,
        TipoItem tipoItem,
        decimal precioUnitario,
        string? codigoBarras,
        string? descripcion,
        string? imagenUrl,
        decimal? precioCosto,
        bool noAplicaExistencias)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            errores.Add(ProductoErrors.CodigoRequerido);
        }
        else if (codigo.Trim().Length > CodigoMaxLength)
        {
            errores.Add(ProductoErrors.CodigoExcedeLongitud);
        }

        if (codigoBarras is not null && codigoBarras.Trim().Length > CodigoBarrasMaxLength)
        {
            errores.Add(ProductoErrors.CodigoBarrasExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(ProductoErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(ProductoErrors.NombreExcedeLongitud);
        }

        if (descripcion is not null && descripcion.Trim().Length > DescripcionMaxLength)
        {
            errores.Add(ProductoErrors.DescripcionExcedeLongitud);
        }

        if (imagenUrl is not null && imagenUrl.Trim().Length > ImagenUrlMaxLength)
        {
            errores.Add(ProductoErrors.ImagenUrlExcedeLongitud);
        }

        if (!Enum.IsDefined(typeof(TipoItem), tipoItem))
        {
            errores.Add(ProductoErrors.TipoItemInvalido);
        }

        if (precioUnitario < 0)
        {
            errores.Add(ProductoErrors.PrecioUnitarioInvalido);
        }

        if (precioCosto.HasValue && precioCosto.Value < 0)
        {
            errores.Add(ProductoErrors.PrecioCostoInvalido);
        }

        if (tipoItem == TipoItem.Servicio && noAplicaExistencias)
        {
            errores.Add(ProductoErrors.NoAplicaExistenciasSoloBien);
        }

        return errores;
    }
}
