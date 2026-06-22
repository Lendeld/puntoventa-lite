using ErrorOr;

namespace PuntoVenta.Domain.Entities.Negocios;

public sealed class LineaPieDocumento
{
    public const int MaxTextoLength = 120;

    private LineaPieDocumento() { }

    public string Texto { get; private set; } = string.Empty;

    public AlineacionLineaPie Alineacion { get; private set; }

    public bool Negrita { get; private set; }

    public int Orden { get; private set; }

    public static ErrorOr<LineaPieDocumento> Crear(
        string texto,
        AlineacionLineaPie alineacion,
        bool negrita,
        int orden)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(texto))
        {
            errores.Add(LineaPieDocumentoErrors.TextoRequerido);
        }
        else if (texto.Trim().Length > MaxTextoLength)
        {
            errores.Add(LineaPieDocumentoErrors.TextoExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new LineaPieDocumento
        {
            Texto = texto.Trim(),
            Alineacion = alineacion,
            Negrita = negrita,
            Orden = orden
        };
    }
}
