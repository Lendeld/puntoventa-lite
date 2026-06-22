export interface VentasPeriodoDto {
    total: number;
    cantidad: number;
}

export interface VentasMesDto {
    total: number;
    cantidad: number;
    totalMesAnterior: number;
    porcentajeCambio: number | null;
}

export interface PuntoTendenciaDto {
    fecha: string;
    total: number;
}

export interface MetodoPagoDto {
    codigo: string;
    detalle: string;
    total: number;
}

export interface TopProductoDto {
    nombre: string;
    cantidad: number;
    total: number;
}

export interface CuentasPorCobrarDto {
    totalVencido: number;
    cantidadVencidas: number;
}

export interface ResumenDashboardDto {
    hoy: VentasPeriodoDto;
    mes: VentasMesDto;
    tendencia: PuntoTendenciaDto[];
    metodosPago: MetodoPagoDto[];
    topProductos: TopProductoDto[];
    cobros: CuentasPorCobrarDto;
}
