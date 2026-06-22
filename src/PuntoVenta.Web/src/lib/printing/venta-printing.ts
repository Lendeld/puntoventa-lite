"use client";

/** Returns the URL for the PDF version of a venta document. */
export function getVentaPdfUrl(documentoId: string): string {
    return `/pdf/ventas/${documentoId}`;
}

/** Returns the URL for the PDF receipt of a specific payment (abono). */
export function getAbonoPdfUrl(documentoId: string, pagoId: string): string {
    return `/pdf/ventas/${documentoId}/abonos/${pagoId}`;
}
