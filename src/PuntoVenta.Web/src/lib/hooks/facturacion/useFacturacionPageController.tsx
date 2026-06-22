"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import {
    actualizarProformaAction,
    crearApartadoAction,
    crearFacturaAction,
    crearProformaAction,
    facturarProformaAction,
} from "@lib/actions/ventas.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { VENTA_FIELDS } from "@lib/constants/ventas.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { useMediosPagoActivosQuery } from "@lib/hooks/useMediosPagoActivosQuery";
import { useTarifasIvaActivasQuery } from "@lib/hooks/useTarifasIvaActivasQuery";
import { useCajasQuery } from "@lib/hooks/useCajasQuery";
import { useVendedoresActivosQuery } from "@lib/hooks/useVendedorQuery";
import { useFacturacionCobro } from "@lib/hooks/facturacion/useFacturacionCobro";
import {
    useCargarProformaInicial,
    useFacturacionDefaults,
} from "@lib/hooks/facturacion/useFacturacionDefaults";
import { useFacturacionHeaderLogic } from "@lib/hooks/facturacion/useFacturacionHeaderLogic";
import { useFacturacionLineas } from "@lib/hooks/facturacion/useFacturacionLineas";
import { useFacturacionPageState } from "@lib/hooks/facturacion/useFacturacionPageState";
import { imprimirTicketAuto } from "@lib/printing/imprimir-ticket";
import { crearBorradorFacturaSchema } from "@lib/schemas/ventas.schema";
import { obtenerNegocioService } from "@lib/services/configuracion.service";
import { obtenerDocumentoVentaPorIdService } from "@lib/services/ventas.service";
import type {
    CrearBorradorFacturaFormValues,
    EmitirFacturaResult,
    GuardarBorradorFacturaResult,
    VentaActionResult,
} from "@lib/types/ventas.types";
import {
    buildInitialValues,
    buildValuesFromDocumento,
    defaultApartadoVencimiento,
    normalizarMoneda,
    normalizarTipoCambio,
} from "@lib/utils/facturacion.utils";
import { redondear } from "@lib/utils/number.utils";
import {
    calcularTotalesFactura,
    convertirMontoMoneda,
    requierePagoCompleto,
} from "@lib/utils/ventas.utils";
import { zodResolver } from "@lib/utils/zodResolver";
import { Text } from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useRouter } from "next/navigation";
import { useQueryState } from "nuqs";
import { useMemo, useRef } from "react";

