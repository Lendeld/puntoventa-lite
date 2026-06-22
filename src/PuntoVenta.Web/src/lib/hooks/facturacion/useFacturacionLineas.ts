"use client";

import { VENTA_FIELDS } from "@lib/constants/ventas.constants";
import type { ProductoDto } from "@lib/types/productos.types";
import type { TarifaIvaImpuestoDto } from "@lib/types/configuracion.types";
import type {
    CrearBorradorFacturaFormValues,
    DocumentoVentaLineaForm,
} from "@lib/types/ventas.types";
import { redondear } from "@lib/utils/number.utils";
import { toBaseCrc, toMonedaDocumento } from "@lib/utils/facturacion.utils";
import {
    calcularTotalesFactura,
    obtenerPorcentajeImpuesto,
    recalcularPagosDocumento,
} from "@lib/utils/ventas.utils";
import type { UseFormReturnType } from "@mantine/form";
import { useState } from "react";

interface UseFacturacionLineasParams {
    form: UseFormReturnType<CrearBorradorFacturaFormValues>;
    monedaCodigo: string;
    tipoCambio: number;
    tarifasIva: TarifaIvaImpuestoDto[];
}

export function useFacturacionLineas({
    form,
    monedaCodigo,
    tipoCambio,
    tarifasIva,
}: UseFacturacionLineasParams) {
    const [selectedProductoId, setSelectedProductoId] = useState<string | null>(
        null,
    );
    const [selectedProducto, setSelectedProducto] =
        useState<ProductoDto | null>(null);

    const lineas = form.values[VENTA_FIELDS.LINEAS] as DocumentoVentaLineaForm[];
    const pagos = form.values[VENTA_FIELDS.PAGOS];

    function commitLineas(nextLineas: DocumentoVentaLineaForm[]) {
        form.setFieldValue(VENTA_FIELDS.LINEAS, nextLineas);
        form.setFieldValue(
            VENTA_FIELDS.PAGOS,
            recalcularPagosDocumento(
                pagos,
                monedaCodigo,
                tipoCambio,
                calcularTotalesFactura(nextLineas, []).total,
            ),
        );
    }

    function clearSeleccion() {
        setSelectedProductoId(null);
        setSelectedProducto(null);
    }

    function addLinea(productoOverride?: ProductoDto) {
        const producto = productoOverride ?? selectedProducto;
        if (!producto) return;

        const indiceExistente = lineas.findIndex(
            (l) => l.ProductoId === producto.id,
        );
        if (indiceExistente !== -1) {
            commitLineas(
                lineas.map((l, i) =>
                    i === indiceExistente ? { ...l, Cantidad: l.Cantidad + 1 } : l,
                ),
            );
            clearSeleccion();
            return;
        }

        const porcentajeImpuesto = obtenerPorcentajeImpuesto(
            null,
            producto.tarifaIvaImpuestoCodigo,
            tarifasIva,
        );
        const precioUnitarioBaseCrc = redondear(producto.precioUnitario);
        const precioUnitario = toMonedaDocumento(
            precioUnitarioBaseCrc,
            monedaCodigo,
            tipoCambio,
        );

        commitLineas([
            ...lineas,
            {
                ProductoId: producto.id,
                TipoItem: producto.tipoItem,
                Codigo: producto.codigo,
                Descripcion: producto.nombre,
                Cantidad: 1,
                PrecioUnitario: precioUnitario,
                MontoDescuento: 0,
                PrecioUnitarioBaseCrc: precioUnitarioBaseCrc,
                MontoDescuentoBaseCrc: 0,
                TarifaIvaImpuestoCodigo: producto.tarifaIvaImpuestoCodigo,
                PorcentajeImpuesto: porcentajeImpuesto,
                PermiteModificarPrecioUnitario:
                    producto.permiteModificarPrecioUnitario,
            },
        ]);
        clearSeleccion();
    }

    function removeLinea(index: number) {
        commitLineas(lineas.filter((_, currentIndex) => currentIndex !== index));
    }

    function updateLinea(
        index: number,
        field: keyof DocumentoVentaLineaForm,
        value: unknown,
    ) {
        const nextLineas = lineas.map((linea, currentIndex) => {
            if (currentIndex !== index) return linea;

            const nextLinea = { ...linea, [field]: value };
            const numeric =
                typeof value === "number" ? value : parseFloat(String(value)) || 0;

            if (field === "PrecioUnitario") {
                nextLinea.PrecioUnitarioBaseCrc = toBaseCrc(
                    numeric,
                    monedaCodigo,
                    tipoCambio,
                );
            }

            if (field === "MontoDescuento") {
                nextLinea.MontoDescuentoBaseCrc = toBaseCrc(
                    numeric,
                    monedaCodigo,
                    tipoCambio,
                );
            }

            return nextLinea;
        });

        commitLineas(nextLineas);
    }

    return {
        lineas,
        selectedProductoId,
        selectedProducto,
        setSelectedProductoId,
        setSelectedProducto,
        addLinea,
        removeLinea,
        updateLinea,
    };
}
