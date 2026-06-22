using ErrorOr;

namespace PuntoVenta.Domain.Entities.Ventas;

public sealed class DocumentoVentaReferencia
{
    public const int TipoDocReferenciaMaxLength = 2;
    public const int RazonMaxLength = 180;

    private DocumentoVentaReferencia() { }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DocumentoVentaId { get; private set; }
    public DocumentoVenta? DocumentoVenta { get; private set; }
    public Guid DocumentoReferenciaId { get; private set; }
    public DocumentoVenta? DocumentoReferencia { get; private set; }
    public string TipoDocReferencia { get; private set; } = string.Empty;
    public DateTime FechaDocumentoReferencia { get; private set; }
    public string Razon { get; private set; } = string.Empty;

    public static ErrorOr<DocumentoVentaReferencia> Crear(
        Guid documentoVentaId,
        Guid documentoReferenciaId,
        string tipoDocReferencia,
        DateTime fechaDocumentoReferencia,
        string? razon)
    {
        var errores = new List<Error>();

        if (documentoReferenciaId == Guid.Empty)
        {
            errores.Add(DocumentoVentaReferenciaErrors.DocumentoReferenciaRequerido);
        }

        if (string.IsNullOrWhiteSpace(tipoDocReferencia))
        {
            errores.Add(DocumentoVentaReferenciaErrors.TipoDocRequerido);
        }
        else if (tipoDocReferencia.Trim().Length > TipoDocReferenciaMaxLength)
        {
            errores.Add(DocumentoVentaReferenciaErrors.TipoDocExcedeLongitud);
        }

        if (fechaDocumentoReferencia == default)
        {
            errores.Add(DocumentoVentaReferenciaErrors.FechaRequerida);
        }

        if (!string.IsNullOrWhiteSpace(razon) && razon.Trim().Length > RazonMaxLength)
        {
            errores.Add(DocumentoVentaReferenciaErrors.RazonExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new DocumentoVentaReferencia
        {
            DocumentoVentaId = documentoVentaId,
            DocumentoReferenciaId = documentoReferenciaId,
            TipoDocReferencia = tipoDocReferencia.Trim(),
            FechaDocumentoReferencia = fechaDocumentoReferencia,
            Razon = razon?.Trim() ?? string.Empty
        };
    }
}
