using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class ConvertirApartadoAFacturaEndpoint(IMediator mediator) : EndpointWithoutRequest<Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/ventas/apartados/{id:guid}/convertir");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VentasApartadosConvertir));
        Summary(s => s.Summary = "Convertir apartado a factura");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ConvertirApartadoAFacturaCommand(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
