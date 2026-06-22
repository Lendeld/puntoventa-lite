using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Usuarios;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Usuarios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Usuarios;

public sealed class ObtenerListaPaginadoUsuarioEndpoint(IMediator mediator) : Endpoint<ObtenerListaPaginadoUsuarioQuery, PagedResult<UsuarioDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/usuarios");
        Tags("Usuarios");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.UsuariosVer));
        Summary(s =>
        {
            s.Summary = "Obtener lista paginada de usuarios";
            s.Description = "Filtros: Activo (nullable), FiltroDinamico (nombre/nombreUsuario/identificacion/correo), NumeroPagina, TamanoPagina. Orden: FechaCreacion desc.";
        });
    }

    public override async Task HandleAsync(ObtenerListaPaginadoUsuarioQuery req, CancellationToken ct)
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
