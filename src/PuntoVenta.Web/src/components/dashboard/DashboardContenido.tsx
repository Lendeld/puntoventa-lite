"use client";

import { Alert, Skeleton, Stack } from "@mantine/core";
import {
    IconAlertCircle,
    IconCalendarStats,
    IconReceipt2,
} from "@tabler/icons-react";
import { useDashboardResumenQuery } from "@lib/hooks/useDashboardResumenQuery";
import { formatMoneda, formatNumero } from "@lib/utils/money.utils";
import type { VentasMesDto } from "@lib/types/dashboard.types";
import { KpiCard } from "./KpiCard";
import { VentasTendenciaChart } from "./VentasTendenciaChart";
import { MetodosPagoChart } from "./MetodosPagoChart";
import { TopProductosList } from "./TopProductosList";
import { CuentasPorCobrarCard } from "./CuentasPorCobrarCard";

// Subtítulo del KPI mensual: muestra la diferencia absoluta con signo vs el
// mismo período del mes anterior. Cuando el mes anterior no tuvo ventas no hay
// base de comparación, así que se indica explícitamente en vez de un % vacío.
function construirSubtituloMes(mes: VentasMesDto): string {
    if (mes.totalMesAnterior <= 0) return "Mes anterior sin ventas";
    const diff = mes.total - mes.totalMesAnterior;
    const signo = diff >= 0 ? "+" : "-";
    return `${signo}${formatMoneda(Math.abs(diff))} vs mes anterior`;
}

export function DashboardContenido() {
    const { data, isLoading, isError } = useDashboardResumenQuery();

    if (isLoading) {
        return (
            <Stack gap="lg">
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-12">
                    <Skeleton h={150} radius="lg" className="xl:col-span-4" />
                    <Skeleton h={150} radius="lg" className="xl:col-span-3" />
                    <Skeleton h={150} radius="lg" className="xl:col-span-5" />
                </div>
                <div className="grid grid-cols-1 gap-4 xl:grid-cols-12">
                    <Skeleton h={332} radius="lg" className="xl:col-span-7" />
                    <Skeleton h={332} radius="lg" className="xl:col-span-5" />
                </div>
                <div className="grid grid-cols-1 gap-4 xl:grid-cols-12">
                    <Skeleton h={244} radius="lg" className="xl:col-span-12" />
                </div>
            </Stack>
        );
    }

    if (isError || !data) {
        return (
            <Alert
                color="yellow"
                variant="light"
                icon={<IconAlertCircle size={18} />}
                title="No se pudo cargar el panel"
            >
                Ocurrió un error al obtener el resumen. Intenta de nuevo más tarde.
            </Alert>
        );
    }

    return (
        <Stack gap="lg">
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-12">
                <KpiCard
                    label="Ventas hoy"
                    value={formatMoneda(data.hoy.total)}
                    subtitle={`${formatNumero(data.hoy.cantidad)} factura(s)`}
                    icon={<IconReceipt2 size={18} />}
                    variant="featured"
                    className="xl:col-span-4"
                />
                <KpiCard
                    label="Ventas del mes"
                    value={formatMoneda(data.mes.total)}
                    delta={data.mes.porcentajeCambio}
                    subtitle={construirSubtituloMes(data.mes)}
                    icon={<IconCalendarStats size={18} />}
                    className="xl:col-span-3"
                />
                <CuentasPorCobrarCard data={data.cobros} className="xl:col-span-5" />
            </div>

            <div className="grid grid-cols-1 gap-4 xl:grid-cols-12">
                <div className="xl:col-span-7">
                    <VentasTendenciaChart data={data.tendencia} />
                </div>
                <div className="xl:col-span-5">
                    <MetodosPagoChart data={data.metodosPago} />
                </div>
            </div>

            <div className="grid grid-cols-1 gap-4 xl:grid-cols-12">
                <div className="xl:col-span-12">
                    <TopProductosList data={data.topProductos} />
                </div>
            </div>
        </Stack>
    );
}
