using PuntoVenta.Domain.Common;

namespace PuntoVenta.Domain.Entities.Ventas;

// Totales del comprobante redondeados al borde (2 dec, monto a pagar/declarar).
// Fuente única del "monto que ve el cliente / se declara a Hacienda / suma en
// reportes". Internamente el documento guarda 5 dec (ver [[project_precision_monetaria]]
// y Dinero); este value object aplica el redondeo de borde UNA sola vez y de
// forma consistente, garantizando por construcción las dos invariantes que
// Hacienda valida en el resumen:
//
//   TotalVenta − TotalDescuentos = TotalVentaNeta
//   TotalVentaNeta + TotalImpuesto = TotalComprobante
//
// Reusable por: PDF/ticket, detalle, y el mapeo del XML/FE cuando se implemente.
// Los reportes que agregan en SQL aplican el mismo redondeo (ROUND(x,2)) por
// documento antes de sumar, para que el total del reporte cuadre con la suma de
// los montos mostrados fila por fila.
public readonly record struct MontosComprobante
{
    public decimal TotalVenta { get; init; }
    public decimal TotalDescuentos { get; init; }
    public decimal TotalVentaNeta { get; init; }
    public decimal TotalImpuesto { get; init; }
    public decimal TotalComprobante { get; init; }

    public static MontosComprobante Desde(DocumentoVenta documento)
    {
        // Redondeo independiente de los componentes brutos; las netas y el total
        // se derivan por resta/suma para que las invariantes se cumplan exacto
        // sin residuo sub-céntimo (un componente redondeado nunca contradice al
        // agregado del que forma parte).
        var venta = Dinero.RedondearPago(documento.TotalVenta);
        var descuentos = Dinero.RedondearPago(documento.TotalDescuentos);
        var impuesto = Dinero.RedondearPago(documento.TotalImpuesto);
        var ventaNeta = venta - descuentos;

        return new MontosComprobante
        {
            TotalVenta = venta,
            TotalDescuentos = descuentos,
            TotalVentaNeta = ventaNeta,
            TotalImpuesto = impuesto,
            TotalComprobante = ventaNeta + impuesto
        };
    }
}
