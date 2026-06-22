"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { registrarAbonoFacturaAction } from "@lib/actions/ventas.actions";
import { MEDIO_PAGO_EFECTIVO } from "@lib/constants/ventas.constants";
import type { DocumentoVentaDto } from "@lib/types/ventas.types";
import { resolveApiErrorMessage } from "@lib/utils/apiErrors";
import dayjs from "dayjs";
import type { AppRouterInstance } from "next/dist/shared/lib/app-router-context.shared-runtime";
import { useState } from "react";

interface AbonoCreditoErrors {
    fechaPago?: string;
    medioPagoCodigo?: string;
    montoAbono?: string;
}

export function useAbonoCredito(
    documento: DocumentoVentaDto,
    router: AppRouterInstance,
) {
    const [loadingAbono, setLoadingAbono] = useState(false);
    const [abonoCreditoOpen, setAbonoCreditoOpen] = useState(false);
    const [montoAbono, setMontoAbono] = useState<number | "">(documento.saldoPendiente);
    const [medioPagoCodigo, setMedioPagoCodigo] = useState<string>(MEDIO_PAGO_EFECTIVO);
    const [referenciaPago, setReferenciaPago] = useState("");
    const [observacionPago, setObservacionPago] = useState("");
    const [fechaPago, setFechaPago] = useState<string | null>(() => dayjs().toISOString());
    const [abonoCreditoErrors, setAbonoCreditoErrors] = useState<AbonoCreditoErrors>({});
    const [abonoCreditoError, setAbonoCreditoError] = useState<string | null>(null);
    const [abonoResultadoPagoId, setAbonoResultadoPagoId] = useState<string | null>(null);

    function abrirAbonoCredito() {
        setMontoAbono(documento.saldoPendiente);
        setMedioPagoCodigo(MEDIO_PAGO_EFECTIVO);
        setReferenciaPago("");
        setObservacionPago("");
        setFechaPago(dayjs().toISOString());
        setAbonoCreditoErrors({});
        setAbonoCreditoError(null);
        setAbonoResultadoPagoId(null);
        setAbonoCreditoOpen(true);
    }

    function cerrarAbonoCredito() {
        if (loadingAbono) return;
        setAbonoCreditoOpen(false);
        setAbonoCreditoErrors({});
        setAbonoCreditoError(null);
    }

    function cerrarAbonoResultado() {
        setAbonoResultadoPagoId(null);
        router.refresh();
    }

    function limpiarAbonoCreditoError(campo: keyof AbonoCreditoErrors) {
        setAbonoCreditoErrors((current) => ({ ...current, [campo]: undefined }));
    }

    function validateAbonoCredito() {
        const nextErrors: AbonoCreditoErrors = {};
        setAbonoCreditoError(null);

        if (!fechaPago) {
            nextErrors.fechaPago = "La fecha informativa es requerida.";
        }
        if (!medioPagoCodigo) {
            nextErrors.medioPagoCodigo = "El método de pago es requerido.";
        }
        if (!montoAbono || montoAbono <= 0) {
            nextErrors.montoAbono = "El monto debe ser mayor a cero.";
        } else if (montoAbono > documento.saldoPendiente) {
            nextErrors.montoAbono = "El abono no puede superar el saldo pendiente.";
        }

        setAbonoCreditoErrors(nextErrors);
        return Object.keys(nextErrors).length === 0;
    }

    async function handleAbonarCredito() {
        if (!validateAbonoCredito() || !montoAbono || !fechaPago) return;

        setLoadingAbono(true);
        const result = await registrarAbonoFacturaAction(documento.id, {
            monedaCodigo: documento.monedaCodigo,
            medioPagoCodigo,
            monto: montoAbono,
            referencia: referenciaPago || null,
            observacion: observacionPago || null,
            fechaPago: dayjs(fechaPago).toISOString(),
        });
        setLoadingAbono(false);

        if (result.errors) {
            setAbonoCreditoError(
                resolveApiErrorMessage(result.errors, {
                    fallback: "No fue posible registrar el abono.",
                }),
            );
            return;
        }

        AppNotifier.success({ message: "Abono registrado correctamente." });
        setAbonoCreditoOpen(false);
        setAbonoCreditoError(null);
        setReferenciaPago("");
        setObservacionPago("");
        setAbonoResultadoPagoId(result.data?.pagoId ?? null);
    }

    return {
        loadingAbono,
        abonoCreditoOpen,
        abrirAbonoCredito,
        cerrarAbonoCredito,
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
        abonoCreditoErrors,
        limpiarAbonoCreditoError,
        abonoCreditoError,
        handleAbonarCredito,
        abonoResultadoPagoId,
        cerrarAbonoResultado,
    };
}
