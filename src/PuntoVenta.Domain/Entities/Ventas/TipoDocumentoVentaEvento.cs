using ErrorOr;

namespace PuntoVenta.Domain.Entities.Ventas;

public sealed class TipoDocumentoVentaEvento : BaseAuditableEntity
{
    public const int CodigoMaxLength = 50;
    public const int NombreMaxLength = 100;
    public const int DescripcionMaxLength = 300;
    public const int CategoriaMaxLength = 30;
    public const int IconoMaxLength = 50;
    public const int ColorMaxLength = 20;

    private TipoDocumentoVentaEvento() { }

    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string Categoria { get; private set; } = string.Empty;
    public string? IconoSugerido { get; private set; }
    public string? ColorSugerido { get; private set; }

    public static ErrorOr<TipoDocumentoVentaEvento> Crear(
        string codigo,
        string nombre,
        string categoria,
        string? descripcion = null,
        string? iconoSugerido = null,
        string? colorSugerido = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            errores.Add(TipoDocumentoVentaEventoErrors.CodigoRequerido);
        }
        else if (codigo.Trim().Length > CodigoMaxLength)
        {
            errores.Add(TipoDocumentoVentaEventoErrors.CodigoExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(TipoDocumentoVentaEventoErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(TipoDocumentoVentaEventoErrors.NombreExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(categoria))
        {
            errores.Add(TipoDocumentoVentaEventoErrors.CategoriaRequerida);
        }
        else if (categoria.Trim().Length > CategoriaMaxLength)
        {
            errores.Add(TipoDocumentoVentaEventoErrors.CategoriaExcedeLongitud);
        }

        if (descripcion is not null && descripcion.Trim().Length > DescripcionMaxLength)
        {
            errores.Add(TipoDocumentoVentaEventoErrors.DescripcionExcedeLongitud);
        }

        if (iconoSugerido is not null && iconoSugerido.Trim().Length > IconoMaxLength)
        {
            errores.Add(TipoDocumentoVentaEventoErrors.IconoExcedeLongitud);
        }

        if (colorSugerido is not null && colorSugerido.Trim().Length > ColorMaxLength)
        {
            errores.Add(TipoDocumentoVentaEventoErrors.ColorExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new TipoDocumentoVentaEvento
        {
            Codigo = codigo.Trim(),
            Nombre = nombre.Trim(),
            Categoria = categoria.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion!.Trim(),
            IconoSugerido = string.IsNullOrWhiteSpace(iconoSugerido) ? null : iconoSugerido!.Trim(),
            ColorSugerido = string.IsNullOrWhiteSpace(colorSugerido) ? null : colorSugerido!.Trim()
        };
    }
}
