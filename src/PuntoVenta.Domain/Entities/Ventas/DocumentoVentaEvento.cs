using ErrorOr;

namespace PuntoVenta.Domain.Entities.Ventas;

public sealed class DocumentoVentaEvento : BaseAuditableEntity
{
    public const int TipoCodigoMaxLength = 50;
    public const int ResumenMaxLength = 200;

    private DocumentoVentaEvento() { }

    public Guid DocumentoVentaId { get; private set; }
    public DocumentoVenta? DocumentoVenta { get; private set; }
    public string TipoEventoCodigo { get; private set; } = string.Empty;
    public TipoDocumentoVentaEvento? TipoEvento { get; private set; }
    public DateTime OcurridoEn { get; private set; }
    public Guid? UsuarioId { get; private set; }
    public string Resumen { get; private set; } = string.Empty;
    public string? Payload { get; private set; }
    public Guid? CorrelacionId { get; private set; }

    public static ErrorOr<DocumentoVentaEvento> Crear(
        Guid documentoVentaId,
        string tipoEventoCodigo,
        string resumen,
        DateTime ocurridoEn,
        Guid? usuarioId = null,
        string? payload = null,
        Guid? correlacionId = null)
    {
        var errores = new List<Error>();

        if (documentoVentaId == Guid.Empty)
        {
            errores.Add(DocumentoVentaEventoErrors.DocumentoRequerido);
        }

        if (string.IsNullOrWhiteSpace(tipoEventoCodigo))
        {
            errores.Add(DocumentoVentaEventoErrors.TipoRequerido);
        }
        else if (tipoEventoCodigo.Trim().Length > TipoCodigoMaxLength)
        {
            errores.Add(DocumentoVentaEventoErrors.TipoExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(resumen))
        {
            errores.Add(DocumentoVentaEventoErrors.ResumenRequerido);
        }
        else if (resumen.Trim().Length > ResumenMaxLength)
        {
            errores.Add(DocumentoVentaEventoErrors.ResumenExcedeLongitud);
        }

        if (ocurridoEn == default)
        {
            errores.Add(DocumentoVentaEventoErrors.FechaRequerida);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new DocumentoVentaEvento
        {
            DocumentoVentaId = documentoVentaId,
            TipoEventoCodigo = tipoEventoCodigo.Trim(),
            Resumen = resumen.Trim(),
            OcurridoEn = ocurridoEn,
            UsuarioId = usuarioId,
            Payload = string.IsNullOrWhiteSpace(payload) ? null : payload,
            CorrelacionId = correlacionId
        };
    }
}
