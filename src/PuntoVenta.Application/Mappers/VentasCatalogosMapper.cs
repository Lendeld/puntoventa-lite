using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Mappers;

public static class VentasCatalogosMapper
{
    public static VentaCatalogoItemDto ToCatalogoItem(TipoDocumentoVenta tipo)
        => new((int)tipo, TipoDocumentoDetalle(tipo), TipoDocumentoColor(tipo));

    public static VentaCatalogoItemDto ToCatalogoItem(EstadoDocumentoVenta estado)
        => new((int)estado, EstadoDocumentoDetalle(estado), EstadoDocumentoColor(estado));

    public static string TipoDocumentoDetalle(TipoDocumentoVenta tipo)
        => tipo switch
        {
            TipoDocumentoVenta.Factura => "Factura",
            TipoDocumentoVenta.Apartado => "Apartado",
            TipoDocumentoVenta.NotaCredito => "Nota de crédito",
            TipoDocumentoVenta.NotaDebito => "Nota de débito",
            TipoDocumentoVenta.Proforma => "Proforma",
            _ => $"Tipo {(int)tipo}"
        };

    public static string TipoDocumentoColor(TipoDocumentoVenta tipo)
        => tipo switch
        {
            TipoDocumentoVenta.Factura => "blue",
            TipoDocumentoVenta.Apartado => "orange",
            TipoDocumentoVenta.NotaCredito => "grape",
            TipoDocumentoVenta.NotaDebito => "cyan",
            TipoDocumentoVenta.Proforma => "yellow",
            _ => "gray"
        };

    public static string EstadoDocumentoDetalle(EstadoDocumentoVenta estado)
        => estado switch
        {
            EstadoDocumentoVenta.Borrador => "Borrador",
            EstadoDocumentoVenta.Emitido => "Emitido",
            EstadoDocumentoVenta.Anulado => "Anulado",
            EstadoDocumentoVenta.Reservado => "Reservado",
            EstadoDocumentoVenta.Convertido => "Convertido",
            EstadoDocumentoVenta.Cancelado => "Cancelado",
            EstadoDocumentoVenta.Vencido => "Vencido",
            _ => $"Estado {(int)estado}"
        };

    public static string EstadoDocumentoColor(EstadoDocumentoVenta estado)
        => estado switch
        {
            EstadoDocumentoVenta.Borrador => "yellow",
            EstadoDocumentoVenta.Emitido => "green",
            EstadoDocumentoVenta.Anulado => "red",
            EstadoDocumentoVenta.Reservado => "blue",
            EstadoDocumentoVenta.Convertido => "teal",
            EstadoDocumentoVenta.Cancelado => "gray",
            EstadoDocumentoVenta.Vencido => "orange",
            _ => "gray"
        };
}
