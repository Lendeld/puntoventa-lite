"use client";

import { useCallback, useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import { ROUTES } from "@lib/constants/routes.constants";
import { BROADCAST_CHANNEL_NAME } from "@lib/constants/auth.constants";
import { tokenValidatorBrowser } from "./tokenValidatorBrowser";

export { BROADCAST_CHANNEL_NAME };

const INTERVALO_MS = 5 * 60 * 1000;

// Backoff para errores transitorios: 3 reintentos a ~2s, ~8s, ~20s
const RETRY_DELAYS_MS = [2000, 8000, 20000];

type ValidationResult = "valid" | "unauthorized" | "transient-error";

function sleep(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
}

async function validarUnaVez(): Promise<ValidationResult> {
    try {
        const response = await fetch(ROUTES.API_VALIDATE_SESSION, {
            method: "GET",
            cache: "no-store",
            credentials: "same-origin",
        });

        if (response.ok) return "valid";
        if (response.status === 401) return "unauthorized";

        return "transient-error";
    } catch {
        return "transient-error";
    }
}

export function TokenValidator() {
    const { replace } = useRouter();
    const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
    const validationRef = useRef<Promise<boolean> | null>(null);
    const channelRef = useRef<BroadcastChannel | null>(null);
    const canceladoRef = useRef(false);

    const forceLogout = useCallback(() => {
        if (typeof window !== "undefined") {
            tokenValidatorBrowser.redirectToLogout();
        } else {
            replace("/login");
        }
    }, [replace]);

    const validar = useCallback(async () => {
        if (validationRef.current) return validationRef.current;

        const validationPromise = (async () => {
            const firstResult = await validarUnaVez();

            if (firstResult === "valid") return true;

            // 401 real → logout inmediato, no reintentar
            if (firstResult === "unauthorized") return false;

            // transitorio → backoff con hasta RETRY_DELAYS_MS.length reintentos
            for (const delay of RETRY_DELAYS_MS) {
                await sleep(delay);
                // Si el componente se desmontó durante el sleep, cancelar
                if (canceladoRef.current) return true;
                const result = await validarUnaVez();
                if (result === "valid") return true;
                if (result === "unauthorized") return false;
                // transient-error → siguiente intento
            }

            // Todos los reintentos agotados y siguen transitorios → no desloguear
            return true;
        })();

        validationRef.current = validationPromise;

        try {
            const isValid = await validationPromise;
            // Verificar cancelación antes de actuar sobre el resultado
            if (!canceladoRef.current && !isValid) {
                forceLogout();
                return false;
            }
            return true;
        } finally {
            validationRef.current = null;
        }
    }, [forceLogout]);

    useEffect(() => {
        canceladoRef.current = false;

        void validar();
        intervalRef.current = setInterval(validar, INTERVALO_MS);

        channelRef.current = new BroadcastChannel(BROADCAST_CHANNEL_NAME);
        channelRef.current.onmessage = (event) => {
            if (event.data?.type === "logout") forceLogout();
            if (event.data?.type === "check") void validar();
        };

        window.onpageshow = (e) => {
            if (e.persisted) tokenValidatorBrowser.reloadPage();
        };

        function handleVisibilityChange() {
            if (document.visibilityState === "visible") void validar();
        }

        function handleFocus() {
            void validar();
        }

        window.addEventListener("visibilitychange", handleVisibilityChange);
        window.addEventListener("focus", handleFocus);

        return () => {
            canceladoRef.current = true;
            if (intervalRef.current) clearInterval(intervalRef.current);
            channelRef.current?.close();
            window.onpageshow = null;
            window.removeEventListener("visibilitychange", handleVisibilityChange);
            window.removeEventListener("focus", handleFocus);
        };
    }, [validar, forceLogout]);

    return null;
}
