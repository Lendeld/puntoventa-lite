namespace PuntoVenta.Application.DTOs.Clientes;

public sealed record SaldoCreditoClienteDto(
    Guid ClienteId,
    decimal SaldoVigente,
    decimal SaldoVencido,
    bool EsMoroso,
    int FacturasVencidas,
    int DiasAtrasoMax);
