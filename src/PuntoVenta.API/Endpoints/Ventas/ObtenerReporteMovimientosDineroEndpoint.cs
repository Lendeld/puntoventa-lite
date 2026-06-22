using ErrorOr;
using FastEndpoints;
using FluentValidation;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class ObtenerReporteMovimientosDineroRequest
{
    public DateTimeOffset FechaDesde { get; set; }
    public DateTimeOffset FechaHasta { get; set; }
    public Guid? CajaId { get; set; }
}

public sealed class ObtenerReporteMovimientosDineroValidator : Validator<ObtenerReporteMovimientosDineroRequest>
{
    public ObtenerReporteMovimientosDineroValidator()
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
    }
}

internal static class ObtenerReporteMovimientosDineroValidacion
{
    public static List<Error> AErroresMovimientosDinero(this IEnumerable<FluentValidation.Results.ValidationFailure> fallas)
        => [.. fallas.Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))];
}

public sealed class ObtenerReporteMovimientosDineroEndpoint(IMediator mediator)
    : Endpoint<ObtenerReporteMovimientosDineroRequest, ReporteMovimientosDineroResultadoDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/ventas/reportes/movimientos-dinero");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ReportesVer));
        DontThrowIfValidationFails();
        Summary(s => s.Summary = "Obtener reporte simple de movimientos de dinero");
    }

    public override async Task HandleAsync(ObtenerReporteMovimientosDineroRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await Send.ResultAsync(ValidationFailures.AErroresMovimientosDinero().ToProblem());
            return;
        }

        var result = await _mediator.Send(
            new ObtenerReporteMovimientosDineroQuery(req.FechaDesde.UtcDateTime, req.FechaHasta.UtcDateTime, req.CajaId),
            ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}

public sealed class ObtenerReporteMovimientosDineroPdfEndpoint(IMediator mediator)
    : Endpoint<ObtenerReporteMovimientosDineroRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/ventas/reportes/movimientos-dinero/pdf");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ReportesVer));
        DontThrowIfValidationFails();
        Summary(s => s.Summary = "Obtener PDF del reporte de movimientos de dinero");
    }

    public override async Task HandleAsync(ObtenerReporteMovimientosDineroRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await Send.ResultAsync(ValidationFailures.AErroresMovimientosDinero().ToProblem());
            return;
        }

        var result = await _mediator.Send(
            new ObtenerReporteMovimientosDineroPdfQuery(req.FechaDesde.UtcDateTime, req.FechaHasta.UtcDateTime, req.CajaId),
            ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        HttpContext.Response.ContentType = result.Value.ContentType;
        HttpContext.Response.Headers.ContentDisposition = $"inline; filename=\"{result.Value.FileName}\"";
        HttpContext.Response.Headers.CacheControl = "no-store, must-revalidate";
        HttpContext.Response.Headers.Pragma = "no-cache";
        await HttpContext.Response.Body.WriteAsync(result.Value.Content, ct);
    }
}
