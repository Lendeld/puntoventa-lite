using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Roles;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Roles;

public sealed class ActualizarRolEndpoint(IMediator mediator) : Endpoint<ActualizarRolCommand>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/roles/{id:guid}");
        Tags("Roles");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.RolesEditar));
        Summary(s =>
        {
            s.Summary = "Actualizar rol";
            s.Description = "Modifica el nombre y descripción de un rol existente";
        });
    }

    public override async Task HandleAsync(ActualizarRolCommand req, CancellationToken ct)
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
