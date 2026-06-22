import { beforeEach, describe, expect, it, vi } from "vitest";
import {
    anularAbonoFacturaService,
    crearFacturaService,
    obtenerDocumentoVentaPorIdService,
    obtenerReporteMovimientosDineroService,
} from "@lib/services/ventas.service";
import { requestAPI } from "@lib/utils/requestApi";

vi.mock("@lib/utils/requestApi", () => ({
    requestAPI: vi.fn(),
}));

const requestAPIMock = vi.mocked(requestAPI);

describe("ventas.service", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        requestAPIMock.mockResolvedValue({ data: null, errors: undefined });
    });

    it("crearFacturaService envía POST con body", async () => {
        await crearFacturaService({
            clienteId: null,
            vendedorId: null,
            condicionVentaCodigo: "01",
            lineas: [],
            pagos: [],
            plazoCreditoDias: null,
            fechaDocumento: "2026-05-01T00:00:00",
            monedaCodigo: "CRC",
            tipoCambio: 1,
            observaciones: null,
        });

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/ventas/facturas",
            method: "POST",
            body: expect.objectContaining({
                condicionVentaCodigo: "01",
                monedaCodigo: "CRC",
            }),
        });
    });

    it("obtenerDocumentoVentaPorIdService pide detalle", async () => {
        await obtenerDocumentoVentaPorIdService("doc-1");

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/ventas/doc-1",
            method: "GET",
        });
    });

    it("anularAbonoFacturaService usa el endpoint esperado", async () => {
        await anularAbonoFacturaService("doc-1", "pago-1", { motivo: "duplicado" });

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/ventas/doc-1/abonos/pago-1/anular",
            method: "POST",
            body: { motivo: "duplicado" },
        });
    });

    it("obtenerReporteMovimientosDineroService envía query del rango", async () => {
        await obtenerReporteMovimientosDineroService({
            fechaDesde: "2026-06-21T06:00:00.000Z",
            fechaHasta: "2026-06-22T05:59:59.999Z",
            cajaId: "caja-1",
        });

        expect(requestAPIMock).toHaveBeenCalledWith({
            url: "/ventas/reportes/movimientos-dinero",
            method: "GET",
            query: {
                FechaDesde: "2026-06-21T06:00:00.000Z",
                FechaHasta: "2026-06-22T05:59:59.999Z",
                CajaId: "caja-1",
            },
        });
    });
});
