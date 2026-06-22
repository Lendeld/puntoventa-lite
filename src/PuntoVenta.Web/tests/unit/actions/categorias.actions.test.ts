import { beforeEach, describe, expect, it, vi } from "vitest";
import { CATEGORIA_FIELDS } from "@lib/constants/categorias.constants";
import {
    actualizarCategoriaAction,
    crearCategoriaAction,
} from "@lib/actions/categorias.actions";
import {
    actualizarCategoriaService,
    crearCategoriaService,
} from "@lib/services/categorias.service";

vi.mock("@lib/services/categorias.service", () => ({
    crearCategoriaService: vi.fn(),
    actualizarCategoriaService: vi.fn(),
}));

const crearCategoriaServiceMock = vi.mocked(crearCategoriaService);
const actualizarCategoriaServiceMock = vi.mocked(actualizarCategoriaService);

const crearValues = {
    [CATEGORIA_FIELDS.NOMBRE]: "  Ropa  ",
    [CATEGORIA_FIELDS.DESCRIPCION]: "  Moda  ",
};

const actualizarValues = {
    [CATEGORIA_FIELDS.NOMBRE]: "  Calzado  ",
    [CATEGORIA_FIELDS.DESCRIPCION]: "  Sport  ",
    [CATEGORIA_FIELDS.ACTIVO]: false,
};

describe("categorias.actions", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("crearCategoriaAction corta payload y retorna 201", async () => {
        crearCategoriaServiceMock.mockResolvedValue({ data: "id", errors: undefined });

        const result = await crearCategoriaAction(crearValues);

        expect(crearCategoriaServiceMock).toHaveBeenCalledWith({
            nombre: "Ropa",
            descripcion: "Moda",
        });
        expect(result).toEqual({ status: 201, errors: undefined });
    });

    it("crearCategoriaAction frena request invalida", async () => {
        const result = await crearCategoriaAction({
            ...crearValues,
            [CATEGORIA_FIELDS.NOMBRE]: "",
        });

        expect(crearCategoriaServiceMock).not.toHaveBeenCalled();
        expect(result.status).toBe(400);
        expect(result.errors?.[CATEGORIA_FIELDS.NOMBRE]).toBe("El nombre es requerido.");
    });

    it("actualizarCategoriaAction convierte descripcion vacia a null", async () => {
        actualizarCategoriaServiceMock.mockResolvedValue({ data: null, errors: undefined });

        await actualizarCategoriaAction("cat-id", {
            ...actualizarValues,
            [CATEGORIA_FIELDS.DESCRIPCION]: "   ",
        });

        expect(actualizarCategoriaServiceMock).toHaveBeenCalledWith({
            id: "cat-id",
            nombre: "Calzado",
            descripcion: null,
            activo: false,
        });
    });
});
