export interface MovimientoStockDto {
    id: string;
    productoId: string;
    nombreProducto: string;
    fechaUtc: string;
    tipoDocumentoOrigen: string | null;
    documentoVentaId: string | null;
    consecutivoDocumento: string | null;
    delta: number;
    saldoResultante: number;
    usuarioId: string | null;
    razon: string | null;
}

export interface ObtenerMovimientosStockResult {
    items: MovimientoStockDto[];
    total: number;
}

export interface ObtenerMovimientosStockParams {
    productoId?: string;
    pagina: number;
    tamano: number;
}
