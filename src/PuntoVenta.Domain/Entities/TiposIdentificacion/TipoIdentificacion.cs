using ErrorOr;

namespace PuntoVenta.Domain.Entities.TiposIdentificacion;

public sealed class TipoIdentificacion : BaseAuditableEntity
{
    public const int CodigoMaxLength = 2;
    public const int DetalleMaxLength = 100;
    public const int ComentarioMaxLength = 255;

    private TipoIdentificacion() { }

    public string Codigo { get; private set; } = string.Empty;

    public string Detalle { get; private set; } = string.Empty;

    public string? Comentario { get; private set; }

    public static ErrorOr<TipoIdentificacion> Crear(
        string codigo,
        string detalle,
        string? comentario = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            errores.Add(TipoIdentificacionErrors.CodigoRequerido);
        }
        else if (codigo.Trim().Length != CodigoMaxLength)
        {
            errores.Add(TipoIdentificacionErrors.CodigoLongitudInvalida);
        }

        if (string.IsNullOrWhiteSpace(detalle))
        {
            errores.Add(TipoIdentificacionErrors.DetalleRequerido);
        }
        else if (detalle.Trim().Length > DetalleMaxLength)
        {
            errores.Add(TipoIdentificacionErrors.DetalleExcedeLongitud);
        }

        if (comentario is not null && comentario.Trim().Length > ComentarioMaxLength)
        {
            errores.Add(TipoIdentificacionErrors.ComentarioExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new TipoIdentificacion
        {
            Codigo = codigo.Trim(),
            Detalle = detalle.Trim(),
            Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
        };
    }
}
