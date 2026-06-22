using PuntoVenta.Domain.Common.Events;

namespace PuntoVenta.Domain.Entities.Ventas.Eventos;

public sealed record NotaCreditoEmitidaEvento(
    Guid DocumentoVentaId,
    string Consecutivo,
    decimal TotalComprobante,
    string MonedaCodigo,
    Guid DocumentoOrigenId) : IDomainEvent;
