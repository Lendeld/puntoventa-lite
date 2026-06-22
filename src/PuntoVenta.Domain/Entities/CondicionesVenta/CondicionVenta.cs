using ErrorOr;

namespace PuntoVenta.Domain.Entities.CondicionesVenta;

public sealed class CondicionVenta : BaseAuditableEntity
{
    public const int CodigoMaxLength = 2;
    public const int DetalleMaxLength = 100;
    public const int ComentarioMaxLength = 255;

    private CondicionVenta() { }

    public string Codigo { get; private set; } = string.Empty;
    public string Detalle { get; private set; } = string.Empty;
    public string? Comentario { get; private set; }

    public static ErrorOr<CondicionVenta> Crear(
        string codigo,
        string detalle,
        string? comentario = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            errores.Add(CondicionVentaErrors.CodigoRequerido);
        }
        else if (codigo.Trim().Length != CodigoMaxLength)
        {
            errores.Add(CondicionVentaErrors.CodigoLongitudInvalida);
        }

        if (string.IsNullOrWhiteSpace(detalle))
        {
            errores.Add(CondicionVentaErrors.DetalleRequerido);
        }
        else if (detalle.Trim().Length > DetalleMaxLength)
        {
            errores.Add(CondicionVentaErrors.DetalleExcedeLongitud);
        }

        if (comentario is not null && comentario.Trim().Length > ComentarioMaxLength)
        {
            errores.Add(CondicionVentaErrors.ComentarioExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new CondicionVenta
        {
            Codigo = codigo.Trim(),
            Detalle = detalle.Trim(),
            Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
        };
    }
}
