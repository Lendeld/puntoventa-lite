using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Cajas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerReporteMovimientosDineroQuery(
    DateTime FechaDesdeUtc,
    DateTime FechaHastaUtc,
    Guid? CajaId = null) : IRequest<ErrorOr<ReporteMovimientosDineroResultadoDto>>;

public sealed class ObtenerReporteMovimientosDineroHandler(
    IDocumentoVentaRepository documentoRepository)
    : IRequestHandler<ObtenerReporteMovimientosDineroQuery, ErrorOr<ReporteMovimientosDineroResultadoDto>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;

    public async ValueTask<ErrorOr<ReporteMovimientosDineroResultadoDto>> Handle(
        ObtenerReporteMovimientosDineroQuery query,
        CancellationToken cancellationToken)
    {
        if (query.FechaHastaUtc < query.FechaDesdeUtc)
        {
            return Error.Validation(
                "ReporteMovimientosDinero_RangoInvertido",
                "La fecha final no puede ser anterior a la fecha inicial.");
        }

        return await _documentoRepository.ObtenerReporteMovimientosDineroAsync(
            query.FechaDesdeUtc,
            query.FechaHastaUtc,
            query.CajaId,
            cancellationToken);
    }
}

public sealed record ObtenerReporteMovimientosDineroPdfQuery(
    DateTime FechaDesdeUtc,
    DateTime FechaHastaUtc,
    Guid? CajaId = null) : IRequest<ErrorOr<DocumentoVentaPdfDto>>;

public sealed class ObtenerReporteMovimientosDineroPdfHandler(
    IDocumentoVentaRepository documentoRepository,
    INegocioRepository negocioRepository,
    ICajaRepository cajaRepository,
    IReporteMovimientosDineroPdfService pdfService)
    : IRequestHandler<ObtenerReporteMovimientosDineroPdfQuery, ErrorOr<DocumentoVentaPdfDto>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly INegocioRepository _negocioRepository = negocioRepository;
    private readonly ICajaRepository _cajaRepository = cajaRepository;
    private readonly IReporteMovimientosDineroPdfService _pdfService = pdfService;

    public async ValueTask<ErrorOr<DocumentoVentaPdfDto>> Handle(
        ObtenerReporteMovimientosDineroPdfQuery query,
        CancellationToken cancellationToken)
    {
        if (query.FechaHastaUtc < query.FechaDesdeUtc)
        {
            return Error.Validation(
                "ReporteMovimientosDinero_RangoInvertido",
                "La fecha final no puede ser anterior a la fecha inicial.");
        }

        var negocio = await _negocioRepository.ObtenerAsync(cancellationToken);
        if (negocio is null)
        {
            return Error.NotFound("Negocio.NoEncontrado", "No se encontró el negocio.");
        }

        string? cajaCodigo = null;
        string? cajaNombre = null;
        if (query.CajaId.HasValue)
        {
            var caja = await _cajaRepository.GetByIdAsync(query.CajaId.Value, cancellationToken);
            if (caja is null)
            {
                return CajaErrors.NoEncontrada;
            }

            cajaCodigo = caja.Codigo;
            cajaNombre = caja.Nombre;
        }

        var reporte = await _documentoRepository.ObtenerReporteMovimientosDineroAsync(
            query.FechaDesdeUtc,
            query.FechaHastaUtc,
            query.CajaId,
            cancellationToken);

        var data = new ReporteMovimientosDineroPdfData(
            negocio,
            query.FechaDesdeUtc,
            query.FechaHastaUtc,
            cajaCodigo,
            cajaNombre,
            reporte);

        var content = await _pdfService.GenerarAsync(data, cancellationToken);
        var sufijoCaja = !string.IsNullOrWhiteSpace(cajaCodigo)
            ? $"-{SanitizarParaFilename(cajaCodigo)}"
            : "-todas";
        var fileName = $"reporte-movimientos-dinero{sufijoCaja}-{query.FechaDesdeUtc:yyyyMMdd}-{query.FechaHastaUtc:yyyyMMdd}.pdf";

        return new DocumentoVentaPdfDto(content, fileName, "application/pdf");
    }

    private static string SanitizarParaFilename(string value)
    {
        Span<char> buffer = stackalloc char[value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            buffer[i] = char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_';
        }
        return new string(buffer);
    }
}
