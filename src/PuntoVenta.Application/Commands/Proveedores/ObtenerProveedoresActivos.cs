using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Proveedores;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Proveedores;

public sealed record ObtenerProveedoresActivosQuery : IRequest<ErrorOr<IReadOnlyList<ProveedorDto>>>;

public sealed class ObtenerProveedoresActivosHandler(IProveedorRepository proveedorRepository) : IRequestHandler<ObtenerProveedoresActivosQuery, ErrorOr<IReadOnlyList<ProveedorDto>>>
{
    private readonly IProveedorRepository _proveedorRepository = proveedorRepository;

    public async ValueTask<ErrorOr<IReadOnlyList<ProveedorDto>>> Handle(ObtenerProveedoresActivosQuery query, CancellationToken cancellationToken)
    {
        var items = await _proveedorRepository.ObtenerActivosAsync(cancellationToken);
        return items.Select(ProveedorMapper.ToDto).ToList();
    }
}
