using ErrorOr;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Domain.Entities.Secuencias;

/// <summary>
/// Mantiene el último número emitido por tipo de documento.
/// Una fila por tipo; el incremento es atómico dentro de la misma transacción.
/// </summary>
public sealed class Secuencia : BaseAuditableEntity
{
    private Secuencia() { }

    public TipoDocumentoVenta TipoDocumento { get; private set; }
    public long UltimoNumero { get; private set; }

    public static ErrorOr<Secuencia> Crear(TipoDocumentoVenta tipoDocumento)
    {
        if (!Enum.IsDefined(typeof(TipoDocumentoVenta), tipoDocumento))
        {
            return SecuenciaErrors.TipoDocumentoInvalido;
        }

        return new Secuencia
        {
            TipoDocumento = tipoDocumento,
            UltimoNumero = 0
        };
    }

    /// <summary>
    /// Incrementa el contador y devuelve el número formateado como consecutivo.
    /// Debe llamarse dentro de la misma transacción que el documento.
    /// </summary>
    public string Siguiente()
    {
        UltimoNumero += 1;
        return $"{Prefijo(TipoDocumento)}-{UltimoNumero:D6}";
    }

    private static string Prefijo(TipoDocumentoVenta tipo) => tipo switch
    {
        TipoDocumentoVenta.Factura => "FAC",
        TipoDocumentoVenta.Proforma => "PRO",
        TipoDocumentoVenta.Apartado => "APA",
        TipoDocumentoVenta.NotaCredito => "NC",
        TipoDocumentoVenta.NotaDebito => "ND",
        _ => "DOC"
    };
}
