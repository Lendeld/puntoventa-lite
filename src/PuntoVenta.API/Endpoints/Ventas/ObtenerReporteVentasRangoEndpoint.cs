using ErrorOr;
using FastEndpoints;
using FluentValidation;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class ObtenerReporteVentasRangoRequest
{
    // Bindeadas como DateTimeOffset para preservar el instante exacto del ISO-8601
    // con offset (ej. "2026-06-01T06:00:00.000Z"). Un DateTime parseado desde ese
    // string queda con Kind=Local/Unspecified; con DateTimeOffset el offset es
    // explícito y .UtcDateTime entrega el instante correcto con Kind=Utc.
    public DateTimeOffset FechaDesde { get; set; }
    public DateTimeOffset FechaHasta { get; set; }
    public string? Consecutivo { get; set; }
    public bool Colonizar { get; set; } = true;
    public bool Detallado { get; set; } = true;
}

// Valida que el rango de fechas venga completo y bien ordenado. Compartido por el
// endpoint de datos y el de Excel (ambos usan ObtenerReporteVentasRangoRequest).
public sealed class ObtenerReporteVentasRangoValidator : Validator<ObtenerReporteVentasRangoRequest>
{
    // Tope de tamaño del rango. Evita reportes inmanejables (y el costo de query/Excel).
    public const int MaxDiasRango = 366;

    public ObtenerReporteVentasRangoValidator()
    {
        RuleFor(x => x.FechaDesde)
            .NotEqual(default(DateTimeOffset))
            .WithMessage("La fecha inicial es obligatoria.");

        RuleFor(x => x.FechaHasta)
            .NotEqual(default(DateTimeOffset))
            .WithMessage("La fecha final es obligatoria.");

        RuleFor(x => x.FechaHasta)
            .GreaterThanOrEqualTo(x => x.FechaDesde)
            .When(x => x.FechaDesde != default && x.FechaHasta != default)
            .WithMessage("La fecha final no puede ser anterior a la fecha inicial.");

        RuleFor(x => x.FechaHasta)
            .Must((req, _) => (req.FechaHasta - req.FechaDesde).TotalDays <= MaxDiasRango)
            .When(x => x.FechaDesde != default && x.FechaHasta != default
                && x.FechaHasta >= x.FechaDesde)
            .WithMessage($"El rango no puede superar {MaxDiasRango} días. Reduce el período.");
    }
}

// Mapea las fallas del validador de FastEndpoints a Error.Validation y las pasa por
// ToProblem(), igual que los errores del handler (validación = warning en frontend).
internal static class ObtenerReporteVentasRangoValidacion
{
    public static List<Error> AErrores(this IEnumerable<FluentValidation.Results.ValidationFailure> fallas)
        => [.. fallas.Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))];
}

public sealed class ObtenerReporteVentasRangoEndpoint(IMediator mediator)
        : Endpoint<ObtenerReporteVentasRangoRequest, ReporteVentasRangoResultadoDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/ventas/reportes/rango");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ReportesVentasRangoVer));
        // No lanzar el 400 por defecto de FastEndpoints: lo mapeamos por ToProblem()
        // para mantener el shape ApiErrorResponse y la severidad consistente.
        DontThrowIfValidationFails();
        Summary(s => s.Summary = "Reporte de ventas por rango de fecha de factura");
    }

    public override async Task HandleAsync(ObtenerReporteVentasRangoRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await Send.ResultAsync(ValidationFailures.AErrores().ToProblem());
            return;
        }

        var query = new ObtenerReporteVentasRangoQuery(
            req.FechaDesde.UtcDateTime,
            req.FechaHasta.UtcDateTime,
            req.Consecutivo,
            req.Colonizar,
            req.Detallado);

        var result = await _mediator.Send(query, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
