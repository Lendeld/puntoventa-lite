using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Categorias;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Categorias;

public sealed class CrearCategoriaEndpoint(IMediator mediator) : Endpoint<CrearCategoriaCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/categorias");
        Tags("Categorias");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CategoriasCrear));
        Summary(s =>
        {
            s.Summary = "Crear categoría";
            s.Description = "Registra una nueva categoría";
        });
    }

    public override async Task HandleAsync(CrearCategoriaCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
