namespace PuntoVenta.Domain.Common;

// Fuente única de redondeo monetario del sistema. Evita tener round(x, 2) vs
// round(x, 5) dispersos. Espejo exacto en el frontend: number.utils.ts
// (redondear = EscalaInterna, redondearMoneda = EscalaPago).
//
// Modelo: se calcula y almacena con precisión interna (5 dec, lo que permite
// Hacienda) y se redondea a 2 dec SOLO en el borde (display y total a pagar).
public static class Dinero
{
    // Precisión interna de cálculo/almacenamiento (numeric(18,5)).
    public const int EscalaInterna = 5;

    // Precisión del monto a pagar / mostrar (colón y dólar usan 2 decimales).
    public const int EscalaPago = 2;

    // Redondeo interno (5 dec, half-away-from-zero). Para precio, subtotal,
    // impuesto y totales antes de mostrarse.
    public static decimal Redondear(decimal valor)
        => decimal.Round(valor, EscalaInterna, MidpointRounding.AwayFromZero);

    // Redondeo al monto a pagar / mostrar (2 dec, half-away-from-zero).
    public static decimal RedondearPago(decimal valor)
        => decimal.Round(valor, EscalaPago, MidpointRounding.AwayFromZero);

    // Delta de redondeo: lo que se ajusta al llevar el total preciso al monto a
    // pagar (los "decimales perdidos"). RedondearPago(total) = total + Redondeo(total).
    public static decimal Redondeo(decimal total)
        => RedondearPago(total) - total;
}
