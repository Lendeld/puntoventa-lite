using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Queries.Cajas;

public sealed record ListarCajasQuery : IRequest<ErrorOr<IReadOnlyList<CajaListadoItem>>>;

public sealed record CajaListadoItem(
    Guid Id,
    string Codigo,
    string Nombre,
    bool Activo);

public sealed class ListarCajasHandler(
    ICajaRepository cajaRepository) : IRequestHandler<ListarCajasQuery, ErrorOr<IReadOnlyList<CajaListadoItem>>>
{
    private readonly ICajaRepository _cajaRepository = cajaRepository;

    public async ValueTask<ErrorOr<IReadOnlyList<CajaListadoItem>>> Handle(ListarCajasQuery query, CancellationToken cancellationToken)
    {
        var cajas = await _cajaRepository.ObtenerTodasAsync(cancellationToken);
        IReadOnlyList<CajaListadoItem> items = cajas
            .Select(c => new CajaListadoItem(c.Id, c.Codigo, c.Nombre, c.Activo))
            .ToList();
        return ErrorOrFactory.From(items);
    }
}
