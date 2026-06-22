import { beforeEach, describe, expect, it, vi } from "vitest";
import { VENTA_FIELDS } from "@lib/constants/ventas.constants";
import {
    anularAbonoFacturaAction,
    crearFacturaAction,
    registrarAbonoFacturaAction,
} from "@lib/actions/ventas.actions";
import {
    anularAbonoFacturaService,
    crearFacturaService,
    obtenerDocumentoVentaPorIdService,
    registrarAbonoFacturaService,
} from "@lib/services/ventas.service";
import dayjs from "dayjs";

vi.mock("@lib/services/ventas.service", () => ({
    crearFacturaService: vi.fn(),
    obtenerDocumentoVentaPorIdService: vi.fn(),
    registrarAbonoFacturaService: vi.fn(),
    anularAbonoFacturaService: vi.fn(),
}));

const crearServiceMock = vi.mocked(crearFacturaService);
const obtenerDetalleMock = vi.mocked(obtenerDocumentoVentaPorIdService);
const registrarAbonoFacturaServiceMock = vi.mocked(registrarAbonoFacturaService);
const anularAbonoFacturaServiceMock = vi.mocked(anularAbonoFacturaService);

const VALID_VALUES = {
    [VENTA_FIELDS.CLIENTE_ID]: "",
    [VENTA_FIELDS.VENDEDOR_ID]: "",
    [VENTA_FIELDS.CAJA_ID]: "",
    [VENTA_FIELDS.CONDICION_VENTA_CODIGO]: "01",
    [VENTA_FIELDS.FECHA_DOCUMENTO]: "2026-05-01T14:35",
    [VENTA_FIELDS.FECHA_VENCIMIENTO]: "",
    [VENTA_FIELDS.MONEDA_CODIGO]: "crc",
    [VENTA_FIELDS.TIPO_CAMBIO]: 1,
    [VENTA_FIELDS.PLAZO_CREDITO_DIAS]: null,
    [VENTA_FIELDS.OBSERVACIONES]: "  venta rápida ",
    [VENTA_FIELDS.LINEAS]: [
        {
            ProductoId: "0f0a61b3-5cc4-4dd4-92f2-2f7da4126c12",
            TipoItem: "Bien" as const,
            Codigo: "P-001",
            Descripcion: "Producto 1",
            Cantidad: 1,
            PrecioUnitario: 1000,
            MontoDescuento: 0,
            TarifaIvaImpuestoCodigo: "08",
            PorcentajeImpuesto: 13,
        },
    ],
    [VENTA_FIELDS.PAGOS]: [
        {
            MonedaCodigo: "USD",
            TipoCambioAplicado: 540.12345,
            MedioPagoCodigo: "01",
            MontoEntregado: 3,
            MontoAplicadoMonedaPago: 2.0921,
            MontoAplicadoDocumento: 1130,
            MontoVueltoMonedaPago: 0.9079,
            MontoVueltoDocumento: 490.87822,
            Referencia: " ref-1 ",
            Observacion: " obs-1 ",
        },
    ],
};

describe("ventas.actions", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("crea factura, normaliza payload y trae detalle", async () => {
        crearServiceMock.mockResolvedValue({ data: "doc-1", errors: undefined });
        obtenerDetalleMock.mockResolvedValue({
            data: { id: "doc-1", estado: 2, consecutivo: "FAC-0001" } as never,
            errors: undefined,
        });

        const result = await crearFacturaAction(VALID_VALUES);

        expect(crearServiceMock).toHaveBeenCalledWith(expect.objectContaining({
            monedaCodigo: "CRC",
            observaciones: "venta rápida",
            vendedorId: null,
            fechaDocumento: dayjs("2026-05-01T14:35").toISOString(),
            pagos: [
                expect.objectContaining({
                    monedaCodigo: "USD",
                    tipoCambioAplicado: 540.12345,
                    montoEntregado: 3,
                    montoAplicadoDocumento: 1130,
                }),
            ],
        }));
        expect(result.status).toBe(201);
        expect(result.data?.id).toBe("doc-1");
    });

    it("frena request inválida antes del service", async () => {
        const result = await crearFacturaAction({
            ...VALID_VALUES,
            [VENTA_FIELDS.LINEAS]: [],
        });

        expect(crearServiceMock).not.toHaveBeenCalled();
        expect(result.status).toBe(400);
        expect(result.errors?.[VENTA_FIELDS.LINEAS]).toBeDefined();
    });

    it("propaga error del service al crear factura", async () => {
        crearServiceMock.mockResolvedValue({
            data: null,
            errors: {
                status: 409,
                title: "Conflicto",
                errors: { DocumentoVenta_Pagos: "Pago inválido." },
            },
        });

        const result = await crearFacturaAction(VALID_VALUES);

        expect(result.status).toBe(409);
        expect(result.errors).toEqual({ DocumentoVenta_Pagos: "Pago inválido." });
    });

    it("registrarAbonoFacturaAction devuelve pagoId y serializa fecha", async () => {
        registrarAbonoFacturaServiceMock.mockResolvedValue({
            data: "pago-123",
            errors: undefined,
        });

        const result = await registrarAbonoFacturaAction("factura-1", {
            monedaCodigo: "crc",
            medioPagoCodigo: "02",
            monto: 15000,
            referencia: "ref-22",
            observacion: "abono parcial",
            fechaPago: "2026-05-01T14:35",
        });

        expect(registrarAbonoFacturaServiceMock).toHaveBeenCalledWith(
            "factura-1",
            expect.objectContaining({
                fechaPago: dayjs("2026-05-01T14:35").toISOString(),
                pago: expect.objectContaining({
                    monedaCodigo: "CRC",
                    medioPagoCodigo: "02",
                    montoAplicadoDocumento: 15000,
                    referencia: "ref-22",
                }),
            }),
        );
        expect(result.status).toBe(200);
        expect(result.data?.pagoId).toBe("pago-123");
    });

    it("anularAbonoFacturaAction valida motivo y retorna pagoId", async () => {
        anularAbonoFacturaServiceMock.mockResolvedValue({
            data: "pago-123",
            errors: undefined,
        });

        const invalid = await anularAbonoFacturaAction("factura-1", "pago-123", "no");
        expect(invalid.status).toBe(400);
        expect(anularAbonoFacturaServiceMock).not.toHaveBeenCalled();

        const valid = await anularAbonoFacturaAction(
            "factura-1",
            "pago-123",
            "  cliente anuló el depósito  ",
        );

        expect(anularAbonoFacturaServiceMock).toHaveBeenCalledWith(
            "factura-1",
            "pago-123",
            { motivo: "cliente anuló el depósito" },
        );
        expect(valid.data?.pagoId).toBe("pago-123");
    });
});
