using ErrorOr;

namespace PuntoVenta.Domain.Entities.TarifasIvaImpuesto;

public sealed class TarifaIvaImpuesto : BaseAuditableEntity
{
    public const int CodigoMaxLength = 2;
    public const int DetalleMaxLength = 100;
    public const int ComentarioMaxLength = 255;

    private TarifaIvaImpuesto() { }

    public string Codigo { get; private set; } = string.Empty;
    public string Detalle { get; private set; } = string.Empty;
    public decimal Porcentaje { get; private set; }
    public string? Comentario { get; private set; }

    public static ErrorOr<TarifaIvaImpuesto> Crear(
        string codigo,
        string detalle,
        decimal porcentaje,
        string? comentario = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            errores.Add(TarifaIvaImpuestoErrors.CodigoRequerido);
        }
        else if (codigo.Trim().Length != CodigoMaxLength)
        {
            errores.Add(TarifaIvaImpuestoErrors.CodigoLongitudInvalida);
        }

        if (string.IsNullOrWhiteSpace(detalle))
        {
            errores.Add(TarifaIvaImpuestoErrors.DetalleRequerido);
        }
        else if (detalle.Trim().Length > DetalleMaxLength)
        {
            errores.Add(TarifaIvaImpuestoErrors.DetalleExcedeLongitud);
        }

        if (porcentaje < 0 || porcentaje > 100)
        {
            errores.Add(TarifaIvaImpuestoErrors.PorcentajeInvalido);
        }

        if (comentario is not null && comentario.Trim().Length > ComentarioMaxLength)
        {
            errores.Add(TarifaIvaImpuestoErrors.ComentarioExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new TarifaIvaImpuesto
        {
            Codigo = codigo.Trim(),
            Detalle = detalle.Trim(),
            Porcentaje = porcentaje,
            Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
        };
    }
}
