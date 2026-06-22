"use client";

import { useEffect, useSyncExternalStore } from "react";

const PREFIX = "returnUrl:";

// Persiste URL actual (path + search) en sessionStorage bajo `key`. Usar en
// listing pages con filtros — el detalle/sub-page despues lee con useReturnUrl
// y arma su boton "Volver" con los filtros intactos.
export function useReturnUrlPersist(key: string): void {
    useEffect(() => {
        sessionStorage.setItem(PREFIX + key, window.location.pathname + window.location.search);
    });
}

// Lee URL guardada por useReturnUrlPersist. Fallback cuando no hay nada
// (entró directo por deep-link sin pasar por la listing). useSyncExternalStore:
// en SSR usa el fallback (getServerSnapshot) y en cliente lee sessionStorage,
// sin hydration mismatch ni efecto de sincronización.
const subscribeNoop = () => () => {};

export function useReturnUrl(key: string, fallback: string): string {
    return useSyncExternalStore(
        subscribeNoop,
        () => sessionStorage.getItem(PREFIX + key) ?? fallback,
        () => fallback,
    );
}
