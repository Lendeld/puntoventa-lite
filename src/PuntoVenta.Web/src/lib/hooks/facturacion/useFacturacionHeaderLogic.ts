"use client";

import {
    CONDICIONES_VENTA_CREDITO,
    VENTA_FIELDS,
} from "@lib/constants/ventas.constants";
import type {
    CrearBorradorFacturaFormValues,
    DocumentoVentaLineaForm,
} from "@lib/types/ventas.types";
import {
    normalizarMoneda,
    normalizarTipoCambio,
    toBaseCrc,
    toMonedaDocumento,
} from "@lib/utils/facturacion.utils";
import { calcularTotalesFactura, recalcularPagosDocumento } from "@lib/utils/ventas.utils";
import type { UseFormReturnType } from "@mantine/form";

export function useFacturacionHeaderLogic(
    form: UseFormReturnType<CrearBorradorFacturaFormValues>,
) {
    function handleHeaderChange(field: string, value: unknown) {
        const lineas = form.values[
            VENTA_FIELDS.LINEAS
        ] as DocumentoVentaLineaForm[];
        const pagos = form.values[VENTA_FIELDS.PAGOS];
        const monedaActual = normalizarMoneda(
            form.values[VENTA_FIELDS.MONEDA_CODIGO],
        );
        const tipoCambioActual = normalizarTipoCambio(
            form.values[VENTA_FIELDS.TIPO_CAMBIO],
        );

        form.setFieldValue(field, value);

        if (
            field === VENTA_FIELDS.CONDICION_VENTA_CODIGO &&
            !(CONDICIONES_VENTA_CREDITO as readonly string[]).includes(
                String(value),
            )
        ) {
            form.setFieldValue(VENTA_FIELDS.PLAZO_CREDITO_DIAS, null);
        }

        if (field === VENTA_FIELDS.MONEDA_CODIGO) {
            const monedaNueva = normalizarMoneda(value);

            if (monedaNueva !== monedaActual) {
                form.setFieldValue(
                    VENTA_FIELDS.LINEAS,
                    lineas.map((linea) => ({
                        ...linea,
                        PrecioUnitario: toMonedaDocumento(
                            linea.PrecioUnitarioBaseCrc ??
                                toBaseCrc(
                                    linea.PrecioUnitario,
                                    monedaActual,
                                    tipoCambioActual,
                                ),
                            monedaNueva,
                            tipoCambioActual,
                        ),
                        MontoDescuento: toMonedaDocumento(
                            linea.MontoDescuentoBaseCrc ??
                                toBaseCrc(
                                    linea.MontoDescuento,
                                    monedaActual,
                                    tipoCambioActual,
                                ),
                            monedaNueva,
                            tipoCambioActual,
                        ),
                    })),
                );
                form.setFieldValue(
                    VENTA_FIELDS.PAGOS,
                    recalcularPagosDocumento(
                        pagos.map((pago) => ({
                            ...pago,
                            MonedaCodigo: normalizarMoneda(pago.MonedaCodigo),
                        })),
                        monedaNueva,
                        tipoCambioActual,
                        calcularTotalesFactura(lineas, []).total,
                    ),
                );
            }

            form.setFieldValue(VENTA_FIELDS.MONEDA_CODIGO, monedaNueva);
        }

        if (field === VENTA_FIELDS.TIPO_CAMBIO) {
            const tipoCambioNuevo = normalizarTipoCambio(value);

            if (tipoCambioNuevo !== tipoCambioActual && monedaActual === "USD") {
                form.setFieldValue(
                    VENTA_FIELDS.LINEAS,
                    lineas.map((linea) => ({
                        ...linea,
                        PrecioUnitario: toMonedaDocumento(
                            linea.PrecioUnitarioBaseCrc ??
                                toBaseCrc(
                                    linea.PrecioUnitario,
                                    "USD",
                                    tipoCambioActual,
                                ),
                            "USD",
                            tipoCambioNuevo,
                        ),
                        MontoDescuento: toMonedaDocumento(
                            linea.MontoDescuentoBaseCrc ??
                                toBaseCrc(
                                    linea.MontoDescuento,
                                    "USD",
                                    tipoCambioActual,
                                ),
                            "USD",
                            tipoCambioNuevo,
                        ),
                    })),
                );
            }

            if (tipoCambioNuevo !== tipoCambioActual) {
                form.setFieldValue(
                    VENTA_FIELDS.PAGOS,
                    recalcularPagosDocumento(
                        pagos,
                        monedaActual,
                        tipoCambioNuevo,
                        calcularTotalesFactura(lineas, []).total,
                    ),
                );
            }
        }
    }

    return { handleHeaderChange };
}
