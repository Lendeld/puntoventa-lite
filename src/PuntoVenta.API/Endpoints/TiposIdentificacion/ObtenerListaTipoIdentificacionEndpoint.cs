using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.TiposIdentificacion;
using PuntoVenta.Application.DTOs.TiposIdentificacion;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.TiposIdentificacion;

public sealed class ObtenerListaTipoIdentificacionEndpoint(IMediator mediator) : Endpoint<ObtenerListaTipoIdentificacionQuery, IReadOnlyList<TipoIdentificacionDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/tipos-identificacion");
        Tags("TiposIdentificacion");
        Options(b => b.RequireAuthorization());
        Summary(s =>
        {
            s.Summary = "Obtener tipos de identificación";
            s.Description = "Retorna listado de tipos de identificación. Filtro opcional: Activo";
        });
    }

    public override async Task HandleAsync(ObtenerListaTipoIdentificacionQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
