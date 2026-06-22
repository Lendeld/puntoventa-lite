"use client";

import { IconArrowsExchange, IconChartBar, IconReportAnalytics } from "@tabler/icons-react";
import { ROUTES } from "@lib/constants/routes.constants";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { ReportesHub } from "@ui/reportes/ReportesHub";
import type { ReporteEntrada } from "@ui/reportes/ReporteCard";

const REPORTES: ReporteEntrada[] = [
    {
        titulo: "Ventas por rango",
        descripcion:
            "Ventas por fecha de factura con desglose línea a línea o resumido por documento. Colonización histórica y notas de crédito en negativo.",
        href: ROUTES.REPORTES_VENTAS_RANGO,
        capacidades: ["Detallado / Resumido", "Colonizar", "Exporta a Excel"],
        icono: IconChartBar,
        permiso: PERMISOS.REPORTES_VENTAS_RANGO_VER,
    },
    {
        titulo: "Movimientos de dinero",
        descripcion:
            "Consulta abonos y anulaciones por fecha real de registro, con totales por medio de pago y trazabilidad al documento.",
        href: ROUTES.REPORTES_MOVIMIENTOS_DINERO,
        capacidades: ["Entradas / Salidas", "Totales por medio", "Trazabilidad por pago"],
        icono: IconArrowsExchange,
        permiso: PERMISOS.REPORTES_VER,
    },
];

export function ReportesVentasHubSection() {
    return (
        <ReportesHub
            titulo="Reportes Ventas"
            descripcion="Consultas analíticas sobre las ventas. Elige un reporte para configurar filtros y exportar resultados."
            icono={IconReportAnalytics}
            reportes={REPORTES}
        />
    );
}
