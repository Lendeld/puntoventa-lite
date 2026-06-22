import { beforeEach, describe, expect, it, vi } from "vitest";
import { PRODUCTO_FIELDS } from "@lib/constants/productos.constants";
import {
    crearProductoAction,
    editarProductoAction,
    toggleEstadoProductoAction,
} from "@lib/actions/productos.actions";
import {
    actualizarProductoService,
    crearProductoService,
    toggleEstadoProductoService,
} from "@lib/services/productos.service";

vi.mock("@lib/services/productos.service", () => ({
    crearProductoService: vi.fn(),
    actualizarProductoService: vi.fn(),
    toggleEstadoProductoService: vi.fn(),
    obtenerProductosService: vi.fn(),
    obtenerProductoPorIdService: vi.fn(),
}));

const crearServiceMock = vi.mocked(crearProductoService);
const actualizarServiceMock = vi.mocked(actualizarProductoService);
const toggleServiceMock = vi.mocked(toggleEstadoProductoService);

const VALID_VALUES = {
    [PRODUCTO_FIELDS.CODIGO]: "  ABC-001  ",
    [PRODUCTO_FIELDS.CODIGO_BARRAS]: "  1234567890  ",
    [PRODUCTO_FIELDS.NOMBRE]: "  Producto Test  ",
    [PRODUCTO_FIELDS.DESCRIPCION]: "  Descripción  ",
    [PRODUCTO_FIELDS.TIPO_ITEM]: 1,
    [PRODUCTO_FIELDS.PRECIO_UNITARIO]: 100,
    [PRODUCTO_FIELDS.PRECIO_COSTO]: 50,
    [PRODUCTO_FIELDS.CATEGORIA_ID]: "3c4d5e6f-7a8b-4c9d-ae1f-2a3b4c5d6e7f",
    [PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO]: undefined,
    [PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS]: false,
    [PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO]: false,
};

describe("productos.actions", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe("crearProductoAction", () => {
        it("trimea payload y retorna 201", async () => {
            crearServiceMock.mockResolvedValue({ data: "new-id", errors: undefined });

            const result = await crearProductoAction(VALID_VALUES);

            expect(crearServiceMock).toHaveBeenCalledWith(
                expect.objectContaining({
                    codigo: "ABC-001",
                    nombre: "Producto Test",
                    descripcion: "Descripción",
                    codigoBarras: "1234567890",
                }),
            );
            expect(result).toEqual({ status: 201, errors: undefined });
        });

        it("frena request inválida — codigo vacío", async () => {
            const result = await crearProductoAction({
                ...VALID_VALUES,
                [PRODUCTO_FIELDS.CODIGO]: "",
            });

            expect(crearServiceMock).not.toHaveBeenCalled();
            expect(result.status).toBe(400);
            expect(result.errors?.[PRODUCTO_FIELDS.CODIGO]).toBe("El código es requerido.");
        });

        it("frena request inválida — categoriaId no UUID", async () => {
            const result = await crearProductoAction({
                ...VALID_VALUES,
                [PRODUCTO_FIELDS.CATEGORIA_ID]: "no-uuid",
            });

            expect(crearServiceMock).not.toHaveBeenCalled();
            expect(result.status).toBe(400);
        });

        it("propaga error del servicio", async () => {
            crearServiceMock.mockResolvedValue({
                data: null,
                errors: {
                    status: 409,
                    title: "Conflicto",
                    errors: { [PRODUCTO_FIELDS.CODIGO]: "Código duplicado." },
                },
            });

            const result = await crearProductoAction(VALID_VALUES);

            expect(result.status).toBe(409);
            expect(result.errors).toEqual({
                [PRODUCTO_FIELDS.CODIGO]: "Código duplicado.",
            });
        });

        it("mapea descripcion vacía a null", async () => {
            crearServiceMock.mockResolvedValue({ data: "id", errors: undefined });

            await crearProductoAction({
                ...VALID_VALUES,
                [PRODUCTO_FIELDS.DESCRIPCION]: "",
            });

            expect(crearServiceMock).toHaveBeenCalledWith(
                expect.objectContaining({ descripcion: null }),
            );
        });

        it("envía permiteModificarPrecioUnitario true cuando está activo", async () => {
            crearServiceMock.mockResolvedValue({ data: "id", errors: undefined });

            await crearProductoAction({
                ...VALID_VALUES,
                [PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO]: true,
            });

            expect(crearServiceMock).toHaveBeenCalledWith(
                expect.objectContaining({ permiteModificarPrecioUnitario: true }),
            );
        });
    });

    describe("editarProductoAction", () => {
        it("trimea payload y retorna 204", async () => {
            actualizarServiceMock.mockResolvedValue({ data: null, errors: undefined });

            const result = await editarProductoAction("prod-id", VALID_VALUES);

            expect(actualizarServiceMock).toHaveBeenCalledWith(
                expect.objectContaining({
                    id: "prod-id",
                    codigo: "ABC-001",
                    nombre: "Producto Test",
                }),
            );
            expect(result).toEqual({ status: 204, errors: undefined });
        });

        it("frena request inválida — precio unitario 0 (bien)", async () => {
            const result = await editarProductoAction("prod-id", {
                ...VALID_VALUES,
                [PRODUCTO_FIELDS.TIPO_ITEM]: 1,
                [PRODUCTO_FIELDS.PRECIO_UNITARIO]: 0,
            });

            expect(actualizarServiceMock).not.toHaveBeenCalled();
            expect(result.status).toBe(400);
        });

        it("propaga error del servicio", async () => {
            actualizarServiceMock.mockResolvedValue({
                data: null,
                errors: {
                    status: 404,
                    title: "No encontrado",
                    errors: { Producto_Id: "Producto no encontrado." },
                },
            });

            const result = await editarProductoAction("prod-id", VALID_VALUES);

            expect(result.status).toBe(404);
        });

        it("envía permiteModificarPrecioUnitario true cuando está activo", async () => {
            actualizarServiceMock.mockResolvedValue({ data: null, errors: undefined });

            await editarProductoAction("prod-id", {
                ...VALID_VALUES,
                [PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO]: true,
            });

            expect(actualizarServiceMock).toHaveBeenCalledWith(
                expect.objectContaining({ permiteModificarPrecioUnitario: true }),
            );
        });
    });

    describe("toggleEstadoProductoAction", () => {
        it("retorna 200 al éxito", async () => {
            toggleServiceMock.mockResolvedValue({ data: false, errors: undefined });

            const result = await toggleEstadoProductoAction("prod-id");

            expect(toggleServiceMock).toHaveBeenCalledWith("prod-id");
            expect(result).toEqual({ status: 200, errors: undefined });
        });

        it("propaga error del servicio", async () => {
            toggleServiceMock.mockResolvedValue({
                data: null,
                errors: {
                    status: 404,
                    title: "No encontrado",
                    errors: { Producto_Id: "Producto no encontrado." },
                },
            });

            const result = await toggleEstadoProductoAction("prod-id");

            expect(result.status).toBe(404);
        });
    });
});
