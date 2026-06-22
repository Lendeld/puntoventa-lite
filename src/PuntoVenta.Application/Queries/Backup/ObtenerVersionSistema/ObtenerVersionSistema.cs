using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Queries.Backup.ObtenerVersionSistema;

public sealed record ObtenerVersionSistemaQuery : IRequest<ErrorOr<string>>;

/// <summary>
/// Devuelve la versión del sistema (informativa, legible para el usuario) que se muestra
/// en la pantalla de respaldo. La compatibilidad de los backups se valida internamente por
/// el identificador de migración EF (ver ValidarBackup), no por esta versión.
/// </summary>
public sealed class ObtenerVersionSistemaHandler(
    IInfoSistema infoSistema) : IRequestHandler<ObtenerVersionSistemaQuery, ErrorOr<string>>
{
    private readonly IInfoSistema _infoSistema = infoSistema;

    public ValueTask<ErrorOr<string>> Handle(
        ObtenerVersionSistemaQuery query,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<ErrorOr<string>>(_infoSistema.BackendVersion);
}
