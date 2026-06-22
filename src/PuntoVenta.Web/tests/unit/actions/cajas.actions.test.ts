import { beforeEach, describe, expect, it, vi } from "vitest";
import { CAJA_FIELDS } from "@lib/constants/cajas.constants";
import {
    actualizarCajaAction,
    crearCajaAction,
    toggleEstadoCajaAction,
} from "@lib/actions/cajas.actions";
import {
    actualizarCajaService,
    crearCajaService,
    toggleEstadoCajaService,
} from "@lib/services/cajas.service";

vi.mock("@lib/services/cajas.service", () => ({
    crearCajaService: vi.fn(),
    actualizarCajaService: vi.fn(),
    toggleEstadoCajaService: vi.fn(),
}));

const crearCajaServiceMock = vi.mocked(crearCajaService);
const actualizarCajaServiceMock = vi.mocked(actualizarCajaService);
const toggleEstadoCajaServiceMock = vi.mocked(toggleEstadoCajaService);

const crearValues = {
    [CAJA_FIELDS.CODIGO]: "  POS01  ",
    [CAJA_FIELDS.NOMBRE]: "  Caja principal  ",
};

const actualizarValues = {
    [CAJA_FIELDS.CODIGO]: "  POS02  ",
    [CAJA_FIELDS.NOMBRE]: "  Caja secundaria  ",
};

const errorConflicto = {
    title: "Conflict",
    status: 409,
    errors: { [CAJA_FIELDS.CODIGO]: "Código ya existe." },
};

const errorNoEncontrado = {
    title: "Not Found",
    status: 404,
    errors: { Caja: "No encontrada." },
};

describe("cajas.actions", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe("crearCajaAction", () => {
        it("normaliza el payload y retorna 201", async () => {
            crearCajaServiceMock.mockResolvedValue({ data: "new-id", errors: undefined });

            const result = await crearCajaAction(crearValues);

            expect(crearCajaServiceMock).toHaveBeenCalledWith({
                codigo: "POS01",
                nombre: "Caja principal",
            });
            expect(result).toEqual({ status: 201, errors: undefined });
        });

        it("frena request con codigo vacio", async () => {
            const result = await crearCajaAction({
                ...crearValues,
                [CAJA_FIELDS.CODIGO]: "",
            });

            expect(crearCajaServiceMock).not.toHaveBeenCalled();
            expect(result.status).toBe(400);
            expect(result.errors?.[CAJA_FIELDS.CODIGO]).toBe("El código es requerido.");
        });

        it("propaga error del servicio", async () => {
            crearCajaServiceMock.mockResolvedValue({
                data: undefined,
                errors: errorConflicto,
            });

            const result = await crearCajaAction(crearValues);

            expect(result.status).toBe(409);
        });
    });

    describe("actualizarCajaAction", () => {
        it("normaliza el payload y retorna 204", async () => {
            actualizarCajaServiceMock.mockResolvedValue({ data: null, errors: undefined });

            const result = await actualizarCajaAction("caja-id", actualizarValues);

            expect(actualizarCajaServiceMock).toHaveBeenCalledWith({
                id: "caja-id",
                codigo: "POS02",
                nombre: "Caja secundaria",
            });
            expect(result).toEqual({ status: 204, errors: undefined });
        });

        it("frena request con nombre vacio", async () => {
            const result = await actualizarCajaAction("caja-id", {
                ...actualizarValues,
                [CAJA_FIELDS.NOMBRE]: "",
            });

            expect(actualizarCajaServiceMock).not.toHaveBeenCalled();
            expect(result.status).toBe(400);
        });
    });

    describe("toggleEstadoCajaAction", () => {
        it("llama al servicio y retorna 200", async () => {
            toggleEstadoCajaServiceMock.mockResolvedValue({ data: null, errors: undefined });

            const result = await toggleEstadoCajaAction("caja-id");

            expect(toggleEstadoCajaServiceMock).toHaveBeenCalledWith("caja-id");
            expect(result).toEqual({ status: 200, errors: undefined });
        });

        it("propaga error del servicio", async () => {
            toggleEstadoCajaServiceMock.mockResolvedValue({
                data: undefined,
                errors: errorNoEncontrado,
            });

            const result = await toggleEstadoCajaAction("caja-id");

            expect(result.status).toBe(404);
        });
    });
});
