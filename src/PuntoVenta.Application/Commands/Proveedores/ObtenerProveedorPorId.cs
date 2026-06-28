using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Proveedores;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Proveedores;

namespace PuntoVenta.Application.Commands.Proveedores;

public sealed record ObtenerProveedorPorIdQuery(Guid Id) : IRequest<ErrorOr<ProveedorDto>>;

public sealed class ObtenerProveedorPorIdHandler(IProveedorRepository proveedorRepository) : IRequestHandler<ObtenerProveedorPorIdQuery, ErrorOr<ProveedorDto>>
{
    private readonly IProveedorRepository _proveedorRepository = proveedorRepository;

    public async ValueTask<ErrorOr<ProveedorDto>> Handle(ObtenerProveedorPorIdQuery query, CancellationToken cancellationToken)
    {
        var proveedor = await _proveedorRepository.ObtenerPorIdConAuditoriaAsync(query.Id, cancellationToken);

        if (proveedor is null)
        {
            return ProveedorErrors.NoEncontrado;
        }

        return ProveedorMapper.ToDto(proveedor);
    }
}
