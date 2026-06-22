import { beforeEach, describe, expect, it, vi } from "vitest";
import { AJUSTE_STOCK_FIELDS } from "@lib/constants/inventario.constants";
import { ajustarStockAction } from "@lib/actions/inventario.actions";
import { ajustarStockService } from "@lib/services/inventario.service";

vi.mock("@lib/services/inventario.service", () => ({
    ajustarStockService: vi.fn(),
    obtenerMovimientosStockService: vi.fn(),
}));

const ajustarServiceMock = vi.mocked(ajustarStockService);

const VALID_VALUES = {
    [AJUSTE_STOCK_FIELDS.PRODUCTO_ID]: "3c4d5e6f-7a8b-4c9d-ae1f-2a3b4c5d6e7f",
    [AJUSTE_STOCK_FIELDS.DELTA]: 10,
    [AJUSTE_STOCK_FIELDS.RAZON]: "Ingreso por compra",
};

describe("inventario.actions — ajustarStockAction", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("retorna 201 cuando el ajuste es exitoso", async () => {
        ajustarServiceMock.mockResolvedValue({ data: "nuevo-mov-id", errors: undefined });

        const result = await ajustarStockAction(VALID_VALUES);

        expect(ajustarServiceMock).toHaveBeenCalledWith({
            productoId: VALID_VALUES[AJUSTE_STOCK_FIELDS.PRODUCTO_ID],
            delta: 10,
            razon: "Ingreso por compra",
        });
        expect(result).toEqual({ status: 201, errors: undefined });
    });

    it("acepta delta negativo (salida de stock)", async () => {
        ajustarServiceMock.mockResolvedValue({ data: "id", errors: undefined });

        const result = await ajustarStockAction({
            ...VALID_VALUES,
            [AJUSTE_STOCK_FIELDS.DELTA]: -5,
        });

        expect(ajustarServiceMock).toHaveBeenCalledWith(
            expect.objectContaining({ delta: -5 }),
        );
        expect(result.status).toBe(201);
    });

    it("convierte razon vacía en undefined (no la envía al backend)", async () => {
        ajustarServiceMock.mockResolvedValue({ data: "id", errors: undefined });

        await ajustarStockAction({
            ...VALID_VALUES,
            [AJUSTE_STOCK_FIELDS.RAZON]: "",
        });

        expect(ajustarServiceMock).toHaveBeenCalledWith(
            expect.objectContaining({ razon: undefined }),
        );
    });

    it("frena request inválida — productoId no UUID", async () => {
        const result = await ajustarStockAction({
            ...VALID_VALUES,
            [AJUSTE_STOCK_FIELDS.PRODUCTO_ID]: "no-es-uuid",
        });

        expect(ajustarServiceMock).not.toHaveBeenCalled();
        expect(result.status).toBe(400);
    });

    it("frena request inválida — delta igual a cero", async () => {
        const result = await ajustarStockAction({
            ...VALID_VALUES,
            [AJUSTE_STOCK_FIELDS.DELTA]: 0,
        });

        expect(ajustarServiceMock).not.toHaveBeenCalled();
        expect(result.status).toBe(400);
    });

    it("propaga error del servicio", async () => {
        ajustarServiceMock.mockResolvedValue({
            data: null,
            errors: {
                status: 404,
                title: "No encontrado",
                errors: { Producto_Id: "Producto no encontrado." },
            },
        });

        const result = await ajustarStockAction(VALID_VALUES);

        expect(result.status).toBe(404);
        expect(result.errors).toEqual({ Producto_Id: "Producto no encontrado." });
    });

    it("propaga error 422 (delta cero desde backend)", async () => {
        ajustarServiceMock.mockResolvedValue({
            data: null,
            errors: {
                status: 422,
                title: "Unprocessable",
                errors: { MovimientoStock_Delta: "El delta no puede ser cero." },
            },
        });

        const result = await ajustarStockAction(VALID_VALUES);

        expect(result.status).toBe(422);
    });
});
