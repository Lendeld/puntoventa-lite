import { describe, it, expect } from "vitest";
import {
    colorPorEstadoStock,
    colorPorTipoItem,
    etiquetaStock,
    obtenerEstadoStock,
} from "@lib/utils/productos.utils";

describe("obtenerEstadoStock", () => {
    it("sin-control cuando no aplica existencias", () => {
        expect(
            obtenerEstadoStock({ noAplicaExistencias: true, existenciaTotal: 0 }),
        ).toBe("sin-control");
    });

    it("agotado cuando existencia es 0", () => {
        expect(
            obtenerEstadoStock({ noAplicaExistencias: false, existenciaTotal: 0 }),
        ).toBe("agotado");
    });

    it("bajo en el umbral exacto (5)", () => {
        expect(
            obtenerEstadoStock({ noAplicaExistencias: false, existenciaTotal: 5 }),
        ).toBe("bajo");
    });

    it("disponible justo arriba del umbral (6)", () => {
        expect(
            obtenerEstadoStock({ noAplicaExistencias: false, existenciaTotal: 6 }),
        ).toBe("disponible");
    });
});

describe("etiquetaStock", () => {
    it("null cuando no aplica existencias", () => {
        expect(
            etiquetaStock({ noAplicaExistencias: true, existenciaTotal: 99 }),
        ).toBeNull();
    });

    it("Agotado cuando es 0", () => {
        expect(
            etiquetaStock({ noAplicaExistencias: false, existenciaTotal: 0 }),
        ).toBe("Agotado");
    });

    it("Quedan N cuando es bajo", () => {
        expect(
            etiquetaStock({ noAplicaExistencias: false, existenciaTotal: 3 }),
        ).toBe("Quedan 3");
    });

    it("N en stock cuando hay disponibilidad", () => {
        expect(
            etiquetaStock({ noAplicaExistencias: false, existenciaTotal: 42 }),
        ).toBe("42 en stock");
    });
});

describe("colorPorEstadoStock", () => {
    it("mapea cada estado", () => {
        expect(colorPorEstadoStock("agotado")).toBe("red");
        expect(colorPorEstadoStock("bajo")).toBe("yellow");
        expect(colorPorEstadoStock("disponible")).toBe("teal");
        expect(colorPorEstadoStock("sin-control")).toBe("gray");
    });
});

describe("colorPorTipoItem", () => {
    it("Bien azul, Servicio grape", () => {
        expect(colorPorTipoItem("Bien")).toBe("blue");
        expect(colorPorTipoItem("Servicio")).toBe("grape");
    });
});
