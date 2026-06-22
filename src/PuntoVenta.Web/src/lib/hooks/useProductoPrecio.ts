"use client";

import { useState } from "react";
import { PRODUCTO_FIELDS } from "@lib/constants/productos.constants";
import { useTarifasIvaActivasQuery } from "@lib/hooks/useTarifasIvaActivasQuery";
import { redondear } from "@lib/utils/number.utils";
import type { TarifaIvaImpuestoDto } from "@lib/types/configuracion.types";

interface FormLike {
    values: Record<string, unknown>;
    setFieldValue: (field: string, value: unknown) => void;
}

interface UseProductoPrecioReturn {
    precioVenta: number | string;
    porcentaje: number | null;
    tarifasData: TarifaIvaImpuestoDto[] | undefined;
    initFromProducto: (unitario: number, tarifaCodigo: string | undefined) => void;
    handleTarifaChange: (val: string | null) => void;
    handlePrecioUnitarioChange: (val: number | string) => void;
    handlePrecioVentaChange: (val: number | string) => void;
}

export function useProductoPrecio(form: FormLike): UseProductoPrecioReturn {
    const [precioVenta, setPrecioVenta] = useState<number | string>("");
    const { data: tarifasData } = useTarifasIvaActivasQuery();

    const tarifaCodigo = form.values[PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO] as string | undefined;
    const precioUnitario = (form.values[PRODUCTO_FIELDS.PRECIO_UNITARIO] as number) ?? 0;

    const tarifaData = tarifasData?.find((t) => t.codigo === tarifaCodigo);
    const porcentaje = tarifaData?.porcentaje ?? 0;

    function initFromProducto(unitario: number, tarifa: string | undefined) {
        const t = tarifasData?.find((x) => x.codigo === tarifa);
        const pct = t?.porcentaje ?? 0;
        setPrecioVenta(redondear(unitario * (1 + pct / 100)));
    }

    function handleTarifaChange(val: string | null) {
        form.setFieldValue(PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO, val ?? undefined);
        const newTarifa = tarifasData?.find((t) => t.codigo === val);
        const pct = newTarifa?.porcentaje ?? 0;
        if (precioUnitario > 0) {
            setPrecioVenta(redondear(precioUnitario * (1 + pct / 100)));
        }
    }

    function handlePrecioUnitarioChange(val: number | string) {
        const n = typeof val === "number" ? val : parseFloat(String(val)) || 0;
        form.setFieldValue(PRODUCTO_FIELDS.PRECIO_UNITARIO, n);
        setPrecioVenta(redondear(n * (1 + porcentaje / 100)));
    }

    function handlePrecioVentaChange(val: number | string) {
        const n = typeof val === "number" ? val : parseFloat(String(val)) || 0;
        setPrecioVenta(n);
        const unitario = porcentaje === 0 ? n : redondear(n / (1 + porcentaje / 100));
        form.setFieldValue(PRODUCTO_FIELDS.PRECIO_UNITARIO, unitario);
    }

    return {
        precioVenta,
        porcentaje: tarifaData?.porcentaje ?? null,
        tarifasData,
        initFromProducto,
        handleTarifaChange,
        handlePrecioUnitarioChange,
        handlePrecioVentaChange,
    };
}
