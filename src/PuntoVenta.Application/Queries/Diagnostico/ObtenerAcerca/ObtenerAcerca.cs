using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Queries.Diagnostico.ObtenerAcerca;

public sealed record ObtenerAcercaQuery : IRequest<ErrorOr<AcercaDto>>;

public sealed record AcercaDto(
    string BackendVersion,
    string? BackendCommitSha,
    string Ambiente);

public sealed class ObtenerAcercaHandler(
    IInfoSistema infoSistema) : IRequestHandler<ObtenerAcercaQuery, ErrorOr<AcercaDto>>
{
    private readonly IInfoSistema _infoSistema = infoSistema;

    public ValueTask<ErrorOr<AcercaDto>> Handle(ObtenerAcercaQuery query, CancellationToken cancellationToken)
    {
        var dto = new AcercaDto(
            BackendVersion: _infoSistema.BackendVersion,
            BackendCommitSha: _infoSistema.BackendCommitSha,
            Ambiente: _infoSistema.Ambiente);

        return ValueTask.FromResult<ErrorOr<AcercaDto>>(dto);
    }
}
