"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import {
    abonarApartadoAction,
    cancelarApartadoAction,
    convertirApartadoAFacturaAction,
    extenderVencimientoApartadoAction,
} from "@lib/actions/ventas.actions";
import type { DocumentoVentaDto } from "@lib/types/ventas.types";
import { resolveApiErrorMessage } from "@lib/utils/apiErrors";
import { Text } from "@mantine/core";
import { modals } from "@mantine/modals";
import dayjs from "dayjs";
import type { AppRouterInstance } from "next/dist/shared/lib/app-router-context.shared-runtime";
import { useState } from "react";

interface AbonoErrors {
    fechaPago?: string;
    medioPagoCodigo?: string;
    montoAbono?: string;
}

export function useApartadoAcciones(
    documento: DocumentoVentaDto,
    router: AppRouterInstance,
) {
    const [loadingAction, setLoadingAction] = useState(false);

    const [abonoOpen, setAbonoOpen] = useState(false);
    const [montoAbono, setMontoAbono] = useState<number | "">(
        documento.saldoPendiente,
    );
    const [medioPagoCodigo, setMedioPagoCodigo] = useState("");
    const [referenciaPago, setReferenciaPago] = useState("");
    const [observacionPago, setObservacionPago] = useState("");
    const [fechaPago, setFechaPago] = useState<string | null>(() =>
        dayjs().toISOString(),
    );
    const [abonoErrors, setAbonoErrors] = useState<AbonoErrors>({});
    const [abonoError, setAbonoError] = useState<string | null>(null);

    const [extensionOpen, setExtensionOpen] = useState(false);
    const [fechaVencimiento, setFechaVencimiento] = useState<string | null>(
        documento.fechaVencimiento
            ? dayjs(documento.fechaVencimiento).toISOString()
            : null,
    );

    // Resetea el formulario al abrir, sin useEffect que encadene setState.
    function abrirAbono() {
        setMontoAbono(documento.saldoPendiente);
        setMedioPagoCodigo("");
        setReferenciaPago("");
        setObservacionPago("");
        setFechaPago(dayjs().toISOString());
        setAbonoErrors({});
        setAbonoError(null);
        setAbonoOpen(true);
    }

    function closeAbonoModal() {
        if (loadingAction) return;
        setAbonoOpen(false);
        setAbonoErrors({});
        setAbonoError(null);
    }

    function limpiarAbonoError(campo: keyof AbonoErrors) {
        setAbonoErrors((current) => ({ ...current, [campo]: undefined }));
    }

    function validateAbono() {
        const nextErrors: AbonoErrors = {};
        setAbonoError(null);

        if (!fechaPago) {
            nextErrors.fechaPago = "La fecha de pago es requerida.";
        }
        if (!medioPagoCodigo) {
            nextErrors.medioPagoCodigo = "El método de pago es requerido.";
        }
        if (!montoAbono || montoAbono <= 0) {
            nextErrors.montoAbono = "El monto debe ser mayor a cero.";
        } else if (montoAbono > documento.saldoPendiente) {
            nextErrors.montoAbono = "El abono no puede superar el saldo pendiente.";
        }

        setAbonoErrors(nextErrors);
        return Object.keys(nextErrors).length === 0;
    }

    async function handleAbonar() {
        if (!validateAbono() || !montoAbono || !fechaPago) return;
        setLoadingAction(true);
        const result = await abonarApartadoAction(
            documento.id,
            {
                MonedaCodigo: documento.monedaCodigo,
                TipoCambioAplicado: 1,
                MedioPagoCodigo: medioPagoCodigo,
                MontoEntregado: montoAbono,
                MontoAplicadoMonedaPago: montoAbono,
                MontoAplicadoDocumento: montoAbono,
                MontoVueltoMonedaPago: 0,
                MontoVueltoDocumento: 0,
                Referencia: referenciaPago,
                Observacion: observacionPago,
            },
            fechaPago,
        );
        setLoadingAction(false);

        if (result.errors) {
            setAbonoError(
                resolveApiErrorMessage(result.errors, {
                    fallback: "No fue posible registrar el abono.",
                }),
            );
            return;
        }

        AppNotifier.success({ message: "Abono registrado correctamente." });
        setAbonoOpen(false);
        setAbonoError(null);
        setReferenciaPago("");
        setObservacionPago("");
        router.refresh();
    }

    async function handleExtender() {
        if (!fechaVencimiento) return;
        setLoadingAction(true);
        const result = await extenderVencimientoApartadoAction(
            documento.id,
            fechaVencimiento,
        );
        setLoadingAction(false);

        if (result.errors) {
            AppNotifier.error({
                message:
                    Object.values(result.errors)[0] ??
                    "No fue posible extender el vencimiento.",
            });
            return;
        }

        AppNotifier.success({ message: "Vencimiento actualizado." });
        setExtensionOpen(false);
        router.refresh();
    }

    function handleCancelarApartado() {
        modals.openConfirmModal({
            title: "Cancelar apartado",
            centered: true,
            children: (
                <Text size="sm">
                    ¿Cancelar este apartado? Se liberarán las reservas de
                    inventario. Esta acción no se puede deshacer.
                </Text>
            ),
            labels: { confirm: "Cancelar apartado", cancel: "Volver" },
            confirmProps: { color: "red" },
            onConfirm: async () => {
                setLoadingAction(true);
                const result = await cancelarApartadoAction(documento.id);
                setLoadingAction(false);

                if (result.errors) {
                    AppNotifier.error({
                        message:
                            Object.values(result.errors)[0] ??
                            "No fue posible cancelar el apartado.",
                    });
                    return;
                }

                AppNotifier.success({ message: "Apartado cancelado." });
                router.refresh();
            },
        });
    }

    async function handleConvertirApartado() {
        setLoadingAction(true);
        const result = await convertirApartadoAFacturaAction(documento.id);
        setLoadingAction(false);

        if (result.errors) {
            AppNotifier.error({
                message:
                    Object.values(result.errors)[0] ??
                    "No fue posible convertir el apartado.",
            });
            return;
        }

        AppNotifier.success({ message: "Apartado convertido a factura." });
        if (result.data?.id) {
            router.push(`/emision/ventas/${result.data.id}`);
            return;
        }
        router.refresh();
    }

    return {
        loadingAction,
        // abono
        abonoOpen,
        abrirAbono,
        closeAbonoModal,
        montoAbono,
        setMontoAbono,
        medioPagoCodigo,
        setMedioPagoCodigo,
        referenciaPago,
        setReferenciaPago,
        observacionPago,
        setObservacionPago,
        fechaPago,
        setFechaPago,
        abonoErrors,
        limpiarAbonoError,
        abonoError,
        handleAbonar,
        // extension
        extensionOpen,
        setExtensionOpen,
        fechaVencimiento,
        setFechaVencimiento,
        handleExtender,
        // otros
        handleCancelarApartado,
        handleConvertirApartado,
    };
}
