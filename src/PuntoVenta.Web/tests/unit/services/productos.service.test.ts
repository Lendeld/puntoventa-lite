import { beforeEach, describe, expect, it, vi } from "vitest";
import {
    actualizarProductoService,
    crearProductoService,
    obtenerProductoPorIdService,
    obtenerProductosService,
    toggleEstadoProductoService,
} from "@lib/services/productos.service";
import { requestAPI } from "@lib/utils/requestApi";

vi.mock("@lib/utils/requestApi", () => ({
    requestAPI: vi.fn(),
}));

const requestAPIMock = vi.mocked(requestAPI);

describe("productos.service", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        requestAPIMock.mockResolvedValue({ data: null, errors: undefined });
    });

    it("obtenerProductosService manda query paginada con filtros", async () => {
        await obtenerProductosService({
            numeroPagina: 1,
            tamanoPagina: 10,
            filtroDinamico: "cafe",
            tipoItem: 1,
            categoriaId: "cat-id",
        });

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/productos",
            method: "GET",
            query: {
                NumeroPagina: 1,
                TamanoPagina: 10,
                FiltroDinamico: "cafe",
                TipoItem: 1,
                CategoriaId: "cat-id",
            },
        });
    });

    it("obtenerProductoPorIdService pide detalle por id", async () => {
        await obtenerProductoPorIdService("prod-1");

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/productos/prod-1",
            method: "GET",
        });
    });

    it("crearProductoService manda body completo", async () => {
        await crearProductoService({
            codigo: "ABC-001",
            nombre: "Producto",
            tipoItem: 1,
            precioUnitario: 100,
            codigoBarras: "1234567890",
            descripcion: "Descripción",
            precioCosto: 50,
            categoriaId: "cat-id",
            tarifaIvaImpuestoCodigo: "08",
            noAplicaExistencias: false,
            permiteModificarPrecioUnitario: false,
        });

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/productos",
            method: "POST",
            body: expect.objectContaining({
                codigo: "ABC-001",
                nombre: "Producto",
                tipoItem: 1,
                precioUnitario: 100,
                tarifaIvaImpuestoCodigo: "08",
            }),
        });
    });

    it("actualizarProductoService usa ruta PUT con id y body sin id", async () => {
        await actualizarProductoService({
            id: "prod-1",
            codigo: "ABC-001",
            nombre: "Producto",
            tipoItem: 1,
            precioUnitario: 100,
            codigoBarras: null,
            descripcion: null,
            precioCosto: null,
            categoriaId: null,
            tarifaIvaImpuestoCodigo: null,
            noAplicaExistencias: false,
            permiteModificarPrecioUnitario: false,
        });

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/productos/prod-1",
            method: "PUT",
            body: expect.objectContaining({
                codigo: "ABC-001",
                nombre: "Producto",
                tipoItem: 1,
                precioUnitario: 100,
            }),
        });
    });

    it("actualizarProductoService envía permiteModificarPrecioUnitario true", async () => {
        await actualizarProductoService({
            id: "prod-1",
            codigo: "ABC-001",
            nombre: "Producto",
            tipoItem: 1,
            precioUnitario: 100,
            codigoBarras: null,
            descripcion: null,
            precioCosto: null,
            categoriaId: null,
            tarifaIvaImpuestoCodigo: null,
            noAplicaExistencias: false,
            permiteModificarPrecioUnitario: true,
        });

        expect(requestAPIMock).toHaveBeenCalledWith(
            expect.objectContaining({
                body: expect.objectContaining({ permiteModificarPrecioUnitario: true }),
            }),
        );
    });

    it("toggleEstadoProductoService usa PATCH /estado", async () => {
        await toggleEstadoProductoService("prod-1");

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/productos/prod-1/estado",
            method: "PATCH",
        });
    });
});
