using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record DocumentoVentaLineaCommand(
    Guid ProductoId,
    decimal Cantidad,
    decimal? PrecioUnitario = null,
    decimal MontoDescuento = 0,
    bool DevuelveInventario = false,
    Guid? Id = null,
    string? Descripcion = null);

public sealed record DocumentoVentaPagoCommand(
    string MonedaCodigo,
    decimal TipoCambioAplicado,
    string MedioPagoCodigo,
    decimal MontoEntregado,
    decimal MontoAplicadoMonedaPago,
    decimal MontoAplicadoDocumento,
    decimal MontoVueltoMonedaPago,
    decimal MontoVueltoDocumento,
    string? Referencia = null,
    string? Observacion = null);

internal sealed record LineaPreparada(
    Guid ProductoId,
    TipoItem TipoItem,
    string Codigo,
    string Descripcion,
    string UnidadMedidaCodigo,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal MontoDescuento,
    string? TarifaIvaImpuestoCodigo,
    decimal PorcentajeImpuesto,
    bool DevuelveInventario,
    bool NoAplicaExistencias,
    bool PermiteModificarPrecioUnitario);
