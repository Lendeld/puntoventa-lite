"use client";

import { IconReportAnalytics, IconReportMoney } from "@tabler/icons-react";
import { ROUTES } from "@lib/constants/routes.constants";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { ReportesHub } from "@ui/reportes/ReportesHub";
import type { ReporteEntrada } from "@ui/reportes/ReporteCard";

const REPORTES: ReporteEntrada[] = [
    {
        titulo: "Existencias",
        descripcion:
            "Stock actual valorizado por producto (existencia, costo, precio e impuesto). Exporta a Excel.",
        href: ROUTES.REPORTES_INVENTARIO_EXISTENCIAS,
        capacidades: ["Filtra por código/categoría", "Valorizado", "Exporta a Excel"],
        icono: IconReportMoney,
        permiso: PERMISOS.REPORTES_INVENTARIO_VER,
    },
];

export function ReportesInventarioHubSection() {
    return (
        <ReportesHub
            titulo="Reportes Inventario"
            descripcion="Consultas analíticas sobre el inventario. Elige un reporte para configurar filtros y exportar resultados."
            icono={IconReportAnalytics}
            reportes={REPORTES}
        />
    );
}
