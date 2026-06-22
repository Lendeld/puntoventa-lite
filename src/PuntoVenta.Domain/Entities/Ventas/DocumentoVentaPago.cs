using ErrorOr;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Domain.Entities.Ventas;

public sealed class DocumentoVentaPago
{
    public const int MonedaCodigoMaxLength = 3;
    public const int MedioPagoCodigoMaxLength = 2;
    public const int MedioPagoDetalleSnapshotMaxLength = 100;
    public const int ReferenciaMaxLength = 100;
    public const int ObservacionMaxLength = 255;
    public const int ClaveHaciendaMaxLength = 50;
    public const int ConsecutivoHaciendaMaxLength = 20;
    public const int EstadoElectronicoMaxLength = 40;
    public const int MotivoAnulacionMaxLength = 255;

    private DocumentoVentaPago() { }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DocumentoVentaId { get; private set; }
    public DocumentoVenta? DocumentoVenta { get; private set; }
    public string MonedaCodigo { get; private set; } = "CRC";
    public decimal TipoCambioAplicado { get; private set; }
    public string MedioPagoCodigo { get; private set; } = string.Empty;
    public string MedioPagoDetalleSnapshot { get; private set; } = string.Empty;
    public decimal MontoEntregado { get; private set; }
    public decimal MontoAplicadoMonedaPago { get; private set; }
    public decimal MontoAplicadoDocumento { get; private set; }
    public decimal MontoVueltoMonedaPago { get; private set; }
    public decimal MontoVueltoDocumento { get; private set; }
    public DateTime FechaPago { get; private set; }
    public DateTime FechaRegistroUtc { get; private set; }
    public Guid? UsuarioRegistroId { get; private set; }
    public Usuario? UsuarioRegistro { get; private set; }
    public string? Referencia { get; private set; }
    public string? Observacion { get; private set; }
    public string? ClaveHaciendaREP { get; private set; }
    public string? ConsecutivoHaciendaREP { get; private set; }
    public string? EstadoElectronicoREP { get; private set; }
    public DateTime? FechaAceptacionREP { get; private set; }
    public int NumeroAbono { get; private set; }
    public bool Anulado { get; private set; }
    public DateTime? FechaAnulacionUtc { get; private set; }
    public Guid? UsuarioAnulaId { get; private set; }
    public Usuario? UsuarioAnula { get; private set; }
    public string? MotivoAnulacion { get; private set; }

    internal void AsignarNumeroAbono(int numeroAbono)
        => NumeroAbono = numeroAbono;

    public ErrorOr<Success> AnularPago(Guid usuarioAnulaId, string motivo, DateTime ahoraUtc)
    {
        if (Anulado)
        {
            return DocumentoVentaPagoErrors.YaAnulado;
        }

        if (usuarioAnulaId == Guid.Empty)
        {
            return DocumentoVentaPagoErrors.UsuarioAnulaInvalido;
        }

        var motivoNormalizado = motivo?.Trim();
        if (string.IsNullOrWhiteSpace(motivoNormalizado))
        {
            return DocumentoVentaPagoErrors.MotivoAnulacionRequerido;
        }

        if (motivoNormalizado.Length > MotivoAnulacionMaxLength)
        {
            return DocumentoVentaPagoErrors.MotivoAnulacionExcedeLongitud;
        }

        Anulado = true;
        FechaAnulacionUtc = NormalizarUtc(ahoraUtc);
        UsuarioAnulaId = usuarioAnulaId;
        MotivoAnulacion = motivoNormalizado;

        return Result.Success;
    }

    public void AsignarMetadataREP(string? clave, string? consecutivo, string? estado, DateTime? fechaAceptacion)
    {
        ClaveHaciendaREP = clave;
        ConsecutivoHaciendaREP = consecutivo;
        EstadoElectronicoREP = estado;
        FechaAceptacionREP = fechaAceptacion;
    }

