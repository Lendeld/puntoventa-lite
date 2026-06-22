using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Vendedores;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Vendedores;

public sealed record ObtenerListaPaginadoVendedorQuery(
    int NumeroPagina = 1,
    int TamanoPagina = 10,
    string? FiltroDinamico = null,
    bool? Activo = null) : IRequest<ErrorOr<PagedResult<VendedorDto>>>;

public sealed class ObtenerListaPaginadoVendedorHandler(IVendedorRepository vendedorRepository) : IRequestHandler<ObtenerListaPaginadoVendedorQuery, ErrorOr<PagedResult<VendedorDto>>>
{
    private readonly IVendedorRepository _vendedorRepository = vendedorRepository;

    public async ValueTask<ErrorOr<PagedResult<VendedorDto>>> Handle(ObtenerListaPaginadoVendedorQuery query, CancellationToken cancellationToken)
    {
        var (pagina, tamano) = Paginacion.Normalizar(query.NumeroPagina, query.TamanoPagina);

        var (items, total) = await _vendedorRepository.ObtenerListaPaginadoAsync(
            pagina,
            tamano,
            query.FiltroDinamico,
            query.Activo,
            cancellationToken);

        return PagedResult<VendedorDto>.Crear([.. items.Select(VendedorMapper.ToDto)], pagina, tamano, total);
    }
}
