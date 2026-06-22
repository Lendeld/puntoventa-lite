"use client";

import { VENTA_FIELDS } from "@lib/constants/ventas.constants";
import type { CrearBorradorFacturaFormValues } from "@lib/types/ventas.types";
import {
    buildEmptyPago,
    resolveDefaultMedioPagoCodigo,
} from "@lib/utils/facturacion.utils";
import { recalcularPagosDocumento } from "@lib/utils/ventas.utils";
import type { UseFormReturnType } from "@mantine/form";
import { useState } from "react";

interface UseFacturacionCobroParams {
    form: UseFormReturnType<CrearBorradorFacturaFormValues>;
    mediosPago: Array<{ codigo: string; detalle: string }>;
    monedaCodigo: string;
    tipoCambio: number;
    totalTotal: number;
    isEmitting: () => boolean;
    onValidationError: () => void;
}

export function useFacturacionCobro({
    form,
    mediosPago,
    monedaCodigo,
    tipoCambio,
    totalTotal,
    isEmitting,
    onValidationError,
}: UseFacturacionCobroParams) {
    const [cobroModalOpen, setCobroModalOpen] = useState(false);
    const [cobroModo, setCobroModo] = useState<"rapido" | "detallado">("rapido");

    const pagos = form.values[VENTA_FIELDS.PAGOS];

    function commitPagos(nextPagos: typeof pagos) {
        form.setFieldValue(
            VENTA_FIELDS.PAGOS,
            recalcularPagosDocumento(nextPagos, monedaCodigo, tipoCambio, totalTotal),
        );
    }

    function clearCobroErrors() {
        form.setErrors(
            Object.fromEntries(
                Object.entries(form.errors).filter(
                    ([key]) => !key.startsWith(VENTA_FIELDS.PAGOS),
                ),
            ),
        );
    }

    function resetCobroPagos() {
        form.setFieldValue(VENTA_FIELDS.PAGOS, []);
        clearCobroErrors();
    }

    function addPagoVacio(medioPagoCodigo?: string) {
        commitPagos([
            ...pagos,
            {
                ...buildEmptyPago(monedaCodigo),
                MedioPagoCodigo: medioPagoCodigo ?? "",
            },
        ]);
    }

    function removePago(index: number) {
        commitPagos(pagos.filter((_, currentIndex) => currentIndex !== index));
    }

    function updatePago(
        index: number,
        field: keyof (typeof pagos)[number],
        value: unknown,
    ) {
        commitPagos(
            pagos.map((pago, currentIndex) =>
                currentIndex === index ? { ...pago, [field]: value } : pago,
            ),
        );
    }

    function prepararPagoRapido() {
        const medioPagoCodigo = resolveDefaultMedioPagoCodigo(mediosPago);
        commitPagos([
            {
                ...buildEmptyPago(monedaCodigo),
                MedioPagoCodigo: medioPagoCodigo,
                MonedaCodigo: monedaCodigo,
                MontoEntregado: totalTotal,
            },
        ]);
    }

    function abrirCobroRapido() {
        if (form.validate().hasErrors) {
            onValidationError();
            return;
        }

        clearCobroErrors();
        prepararPagoRapido();
        setCobroModo("rapido");
        setCobroModalOpen(true);
    }

    function abrirCobroDetallado() {
        if (form.validate().hasErrors) {
            onValidationError();
            return;
        }

        resetCobroPagos();
        setCobroModo("detallado");
        setCobroModalOpen(true);
    }

    function closeCobroModal() {
        if (isEmitting()) return;

        setCobroModalOpen(false);
        setCobroModo("rapido");
        resetCobroPagos();
    }

    return {
        cobroModalOpen,
        setCobroModalOpen,
        cobroModo,
        setCobroModo,
        addPagoVacio,
        removePago,
        updatePago,
        clearCobroErrors,
        resetCobroPagos,
        abrirCobroRapido,
        abrirCobroDetallado,
        closeCobroModal,
    };
}
