using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Vendedores;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Vendedores;

namespace PuntoVenta.Application.Commands.Vendedores;

public sealed record ObtenerVendedorPorIdQuery(Guid Id) : IRequest<ErrorOr<VendedorDto>>;

public sealed class ObtenerVendedorPorIdHandler(IVendedorRepository vendedorRepository) : IRequestHandler<ObtenerVendedorPorIdQuery, ErrorOr<VendedorDto>>
{
    private readonly IVendedorRepository _vendedorRepository = vendedorRepository;

    public async ValueTask<ErrorOr<VendedorDto>> Handle(ObtenerVendedorPorIdQuery query, CancellationToken cancellationToken)
    {
        var vendedor = await _vendedorRepository.ObtenerPorIdConAuditoriaAsync(query.Id, cancellationToken);
        return vendedor is null ? VendedorErrors.NoEncontrado : VendedorMapper.ToDto(vendedor);
    }
}
