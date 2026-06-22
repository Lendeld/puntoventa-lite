namespace PuntoVenta.Application.DTOs.Ventas;

public sealed record ReporteMovimientosDineroResultadoDto(
    IReadOnlyList<MovimientoDineroFilaDto> Movimientos,
    IReadOnlyList<MovimientoDineroMedioDto> TotalesPorMedio,
    decimal TotalEntradas,
    decimal TotalSalidas,
    decimal TotalNeto);

public sealed record MovimientoDineroFilaDto(
    Guid PagoId,
    Guid DocumentoId,
    string? DocumentoConsecutivo,
    string TipoMovimiento,
    string TipoDocumento,
    DateTime FechaMovimientoUtc,
    DateTime FechaInformativaUtc,
    DateTime FechaRegistroUtc,
    DateTime? FechaAnulacionUtc,
    Guid? CajaId,
    string? CajaCodigo,
    string? CajaNombre,
    Guid? ClienteId,
    string? ClienteNombre,
    string? ClienteIdentificacion,
    Guid? UsuarioId,
    string? UsuarioNombre,
    string MedioPagoCodigo,
    string MedioPagoDetalle,
    string? Referencia,
    string MonedaCodigo,
    decimal Monto,
    int NumeroAbono,
    string? MotivoAnulacion,
    Guid? EventoId,
    string? EventoTipoCodigo,
    string? EventoResumen,
    DateTime? EventoOcurridoEn);

public sealed record MovimientoDineroMedioDto(
    string Codigo,
    string Detalle,
    decimal Entradas,
    decimal Salidas,
    decimal Neto);
