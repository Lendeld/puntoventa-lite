using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Vendedores;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Vendedores;

public sealed record ObtenerVendedoresActivosQuery : IRequest<ErrorOr<IReadOnlyList<VendedorActivoDto>>>;

public sealed class ObtenerVendedoresActivosHandler(IVendedorRepository vendedorRepository) : IRequestHandler<ObtenerVendedoresActivosQuery, ErrorOr<IReadOnlyList<VendedorActivoDto>>>
{
    private readonly IVendedorRepository _vendedorRepository = vendedorRepository;

    public async ValueTask<ErrorOr<IReadOnlyList<VendedorActivoDto>>> Handle(ObtenerVendedoresActivosQuery query, CancellationToken cancellationToken)
    {
        var vendedores = await _vendedorRepository.ObtenerActivosAsync(cancellationToken);
        return vendedores.Select(VendedorMapper.ToActivoDto).ToList();
    }
}
