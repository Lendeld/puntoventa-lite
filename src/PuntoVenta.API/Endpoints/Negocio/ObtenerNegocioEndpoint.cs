using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Negocios;
using PuntoVenta.Application.DTOs.Negocios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Negocio;

public sealed class ObtenerNegocioEndpoint(IMediator mediator) : EndpointWithoutRequest<NegocioDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/negocio");
        Tags("Negocio");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.NegocioVer));
        Summary(s =>
        {
            s.Summary = "Obtener negocio";
            s.Description = "Retorna datos del negocio configurado en el sistema";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObtenerNegocioQuery(), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
