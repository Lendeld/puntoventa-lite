using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.MediosPago;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.MediosPago;

public sealed record ObtenerListaMedioPagoQuery(bool? Activo = null) : IRequest<ErrorOr<IReadOnlyList<MedioPagoDto>>>;

public sealed class ObtenerListaMedioPagoHandler(IMedioPagoRepository repository) : IRequestHandler<ObtenerListaMedioPagoQuery, ErrorOr<IReadOnlyList<MedioPagoDto>>>
{
    private readonly IMedioPagoRepository _repository = repository;

    public async ValueTask<ErrorOr<IReadOnlyList<MedioPagoDto>>> Handle(ObtenerListaMedioPagoQuery query, CancellationToken cancellationToken)
    {
        var items = await _repository.ObtenerListaAsync(query.Activo, cancellationToken);
        return items.Select(MedioPagoMapper.ToDto).ToList();
    }
}
