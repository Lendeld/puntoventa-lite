"use client";

import { TIPO_CAMBIO_DEFAULT, VENTA_FIELDS } from "@lib/constants/ventas.constants";
import type { CrearBorradorFacturaFormValues, DocumentoVentaDto } from "@lib/types/ventas.types";
import type { VendedorActivoDto } from "@lib/types/vendedores.types";
import { buildValuesFromDocumento, normalizarTipoCambio } from "@lib/utils/facturacion.utils";
import type { UseFormReturnType } from "@mantine/form";
import { useEffect, useRef, type RefObject } from "react";

interface NegocioDefaults {
    tipoCambioPredeterminado: number;
}

interface UseCargarProformaInicialParams {
    form: UseFormReturnType<CrearBorradorFacturaFormValues>;
    proformaInicial?: DocumentoVentaDto;
    loadedProformaIdRef: RefObject<string | null>;
    onLoaded: (proforma: DocumentoVentaDto) => void;
}

export function useCargarProformaInicial({
    form,
    proformaInicial,
    loadedProformaIdRef,
    onLoaded,
}: UseCargarProformaInicialParams) {
    useEffect(() => {
        if (!proformaInicial || loadedProformaIdRef.current === proformaInicial.id) return;
        if (proformaInicial.tipoDocumento !== "Proforma") return;

        form.setValues(buildValuesFromDocumento(proformaInicial));
        form.clearErrors();
        loadedProformaIdRef.current = proformaInicial.id;
        onLoaded(proformaInicial);
    // form.* de Mantine y onLoaded no son estables. El ref evita recargas.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [proformaInicial, loadedProformaIdRef]);
}

interface UseFacturacionDefaultsParams {
    form: UseFormReturnType<CrearBorradorFacturaFormValues>;
    negocio?: NegocioDefaults;
    documentoId: string | null;
    proformaIdParam: string | null;
    aplicaVendedores: boolean;
    vendedores: VendedorActivoDto[];
}

export function useFacturacionDefaults({
    form,
    negocio,
    documentoId,
    proformaIdParam,
    aplicaVendedores,
    vendedores,
}: UseFacturacionDefaultsParams) {
    const tipoCambioPrefilledRef = useRef(false);

    useEffect(() => {
        if (!negocio || documentoId || proformaIdParam || tipoCambioPrefilledRef.current) return;

        if (normalizarTipoCambio(form.values[VENTA_FIELDS.TIPO_CAMBIO]) === TIPO_CAMBIO_DEFAULT) {
            form.setFieldValue(VENTA_FIELDS.TIPO_CAMBIO, negocio.tipoCambioPredeterminado);
        }
        tipoCambioPrefilledRef.current = true;
    // form.* de Mantine no es estable.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [negocio, documentoId, proformaIdParam]);

    const vendedorIdActual = form.values[VENTA_FIELDS.VENDEDOR_ID];
    useEffect(() => {
        if (!aplicaVendedores) {
            if (vendedorIdActual) form.setFieldValue(VENTA_FIELDS.VENDEDOR_ID, "");
            return;
        }

        if (vendedorIdActual) return;
        const principal = vendedores.find((vendedor) => vendedor.isPrincipal);
        if (principal) form.setFieldValue(VENTA_FIELDS.VENDEDOR_ID, principal.id);
    // form.* de Mantine no es estable.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [aplicaVendedores, vendedores, vendedorIdActual]);
}
