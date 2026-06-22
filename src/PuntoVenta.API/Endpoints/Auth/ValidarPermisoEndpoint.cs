using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Auth;

namespace PuntoVenta.API.Endpoints.Auth;

public sealed class ValidarPermisoEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/auth/validar-permiso/{clave}");
        Tags("Auth");
        Summary(summary =>
        {
            summary.Summary = "Validar permiso";
            summary.Description = "Verifica si usuario autenticado tiene permiso solicitado";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var clave = Route<string>("clave");

        if (string.IsNullOrEmpty(clave))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var result = await _mediator.Send(
            new ValidarPermisoQuery(clave), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
