using FastEndpoints;

namespace PuntoVenta.API.Endpoints.Auth;

public sealed class ValidarTokenEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/auth/validar-token");
        Tags("Auth");
        Summary(summary =>
        {
            summary.Summary = "Validar token";
            summary.Description = "Verifica si el JWT actual es válido y no está en lista negra";
        });
    }

    public override Task HandleAsync(CancellationToken ct) => Send.OkAsync(cancellation: ct);
}
