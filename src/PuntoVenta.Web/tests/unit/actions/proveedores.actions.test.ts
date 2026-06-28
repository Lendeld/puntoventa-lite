import { beforeEach, describe, expect, it, vi } from "vitest";
import { PROVEEDOR_FIELDS } from "@lib/constants/proveedores.constants";
import {
    actualizarProveedorAction,
    crearProveedorAction,
} from "@lib/actions/proveedores.actions";
import {
    actualizarProveedorService,
    crearProveedorService,
} from "@lib/services/proveedores.service";

vi.mock("@lib/services/proveedores.service", () => ({
    crearProveedorService: vi.fn(),
    actualizarProveedorService: vi.fn(),
}));

const crearProveedorServiceMock = vi.mocked(crearProveedorService);
const actualizarProveedorServiceMock = vi.mocked(actualizarProveedorService);

const crearValues = {
    [PROVEEDOR_FIELDS.NOMBRE]: "  ACME S.A.  ",
    [PROVEEDOR_FIELDS.CORREO]: "  ventas@acme.cr  ",
    [PROVEEDOR_FIELDS.TELEFONO]: "  2222-3333  ",
    [PROVEEDOR_FIELDS.OBSERVACION]: "  Zona norte  ",
};

const actualizarValues = {
    ...crearValues,
    [PROVEEDOR_FIELDS.ACTIVO]: true,
};

const errorConflicto = {
    title: "Conflict",
    status: 409,
    errors: { [PROVEEDOR_FIELDS.NOMBRE]: "El nombre ya existe." },
};

describe("proveedores.actions", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe("crearProveedorAction", () => {
        it("normaliza el payload y retorna 201", async () => {
            crearProveedorServiceMock.mockResolvedValue({ data: "new-guid", errors: undefined });

            const result = await crearProveedorAction(crearValues);

            expect(crearProveedorServiceMock).toHaveBeenCalledWith({
                nombre: "ACME S.A.",
                correo: "ventas@acme.cr",
                telefono: "2222-3333",
                observacion: "Zona norte",
            });
            expect(result).toEqual({ status: 201, errors: undefined });
        });

        it("correo vacio se mapea como null", async () => {
            crearProveedorServiceMock.mockResolvedValue({ data: "new-guid", errors: undefined });

            await crearProveedorAction({
                ...crearValues,
                [PROVEEDOR_FIELDS.CORREO]: "",
            });

            expect(crearProveedorServiceMock).toHaveBeenCalledWith(
                expect.objectContaining({ correo: null }),
            );
        });

        it("frena request con nombre vacio", async () => {
            const result = await crearProveedorAction({
                ...crearValues,
                [PROVEEDOR_FIELDS.NOMBRE]: "",
            });

            expect(crearProveedorServiceMock).not.toHaveBeenCalled();
            expect(result.status).toBe(400);
            expect(result.errors?.[PROVEEDOR_FIELDS.NOMBRE]).toBe("El nombre es requerido.");
        });

        it("frena request con correo invalido", async () => {
            const result = await crearProveedorAction({
                ...crearValues,
                [PROVEEDOR_FIELDS.CORREO]: "no-es-correo",
            });

            expect(crearProveedorServiceMock).not.toHaveBeenCalled();
            expect(result.status).toBe(400);
        });

        it("propaga error del servicio", async () => {
            crearProveedorServiceMock.mockResolvedValue({
                data: undefined,
                errors: errorConflicto,
            });

            const result = await crearProveedorAction(crearValues);

            expect(result.status).toBe(409);
        });
    });

    describe("actualizarProveedorAction", () => {
        it("normaliza el payload y retorna 204", async () => {
            actualizarProveedorServiceMock.mockResolvedValue({ data: null, errors: undefined });

            const result = await actualizarProveedorAction("proveedor-id", actualizarValues);

            expect(actualizarProveedorServiceMock).toHaveBeenCalledWith({
                id: "proveedor-id",
                nombre: "ACME S.A.",
                correo: "ventas@acme.cr",
                telefono: "2222-3333",
                observacion: "Zona norte",
                activo: true,
            });
            expect(result).toEqual({ status: 204, errors: undefined });
        });

        it("frena request con nombre vacio", async () => {
            const result = await actualizarProveedorAction("proveedor-id", {
                ...actualizarValues,
                [PROVEEDOR_FIELDS.NOMBRE]: "",
            });

            expect(actualizarProveedorServiceMock).not.toHaveBeenCalled();
            expect(result.status).toBe(400);
        });

        it("propaga error del servicio", async () => {
            actualizarProveedorServiceMock.mockResolvedValue({
                data: undefined,
                errors: errorConflicto,
            });

            const result = await actualizarProveedorAction("proveedor-id", actualizarValues);

            expect(result.status).toBe(409);
        });
    });
});
