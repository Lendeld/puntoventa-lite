import { useReducer } from "react";

type PatchAction<T> = Partial<T> | ((state: T) => Partial<T>);

function patchReducer<T extends object>(state: T, action: PatchAction<T>): T {
    const patch = typeof action === "function" ? action(state) : action;
    return { ...state, ...patch };
}

export function usePatchReducer<T extends object>(initialState: T | (() => T)) {
    return useReducer(
        patchReducer<T>,
        undefined,
        () => (typeof initialState === "function" ? initialState() : initialState),
    );
}
