using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Auth.ObtenerUsuarioActual;
using PuntoVenta.Application.DTOs.Auth;

namespace PuntoVenta.API.Endpoints.Auth;

public sealed class ObtenerUsuarioActualEndpoint(IMediator mediator) : EndpointWithoutRequest<UsuarioActualDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/auth/usuario-actual");
        Tags("Auth");
        Summary(summary =>
        {
            summary.Summary = "Obtener usuario actual";
            summary.Description = "Retorna usuario autenticado con usuario, nombre y correo si existe";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObtenerUsuarioActualQuery(), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
