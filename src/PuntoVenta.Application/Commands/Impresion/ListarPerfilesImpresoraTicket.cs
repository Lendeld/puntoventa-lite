using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Impresion;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Impresion;

public sealed record ListarPerfilesImpresoraTicketQuery
    : IRequest<ErrorOr<IReadOnlyList<PerfilImpresoraTicketDto>>>;

public sealed class ListarPerfilesImpresoraTicketHandler(IPerfilImpresoraTicketRepository repository)
        : IRequestHandler<ListarPerfilesImpresoraTicketQuery, ErrorOr<IReadOnlyList<PerfilImpresoraTicketDto>>>
{
    private readonly IPerfilImpresoraTicketRepository _repository = repository;

    public async ValueTask<ErrorOr<IReadOnlyList<PerfilImpresoraTicketDto>>> Handle(
        ListarPerfilesImpresoraTicketQuery query,
        CancellationToken cancellationToken)
    {
        var perfiles = await _repository.ListarActivosAsync(cancellationToken);
        IReadOnlyList<PerfilImpresoraTicketDto> resultado = [.. perfiles.Select(PerfilImpresoraTicketMapper.ToDto)];
        return ErrorOrFactory.From(resultado);
    }
}
