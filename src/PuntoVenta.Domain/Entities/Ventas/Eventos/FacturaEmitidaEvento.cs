using PuntoVenta.Domain.Common.Events;

namespace PuntoVenta.Domain.Entities.Ventas.Eventos;

public sealed record FacturaEmitidaEvento(
    Guid DocumentoVentaId,
    string Consecutivo,
    decimal TotalComprobante,
    string MonedaCodigo,
    Guid? ClienteId,
    bool EsCredito) : IDomainEvent;
