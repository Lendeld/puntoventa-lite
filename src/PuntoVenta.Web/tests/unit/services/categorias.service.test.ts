import { beforeEach, describe, expect, it, vi } from "vitest";
import {
    actualizarCategoriaService,
    crearCategoriaService,
    obtenerCategoriaPorIdService,
    obtenerCategoriasService,
} from "@lib/services/categorias.service";
import { requestAPI } from "@lib/utils/requestApi";

vi.mock("@lib/utils/requestApi", () => ({
    requestAPI: vi.fn(),
}));

const requestAPIMock = vi.mocked(requestAPI);

describe("categorias.service", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        requestAPIMock.mockResolvedValue({ data: null, errors: undefined });
    });

    it("obtenerCategoriasService manda query paginada", async () => {
        await obtenerCategoriasService({
            numeroPagina: 1,
            tamanoPagina: 15,
            filtroDinamico: "ropa",
            activo: false,
        });

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/categorias",
            method: "GET",
            query: {
                NumeroPagina: 1,
                TamanoPagina: 15,
                FiltroDinamico: "ropa",
                Activo: false,
            },
        });
    });

    it("obtenerCategoriaPorIdService pide detalle por id", async () => {
        await obtenerCategoriaPorIdService("cat-1");

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/categorias/cat-1",
            method: "GET",
        });
    });

    it("crearCategoriaService manda body esperado", async () => {
        await crearCategoriaService({ nombre: "Ropa", descripcion: "Moda" });

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/categorias",
            method: "POST",
            body: {
                nombre: "Ropa",
                descripcion: "Moda",
            },
        });
    });

    it("actualizarCategoriaService usa ruta PUT con id", async () => {
        await actualizarCategoriaService({
            id: "cat-1",
            nombre: "Ropa",
            descripcion: null,
            activo: true,
        });

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/categorias/cat-1",
            method: "PUT",
            body: {
                nombre: "Ropa",
                descripcion: null,
                activo: true,
            },
        });
    });

});
