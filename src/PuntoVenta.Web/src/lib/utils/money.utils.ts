const CURRENCY_FORMAT = new Intl.NumberFormat("es-CR", {
    style: "currency",
    currency: "CRC",
    maximumFractionDigits: 0,
});

const NUMBER_FORMAT = new Intl.NumberFormat("es-CR");

/** Formato moneda CR (₡) sin decimales para KPIs del dashboard. */
export function formatMoneda(value: number | null | undefined): string {
    return CURRENCY_FORMAT.format(value ?? 0);
}

/** Formato numérico CR con separadores de miles. */
export function formatNumero(value: number | null | undefined): string {
    return NUMBER_FORMAT.format(value ?? 0);
}
