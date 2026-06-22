using ErrorOr;

namespace PuntoVenta.Domain.Entities.MediosPago;

public sealed class MedioPago : BaseAuditableEntity
{
    public const int CodigoMaxLength = 2;
    public const int DetalleMaxLength = 100;
    public const int ComentarioMaxLength = 255;

    private MedioPago() { }

    public string Codigo { get; private set; } = string.Empty;
    public string Detalle { get; private set; } = string.Empty;
    public string? Comentario { get; private set; }

    public static ErrorOr<MedioPago> Crear(
        string codigo,
        string detalle,
        string? comentario = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            errores.Add(MedioPagoErrors.CodigoRequerido);
        }
        else if (codigo.Trim().Length != CodigoMaxLength)
        {
            errores.Add(MedioPagoErrors.CodigoLongitudInvalida);
        }

        if (string.IsNullOrWhiteSpace(detalle))
        {
            errores.Add(MedioPagoErrors.DetalleRequerido);
        }
        else if (detalle.Trim().Length > DetalleMaxLength)
        {
            errores.Add(MedioPagoErrors.DetalleExcedeLongitud);
        }

        if (comentario is not null && comentario.Trim().Length > ComentarioMaxLength)
        {
            errores.Add(MedioPagoErrors.ComentarioExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new MedioPago
        {
            Codigo = codigo.Trim(),
            Detalle = detalle.Trim(),
            Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
        };
    }
}
