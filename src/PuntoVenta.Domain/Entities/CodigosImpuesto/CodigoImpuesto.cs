using ErrorOr;

namespace PuntoVenta.Domain.Entities.CodigosImpuesto;

public sealed class CodigoImpuesto : BaseAuditableEntity
{
    public const int CodigoMaxLength = 2;
    public const int DetalleMaxLength = 150;
    public const int ComentarioMaxLength = 255;

    private CodigoImpuesto() { }

    public string Codigo { get; private set; } = string.Empty;
    public string Detalle { get; private set; } = string.Empty;
    public string? Comentario { get; private set; }

    public static ErrorOr<CodigoImpuesto> Crear(
        string codigo,
        string detalle,
        string? comentario = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            errores.Add(CodigoImpuestoErrors.CodigoRequerido);
        }
        else if (codigo.Trim().Length != CodigoMaxLength)
        {
            errores.Add(CodigoImpuestoErrors.CodigoLongitudInvalida);
        }

        if (string.IsNullOrWhiteSpace(detalle))
        {
            errores.Add(CodigoImpuestoErrors.DetalleRequerido);
        }
        else if (detalle.Trim().Length > DetalleMaxLength)
        {
            errores.Add(CodigoImpuestoErrors.DetalleExcedeLongitud);
        }

        if (comentario is not null && comentario.Trim().Length > ComentarioMaxLength)
        {
            errores.Add(CodigoImpuestoErrors.ComentarioExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new CodigoImpuesto
        {
            Codigo = codigo.Trim(),
            Detalle = detalle.Trim(),
            Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
        };
    }
}
