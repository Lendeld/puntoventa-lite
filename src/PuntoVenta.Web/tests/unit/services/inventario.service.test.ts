import { beforeEach, describe, expect, it, vi } from "vitest";
import {
    obtenerMovimientosStockService,
    ajustarStockService,
} from "@lib/services/inventario.service";
import { requestAPI } from "@lib/utils/requestApi";

vi.mock("@lib/utils/requestApi", () => ({
    requestAPI: vi.fn(),
}));

const requestAPIMock = vi.mocked(requestAPI);

describe("inventario.service", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        requestAPIMock.mockResolvedValue({ data: null, errors: undefined });
    });

    describe("obtenerMovimientosStockService", () => {
        it("manda GET con pagina y tamano por defecto", async () => {
            await obtenerMovimientosStockService({ pagina: 1, tamano: 20 });

            expect(requestAPIMock).toHaveBeenCalledWith({
                url: "/inventario/movimientos-stock",
                method: "GET",
                query: {
                    productoId: undefined,
                    pagina: 1,
                    tamano: 20,
                },
            });
        });

        it("incluye productoId cuando se filtra por producto", async () => {
            const productoId = "3c4d5e6f-7a8b-4c9d-ae1f-2a3b4c5d6e7f";

            await obtenerMovimientosStockService({
                productoId,
                pagina: 2,
                tamano: 10,
            });

            expect(requestAPIMock).toHaveBeenCalledWith({
                url: "/inventario/movimientos-stock",
                method: "GET",
                query: {
                    productoId,
                    pagina: 2,
                    tamano: 10,
                },
            });
        });
    });

    describe("ajustarStockService", () => {
        it("manda POST al endpoint correcto con body completo", async () => {
            const productoId = "3c4d5e6f-7a8b-4c9d-ae1f-2a3b4c5d6e7f";

            await ajustarStockService({
                productoId,
                delta: 15,
                razon: "Ajuste inventario físico",
            });

            expect(requestAPIMock).toHaveBeenCalledWith({
                url: "/inventario/ajuste-stock",
                method: "POST",
                body: {
                    productoId,
                    delta: 15,
                    razon: "Ajuste inventario físico",
                },
            });
        });

        it("manda POST sin razon cuando no se especifica", async () => {
            const productoId = "3c4d5e6f-7a8b-4c9d-ae1f-2a3b4c5d6e7f";

            await ajustarStockService({
                productoId,
                delta: -3,
            });

            expect(requestAPIMock).toHaveBeenCalledWith({
                url: "/inventario/ajuste-stock",
                method: "POST",
                body: {
                    productoId,
                    delta: -3,
                    razon: undefined,
                },
            });
        });
    });
});
