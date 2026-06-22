using ErrorOr;

namespace PuntoVenta.Domain.Entities.Negocios;

// Elemento ordenable del encabezado del ticket térmico. Los tipos fijos
// (todos menos Texto) siempre están presentes y solo se ocultan/reordenan;
// Texto es agregable/borrable y lleva TextoLibre.
public sealed class ElementoEncabezado
{
    public const int MaxTextoLibreLength = 60;

    private ElementoEncabezado() { }

    public ElementoEncabezadoTipo Tipo { get; private set; }

    public int Orden { get; private set; }

    public bool Visible { get; private set; }

    public string? TextoLibre { get; private set; }

    public bool EsFijo => Tipo != ElementoEncabezadoTipo.Texto;

    public static ErrorOr<ElementoEncabezado> Crear(
        ElementoEncabezadoTipo tipo,
        int orden,
        bool visible,
        string? textoLibre)
    {
        var errores = new List<Error>();

        if (!Enum.IsDefined(tipo))
        {
            errores.Add(ElementoEncabezadoErrors.TipoInvalido);
        }

        if (tipo == ElementoEncabezadoTipo.Texto)
        {
            if (string.IsNullOrWhiteSpace(textoLibre))
            {
                errores.Add(ElementoEncabezadoErrors.TextoLibreRequerido);
            }
            else if (textoLibre.Trim().Length > MaxTextoLibreLength)
            {
                errores.Add(ElementoEncabezadoErrors.TextoLibreExcedeLongitud);
            }
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new ElementoEncabezado
        {
            Tipo = tipo,
            Orden = orden,
            Visible = visible,
            TextoLibre = tipo == ElementoEncabezadoTipo.Texto ? textoLibre!.Trim() : null
        };
    }
}