    public static ErrorOr<DocumentoVentaPago> Crear(
        Guid documentoVentaId,
        string monedaCodigo,
        decimal tipoCambioAplicado,
        string medioPagoCodigo,
        string medioPagoDetalleSnapshot,
        decimal montoEntregado,
        decimal montoAplicadoMonedaPago,
        decimal montoAplicadoDocumento,
        decimal montoVueltoMonedaPago,
        decimal montoVueltoDocumento,
        DateTime fechaPago,
        DateTime fechaRegistroUtc,
        Guid? usuarioRegistroId = null,
        string? referencia = null,
        string? observacion = null)
    {
        var errores = new List<Error>();
        var monedaNormalizada = monedaCodigo.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(monedaCodigo))
        {
            errores.Add(DocumentoVentaPagoErrors.MonedaRequerida);
        }
        else if (monedaNormalizada.Length > MonedaCodigoMaxLength)
        {
            errores.Add(DocumentoVentaPagoErrors.MonedaExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(medioPagoCodigo))
        {
            errores.Add(DocumentoVentaPagoErrors.MedioPagoRequerido);
        }
        else if (medioPagoCodigo.Trim().Length > MedioPagoCodigoMaxLength)
        {
            errores.Add(DocumentoVentaPagoErrors.MedioPagoExcedeLongitud);
        }

        if (medioPagoDetalleSnapshot is not null && medioPagoDetalleSnapshot.Trim().Length > MedioPagoDetalleSnapshotMaxLength)
        {
            errores.Add(DocumentoVentaPagoErrors.DetalleExcedeLongitud);
        }

        if (tipoCambioAplicado <= 0)
        {
            errores.Add(DocumentoVentaPagoErrors.TipoCambioInvalido);
        }

        if (montoEntregado <= 0)
        {
            errores.Add(DocumentoVentaPagoErrors.MontoEntregadoInvalido);
        }

        if (montoAplicadoMonedaPago <= 0)
        {
            errores.Add(DocumentoVentaPagoErrors.MontoAplicadoMonedaPagoInvalido);
        }

        if (montoAplicadoDocumento <= 0)
        {
            errores.Add(DocumentoVentaPagoErrors.MontoAplicadoDocumentoInvalido);
        }

        if (montoVueltoMonedaPago < 0)
        {
            errores.Add(DocumentoVentaPagoErrors.MontoVueltoMonedaPagoInvalido);
        }

        if (montoVueltoDocumento < 0)
        {
            errores.Add(DocumentoVentaPagoErrors.MontoVueltoDocumentoInvalido);
        }

        if (fechaPago == default)
        {
            errores.Add(Error.Validation("DocumentoVentaPago_FechaPago", "La fecha del pago es requerida."));
        }

        if (usuarioRegistroId.HasValue && usuarioRegistroId.Value == Guid.Empty)
        {
            errores.Add(Error.Validation("DocumentoVentaPago_UsuarioRegistroId", "El usuario que registra el pago es inválido."));
        }

        if (decimal.Round(montoEntregado, 5, MidpointRounding.AwayFromZero)
            != decimal.Round(montoAplicadoMonedaPago + montoVueltoMonedaPago, 5, MidpointRounding.AwayFromZero))
        {
            errores.Add(DocumentoVentaPagoErrors.MontoEntregadoNoCuadra);
        }

        if (referencia is not null && referencia.Trim().Length > ReferenciaMaxLength)
        {
            errores.Add(DocumentoVentaPagoErrors.ReferenciaExcedeLongitud);
        }

        if (observacion is not null && observacion.Trim().Length > ObservacionMaxLength)
        {
            errores.Add(DocumentoVentaPagoErrors.ObservacionExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new DocumentoVentaPago
        {
            DocumentoVentaId = documentoVentaId,
            MonedaCodigo = monedaNormalizada,
            TipoCambioAplicado = tipoCambioAplicado,
            MedioPagoCodigo = medioPagoCodigo.Trim(),
            MedioPagoDetalleSnapshot = string.IsNullOrWhiteSpace(medioPagoDetalleSnapshot) ? medioPagoCodigo.Trim() : medioPagoDetalleSnapshot.Trim(),
            MontoEntregado = montoEntregado,
            MontoAplicadoMonedaPago = montoAplicadoMonedaPago,
            MontoAplicadoDocumento = montoAplicadoDocumento,
            MontoVueltoMonedaPago = montoVueltoMonedaPago,
            MontoVueltoDocumento = montoVueltoDocumento,
            FechaPago = NormalizarUtc(fechaPago),
            FechaRegistroUtc = NormalizarUtc(fechaRegistroUtc),
            UsuarioRegistroId = usuarioRegistroId,
            Referencia = string.IsNullOrWhiteSpace(referencia) ? null : referencia.Trim(),
            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim()
        };
    }

    private static DateTime NormalizarUtc(DateTime fecha)
        => fecha.Kind switch
        {
            DateTimeKind.Utc => fecha,
            DateTimeKind.Local => fecha.ToUniversalTime(),
            _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
        };
}
