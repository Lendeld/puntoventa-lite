using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Usuarios;
using PuntoVenta.Application.DTOs.Usuarios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Usuarios;

public sealed class ObtenerUsuarioEndpoint(IMediator mediator) : EndpointWithoutRequest<UsuarioDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/usuarios/{id:guid}");
        Tags("Usuarios");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.UsuariosVer));
        Summary(s =>
        {
            s.Summary = "Obtener usuario por Id";
            s.Description = "Retorna los datos del usuario";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ObtenerUsuarioPorIdQuery(id), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
