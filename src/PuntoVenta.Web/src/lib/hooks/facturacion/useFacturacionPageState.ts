"use client";

import type { ClienteListaDto } from "@lib/types/clientes.types";
import type { DocumentoVentaDto } from "@lib/types/ventas.types";
import { useReducer } from "react";

export interface FacturacionPageState {
    documento: DocumentoVentaDto | null;
    documentoId: string | null;
    clienteSeleccionado: ClienteListaDto | null;
    ultimoDocumentoEmitido: DocumentoVentaDto | null;
    ultimoVueltoEmitido: number;
    apartadoModalOpen: boolean;
    apartadoFechaVencimiento: string;
    apartadoFechaError: string | null;
}

const initialState: FacturacionPageState = {
    documento: null,
    documentoId: null,
    clienteSeleccionado: null,
    ultimoDocumentoEmitido: null,
    ultimoVueltoEmitido: 0,
    apartadoModalOpen: false,
    apartadoFechaVencimiento: "",
    apartadoFechaError: null,
};

type FacturacionPageAction =
    | { type: "patch"; value: Partial<FacturacionPageState> }
    | { type: "reset" };

function reducer(state: FacturacionPageState, action: FacturacionPageAction) {
    if (action.type === "reset") return initialState;
    return { ...state, ...action.value };
}

export function useFacturacionPageState() {
    return useReducer(reducer, initialState);
}