export function useFacturacionPageController() {
    const facturacionViewportRef = useRef<HTMLDivElement>(null);
    const loadedProformaIdRef = useRef<string | null>(null);
    const loadingEmitirRef = useRef(false);
    const router = useRouter();
    const queryClient = useQueryClient();
    const [proformaIdParam, setProformaIdParam] = useQueryState("proformaId");
    const [state, dispatch] = useFacturacionPageState();
    const form = useForm<CrearBorradorFacturaFormValues>({
        initialValues: buildInitialValues(),
        validate: zodResolver(crearBorradorFacturaSchema),
    });

    const { data: tarifasIva = [] } = useTarifasIvaActivasQuery();
    const { data: mediosPago = [] } = useMediosPagoActivosQuery();
    const { data: vendedores = [] } = useVendedoresActivosQuery();
    const { data: cajas = [] } = useCajasQuery();
    const { data: negocio } = useQuery({
        queryKey: QUERY_KEYS.configuracion.negocio,
        queryFn: async () => {
            const res = await obtenerNegocioService();
            if (res.errors) throw res.errors;
            if (!res.data) throw new Error("No se encontró la configuración del negocio.");
            return res.data;
        },
    });
    const { data: proformaInicial } = useQuery({
        queryKey: ["ventas", "proforma-edicion", proformaIdParam],
        queryFn: async () => {
            if (!proformaIdParam) throw new Error("La proforma es requerida.");
            const res = await obtenerDocumentoVentaPorIdService(proformaIdParam);
            if (res.errors) throw res.errors;
            if (!res.data) throw new Error("No se encontró la proforma.");
            return res.data;
        },
        enabled: !!proformaIdParam,
    });

    const pagos = form.values[VENTA_FIELDS.PAGOS];
    const monedaCodigo = normalizarMoneda(form.values[VENTA_FIELDS.MONEDA_CODIGO]);
    const tipoCambio = normalizarTipoCambio(form.values[VENTA_FIELDS.TIPO_CAMBIO]);
    const lineasEditor = useFacturacionLineas({ form, monedaCodigo, tipoCambio, tarifasIva });
    const totales = useMemo(
        () => calcularTotalesFactura(lineasEditor.lineas, pagos),
        [lineasEditor.lineas, pagos],
    );
    const esVentaContado = requierePagoCompleto(form.values[VENTA_FIELDS.CONDICION_VENTA_CODIGO]);
    const esFacturaMontoCero = esVentaContado && totales.total === 0;
    const aplicaVendedores = negocio?.aplicaVendedores ?? false;
    const aplicaCajas = negocio?.aplicaCajas ?? false;
    const esProforma = state.documento?.tipoDocumento === "Proforma" || !!proformaIdParam;
    const esApartado = state.documento?.tipoDocumento === "Apartado";
    const monedaEquivalente = monedaCodigo === "USD" ? "CRC" : "USD";
    const equivalenteTotal = useMemo(
        () =>
            tipoCambio > 0
                ? convertirMontoMoneda(totales.total, monedaCodigo, monedaEquivalente, tipoCambio)
                : null,
        [totales.total, monedaCodigo, monedaEquivalente, tipoCambio],
    );
    const { handleHeaderChange } = useFacturacionHeaderLogic(form);

    useCargarProformaInicial({
        form,
        proformaInicial,
        loadedProformaIdRef,
        onLoaded: (proforma) =>
            dispatch({ type: "patch", value: { documento: proforma, documentoId: proforma.id } }),
    });
    useFacturacionDefaults({
        form,
        negocio,
        documentoId: state.documentoId,
        proformaIdParam,
        aplicaVendedores,
        vendedores,
    });

    const cobro = useFacturacionCobro({
        form,
        mediosPago,
        monedaCodigo,
        tipoCambio,
        totalTotal: totales.total,
        isEmitting: () => loadingEmitirRef.current,
        onValidationError: notifyValidationErrors,
    });

    const guardar = useActionHandler<
        CrearBorradorFacturaFormValues,
        VentaActionResult<GuardarBorradorFacturaResult>
    >({
        form,
        onSuccess: (result) => {
            const data = result.data;
            if (!data) return;

            dispatch({ type: "patch", value: { documentoId: data.id, documento: data.detalle } });
            if (data.detalle?.tipoDocumento === "Proforma") {
                form.setValues(buildValuesFromDocumento(data.detalle));
                form.clearErrors();
                loadedProformaIdRef.current = data.detalle.id;
            }
            AppNotifier.success({
                message: data.detalle?.tipoDocumento === "Apartado"
                    ? "Apartado creado correctamente."
                    : "Proforma guardada correctamente.",
            });
        },
    });
    const emitir = useActionHandler<Record<string, never>, VentaActionResult<EmitirFacturaResult>>({
        onSuccess: (result) => {
            const data = result.data;
            if (!data) return;

            const vuelto = redondear(
                form.values[VENTA_FIELDS.PAGOS].reduce(
                    (acc, pago) => acc + pago.MontoVueltoDocumento,
                    0,
                ),
            );
            AppNotifier.success({ message: "Factura emitida exitosamente." });
            if (!esProforma && data.detalle?.id) imprimirTicket(data.detalle.id);
            resetFactura(
                data.detalle
                    ? { resultado: { documento: data.detalle, vuelto } }
                    : undefined,
            );
        },
    });
    loadingEmitirRef.current = emitir.loading;

    function imprimirTicket(documentoId: string) {
        void imprimirTicketAuto(documentoId).then((result) => {
            if (result.status === "error") {
                AppNotifier.error({ title: "No se pudo imprimir el ticket", message: result.message });
            }
        });
    }

    function scrollFacturacionTop() {
        requestAnimationFrame(() => {
            facturacionViewportRef.current?.scrollTo({ top: 0, behavior: "smooth" });
            window.scrollTo({ top: 0, behavior: "smooth" });
        });
    }

    function resetFactura(options?: {
        resultado?: { documento: EmitirFacturaResult["detalle"]; vuelto: number };
    }) {
        const proformaIdActiva = proformaIdParam ?? loadedProformaIdRef.current;
        form.setValues(buildInitialValues());
        if (negocio) form.setFieldValue(VENTA_FIELDS.TIPO_CAMBIO, negocio.tipoCambioPredeterminado);
        form.clearErrors();
        lineasEditor.setSelectedProductoId(null);
        lineasEditor.setSelectedProducto(null);
        cobro.setCobroModalOpen(false);
        cobro.setCobroModo("rapido");
        guardar.setError(null);
        emitir.setError(null);
        dispatch({ type: "reset" });
        if (options?.resultado?.documento) {
            dispatch({
                type: "patch",
                value: {
                    ultimoDocumentoEmitido: options.resultado.documento,
                    ultimoVueltoEmitido: options.resultado.vuelto,
                },
            });
        }
        scrollFacturacionTop();

        loadedProformaIdRef.current = proformaIdActiva;
        if (proformaIdActiva) {
            queryClient.invalidateQueries({ queryKey: ["ventas", "proforma-edicion", proformaIdActiva] });
        }
        if (proformaIdParam) setProformaIdParam(null, { scroll: false });
    }

    function notifyValidationErrors() {
        AppNotifier.warning({
            title: "Datos por revisar",
            message: "Hay información faltante o inválida. Revise los campos marcados antes de continuar.",
        });
    }

    function validarForm() {
        if (!form.validate().hasErrors) return true;
        notifyValidationErrors();
        return false;
    }

    async function handleGuardar() {
        guardar.setError(null);
        if (!validarForm()) return;
        await guardar.execute(() =>
            state.documentoId && esProforma
                ? actualizarProformaAction(state.documentoId, form.values)
                : crearProformaAction(form.values),
        );
    }

    async function handleCrearApartado() {
        guardar.setError(null);
        if (!validarForm()) return;
        dispatch({
            type: "patch",
            value: {
                apartadoModalOpen: true,
                apartadoFechaVencimiento: defaultApartadoVencimiento(
                    String(form.values[VENTA_FIELDS.FECHA_DOCUMENTO] ?? ""),
                ),
                apartadoFechaError: null,
            },
        });
    }

    async function handleConfirmarCrearApartado() {
        const fechaDocumento = String(form.values[VENTA_FIELDS.FECHA_DOCUMENTO] ?? "");
        const fechaVencimiento = state.apartadoFechaVencimiento;
        if (!fechaVencimiento || !dayjs(fechaVencimiento).isValid()) {
            dispatch({ type: "patch", value: { apartadoFechaError: "La fecha de vencimiento del apartado es requerida." } });
            return;
        }
        if (dayjs(fechaVencimiento).startOf("day").isBefore(dayjs(fechaDocumento).startOf("day"))) {
            dispatch({ type: "patch", value: { apartadoFechaError: "La fecha de vencimiento debe ser igual o posterior a la fecha del documento." } });
            return;
        }

        dispatch({ type: "patch", value: { apartadoFechaError: null } });
        const values = { ...form.values, [VENTA_FIELDS.FECHA_VENCIMIENTO]: fechaVencimiento };
        form.setFieldValue(VENTA_FIELDS.FECHA_VENCIMIENTO, fechaVencimiento);
        const result = await guardar.execute(() => crearApartadoAction(values));
        if (result && result.status < 400) {
            dispatch({ type: "patch", value: { apartadoModalOpen: false } });
            // El apartado se gestiona (abonos, conversión, impresión) en su detalle;
            // quedarse en la pantalla de facturación con él cargado no aporta nada.
            const apartadoId = result.data?.id;
            if (apartadoId) router.push(`/emision/ventas/${apartadoId}`);
        }
    }

    async function executeEmisionActual() {
        const documentoId = state.documentoId;
        if (documentoId && esProforma) {
            await emitir.execute(() => facturarProformaAction(documentoId, form.values));
        } else {
            await emitir.execute(() => crearFacturaAction(form.values));
        }
    }

    async function handleEmitir() {
        closeResultadoDrawer();
        if (esVentaContado && !esFacturaMontoCero) {
            cobro.abrirCobroRapido();
            return;
        }
        emitir.setError(null);
        if (!esVentaContado && !validarForm()) return;
        await executeEmisionActual();
    }

    async function handleConfirmarCobro() {
        closeResultadoDrawer();
        emitir.setError(null);
        if (!validarForm()) return;
        await executeEmisionActual();
        cobro.setCobroModalOpen(false);
    }

    function closeResultadoDrawer() {
        dispatch({ type: "patch", value: { ultimoDocumentoEmitido: null, ultimoVueltoEmitido: 0 } });
    }

    function closeApartadoModal() {
        if (!guardar.loading) {
            dispatch({ type: "patch", value: { apartadoModalOpen: false, apartadoFechaError: null } });
        }
    }

    function confirmarNuevaFactura() {
        modals.openConfirmModal({
            title: "Nueva factura",
            centered: true,
            children: <Text size="sm">Se limpiará la factura actual. ¿Continuar?</Text>,
            labels: { confirm: "Sí, nueva factura", cancel: "Cancelar" },
            confirmProps: { color: "accentPV" },
            cancelProps: { variant: "outline" },
            onConfirm: () => resetFactura(),
        });
    }

    function confirmarGuardarProforma() {
        modals.openConfirmModal({
            title: state.documentoId && esProforma ? "Actualizar proforma" : "Guardar proforma",
            centered: true,
            children: <Text size="sm">¿Confirmás guardar la proforma con los datos actuales?</Text>,
            labels: { confirm: "Sí, guardar", cancel: "Cancelar" },
            confirmProps: { color: "accentPV" },
            cancelProps: { variant: "outline" },
            onConfirm: () => void handleGuardar(),
        });
    }

    function confirmarCrearApartado() {
        modals.openConfirmModal({
            title: "Crear apartado",
            centered: true,
            children: <Text size="sm">¿Confirmás crear un apartado con los datos actuales?</Text>,
            labels: { confirm: "Sí, crear apartado", cancel: "Cancelar" },
            confirmProps: { color: "orange" },
            cancelProps: { variant: "outline" },
            onConfirm: () => void handleCrearApartado(),
        });
    }

    function getFieldError(path: string) {
        const error = form.errors[path];
        return typeof error === "string" ? error : null;
    }

    return {
        form,
        state,
        dispatch,
        vendedores,
        aplicaVendedores,
        cajas,
        aplicaCajas,
        pagos,
        monedaCodigo,
        monedaEquivalente,
        equivalenteTotal,
        totales,
        lineasEditor,
        cobro,
        facturacionViewportRef,
        loadingGuardar: guardar.loading,
        loadingEmitir: emitir.loading,
        error: guardar.error || emitir.error,
        canEmit: lineasEditor.lineas.length > 0,
        showOpcionesCobro: esVentaContado && !esFacturaMontoCero,
        emitirLabel: esProforma ? "Facturar" : "Cobrar",
        esApartado,
        getFieldError,
        handleHeaderChange,
        handleConfirmarCrearApartado,
        handleConfirmarCobro,
        closeApartadoModal,
        closeResultadoDrawer,
        confirmarNuevaFactura,
        confirmarGuardarProforma,
        confirmarCrearApartado,
        handleEmitir,
    };
}
