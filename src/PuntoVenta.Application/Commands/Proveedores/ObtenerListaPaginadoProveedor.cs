using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Proveedores;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Proveedores;

public sealed record ObtenerListaPaginadoProveedorQuery(
    int NumeroPagina = 1,
    int TamanoPagina = 10,
    string? FiltroDinamico = null,
    bool? Activo = null) : IRequest<ErrorOr<PagedResult<ProveedorDto>>>;

public sealed class ObtenerListaPaginadoProveedorHandler(IProveedorRepository proveedorRepository) : IRequestHandler<ObtenerListaPaginadoProveedorQuery, ErrorOr<PagedResult<ProveedorDto>>>
{
    private readonly IProveedorRepository _proveedorRepository = proveedorRepository;

    public async ValueTask<ErrorOr<PagedResult<ProveedorDto>>> Handle(ObtenerListaPaginadoProveedorQuery query, CancellationToken cancellationToken)
    {
        var (pagina, tamano) = Paginacion.Normalizar(query.NumeroPagina, query.TamanoPagina);

        var (items, total) = await _proveedorRepository.ObtenerListaPaginadoAsync(
            pagina,
            tamano,
            query.FiltroDinamico,
            query.Activo,
            cancellationToken);

        return PagedResult<ProveedorDto>.Crear([.. items.Select(ProveedorMapper.ToDto)], pagina, tamano, total);
    }
}
