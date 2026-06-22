using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Negocios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Negocio;

public sealed class SubirLogoNegocioEndpoint(IMediator mediator) : EndpointWithoutRequest<string>
{
    private static readonly HashSet<string> _tiposPermitidos = ["image/jpeg", "image/png", "image/webp"];

    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/negocio/logo");
        Tags("Negocio");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.NegocioEditar));
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Subir logo del negocio";
            s.Description = "Sube o reemplaza el logo del negocio";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var file = Files.FirstOrDefault();

        if (file is null)
        {
            await Send.ResultAsync(new List<Error> { Error.Validation("Logo", "Se requiere un archivo.") }.ToProblem());
            return;
        }

        if (!_tiposPermitidos.Contains(file.ContentType))
        {
            await Send.ResultAsync(new List<Error> { Error.Validation("Logo", "Solo se permiten imágenes JPEG, PNG o WebP.") }.ToProblem());
            return;
        }

        using var stream = file.OpenReadStream();

        var command = new SubirLogoNegocioCommand(stream, file.FileName, file.ContentType);
        var result = await _mediator.Send(command, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
