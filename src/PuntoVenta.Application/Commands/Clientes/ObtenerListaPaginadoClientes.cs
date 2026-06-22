using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Clientes;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Clientes;

public sealed record ObtenerListaPaginadoClientesQuery(
    int NumeroPagina = 1,
    int TamanoPagina = 10,
    string? FiltroDinamico = null,
    bool? Activo = null) : IRequest<ErrorOr<PagedResult<ClienteListaDto>>>;

public sealed class ObtenerListaPaginadoClientesHandler(IClienteRepository clienteRepository) : IRequestHandler<ObtenerListaPaginadoClientesQuery, ErrorOr<PagedResult<ClienteListaDto>>>
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async ValueTask<ErrorOr<PagedResult<ClienteListaDto>>> Handle(ObtenerListaPaginadoClientesQuery query, CancellationToken cancellationToken)
    {
        var (pagina, tamano) = Paginacion.Normalizar(query.NumeroPagina, query.TamanoPagina);

        var (items, total) = await _clienteRepository.ObtenerListaPaginadoAsync(
            pagina,
            tamano,
            query.FiltroDinamico,
            query.Activo,
            cancellationToken);

        return PagedResult<ClienteListaDto>.Crear(
            [.. items.Select(ClienteMapper.ToListaDto)],
            pagina,
            tamano,
            total);
    }
}
