import { describe, expect, it } from "vitest";
import { humanizarEventoVenta } from "@/app/(root)/emision/ventas/[id]/HistoriaEventosVenta";

describe("humanizarEventoVenta", () => {
    it("convierte codigos tecnicos conocidos a etiquetas de cliente", () => {
        expect(humanizarEventoVenta("AbonoRevertido")).toBe("Abono Revertido");
        expect(humanizarEventoVenta("NotaCreditoAplicada")).toBe("Nota de Crédito Aplicada");
    });

    it("separa CamelCase para eventos futuros", () => {
        expect(humanizarEventoVenta("EventoNuevoCliente")).toBe("Evento Nuevo Cliente");
    });
});
