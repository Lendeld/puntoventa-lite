using PuntoVenta.Domain.Entities.Negocios;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Interfaces;

public interface IDocumentoVentaPdfService
{
    Task<byte[]> GenerarPdfAsync(DocumentoVenta documento, Negocio negocio, NegocioTicketConfig? ticketConfig, CancellationToken cancellationToken = default);
    Task<byte[]> GenerarReciboPagoPdfAsync(DocumentoVenta documento, DocumentoVentaPago pago, Negocio negocio, NegocioTicketConfig? ticketConfig, decimal montoNotasCreditoAplicadas = 0m, CancellationToken cancellationToken = default);
}
