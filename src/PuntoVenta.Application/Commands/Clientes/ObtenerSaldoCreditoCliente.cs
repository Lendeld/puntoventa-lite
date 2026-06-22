using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Clientes;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.Application.Commands.Clientes;

public sealed record ObtenerSaldoCreditoClienteQuery(Guid ClienteId) : IRequest<ErrorOr<SaldoCreditoClienteDto>>;

public sealed class ObtenerSaldoCreditoClienteHandler(
    IFechaActual fechaActual,
    IClienteRepository clienteRepository,
    IDocumentoVentaRepository documentoRepository) : IRequestHandler<ObtenerSaldoCreditoClienteQuery, ErrorOr<SaldoCreditoClienteDto>>
{
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;

    public async ValueTask<ErrorOr<SaldoCreditoClienteDto>> Handle(ObtenerSaldoCreditoClienteQuery query, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByIdAsync(query.ClienteId, cancellationToken);
        if (cliente is null)
        {
            return ClienteErrors.NoEncontrado;
        }

        var (SaldoVigente, SaldoVencido, FacturasVencidas, DiasAtrasoMax) = await _documentoRepository.ObtenerSaldosCreditoClienteAsync(cliente.Id, _fechaActual.AhoraUtc, cancellationToken);

        return new SaldoCreditoClienteDto(
            cliente.Id,
            SaldoVigente,
            SaldoVencido,
            FacturasVencidas > 0,
            FacturasVencidas,
            DiasAtrasoMax);
    }
}
