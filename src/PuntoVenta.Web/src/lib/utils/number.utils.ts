const FACTOR = 10 ** 5;

export function redondear(n: number): number {
    return Math.round(n * FACTOR) / FACTOR;
}

// Redondeo monetario a 2 decimales con half-away-from-zero, espejo exacto del
// backend (`decimal.Round(x, 2, MidpointRounding.AwayFromZero)`). Usar para
// todo monto en moneda del documento/pago (subtotales, IVA, totales, montos
// aplicados/vuelto). NO usar `redondear` (5 dec) para dinero: el IVA por línea
// diverge contra el backend y el cobro no cuadra.
export function redondearMoneda(n: number): number {
    return (Math.sign(n) * Math.round(Math.abs(n) * 100)) / 100;
}
