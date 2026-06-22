using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Negocios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Negocio;

public sealed class ActualizarNegocioEndpoint(IMediator mediator) : Endpoint<ActualizarNegocioCommand>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/negocio/{id:guid}");
        Tags("Negocio");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.NegocioEditar));
        Summary(s =>
        {
            s.Summary = "Actualizar negocio";
            s.Description = "Modifica datos generales del negocio";
        });
    }

    public override async Task HandleAsync(ActualizarNegocioCommand req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = req with { Id = id };

        var result = await _mediator.Send(command, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
