using FastEndpoints;

namespace PuntoVenta.API.Endpoints.Health;

public sealed class HealthEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/health");
        Tags("Health");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Health check";
            summary.Description = "Verifica que el proceso está activo (warm-up). No consulta la base de datos.";
        });
    }

    public override Task HandleAsync(CancellationToken ct) => Send.OkAsync(cancellation: ct);
}
