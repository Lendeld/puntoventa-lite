import EmitirNotaCreditoDrawer from "@/app/(root)/emision/ventas/[id]/EmitirNotaCreditoDrawer";
import { emitirNotaCreditoAction } from "@lib/actions/ventas.actions";
import type {
    DocumentoVentaDto,
    DocumentoVentaLineaDto,
} from "@lib/types/ventas.types";
import { fireEvent, render, screen, waitFor } from "../../utils/render";
import { beforeEach, describe, expect, it, vi } from "vitest";

const pushMock = vi.fn();
const refreshMock = vi.fn();
const warningMock = vi.fn();
const successMock = vi.fn();
const imprimirTicketAutoMock = vi.fn().mockResolvedValue({ status: "ok" });

vi.mock("next/navigation", () => ({
    useRouter: () => ({ push: pushMock, refresh: refreshMock }),
}));

vi.mock("@lib/hooks/useMediosPagoActivosQuery", () => ({
    useMediosPagoActivosQuery: () => ({
        data: [
            {
                id: "mp-1",
                codigo: "01",
                detalle: "Efectivo",
                comentario: null,
                activo: true,
                modificadoPor: null,
            },
        ],
    }),
}));

vi.mock("@lib/printing/agent-url-context", () => ({
    useAgentUrl: () => "https://127.0.0.1:9123",
}));

vi.mock("@lib/printing/imprimir-ticket", () => ({
    imprimirTicketAuto: (...args: unknown[]) => imprimirTicketAutoMock(...args),
}));

vi.mock("@lib/actions/ventas.actions", () => ({
    emitirNotaCreditoAction: vi.fn(),
}));

vi.mock("@components/ui/AppNotifier", () => ({
    AppNotifier: {
        warning: (...a: unknown[]) => warningMock(...a),
        success: (...a: unknown[]) => successMock(...a),
        error: vi.fn(),
    },
}));

const emitirMock = vi.mocked(emitirNotaCreditoAction);

function linea(over: Partial<DocumentoVentaLineaDto>): DocumentoVentaLineaDto {
    return {
        id: "l-1",
        productoId: "prod-1",
        tipoItem: "Bien",
        codigo: "P-001",
        descripcion: "Producto uno",
        unidadMedidaCodigo: "Unid",
        cantidad: 1,
        precioUnitario: 1000,
        montoDescuento: 0,
        subtotal: 1000,
        montoImpuesto: 130,
        totalLinea: 1130,
        devuelveInventario: true,
        noAplicaExistencias: false,
        permiteModificarPrecioUnitario: true,
        cantidadDevueltaEnNotasCredito: 0,
        subtotalAcumuladoNotasCredito: 0,
        ...over,
    };
}

const documento = {
    id: "doc-1",
    consecutivo: "FAC-0000000001",
    monedaCodigo: "CRC",
    tipoDocumento: "Factura",
    totalComprobante: 1130,
    totalPagado: 0,
    documentosGenerados: [],
    lineas: [
        linea({ id: "l-1", productoId: "prod-1", descripcion: "Producto uno" }),
        linea({ id: "l-2", productoId: null, descripcion: "Servicio sin producto" }),
    ],
} as unknown as DocumentoVentaDto;

describe("EmitirNotaCreditoDrawer", () => {
    beforeEach(() => {
        pushMock.mockClear();
        refreshMock.mockClear();
        warningMock.mockClear();
        successMock.mockClear();
        emitirMock.mockReset();
    });

    it("muestra solo las líneas con producto", () => {
        render(
            <EmitirNotaCreditoDrawer opened onClose={vi.fn()} documento={documento} />,
        );

        expect(screen.getByText("Producto uno")).toBeInTheDocument();
        expect(
            screen.queryByText("Servicio sin producto"),
        ).not.toBeInTheDocument();
    });

    it("en Devolución exige al menos una línea con cantidad antes de emitir", () => {
        render(
            <EmitirNotaCreditoDrawer opened onClose={vi.fn()} documento={documento} />,
        );

        fireEvent.click(screen.getByRole("button", { name: /emitir nota/i }));

        expect(warningMock).toHaveBeenCalled();
        expect(emitirMock).not.toHaveBeenCalled();
    });

    it("cambiar a Anulación remonta el form y permite emitir sin tocar líneas", async () => {
        emitirMock.mockResolvedValue({
            status: 200,
            data: { id: "nc-9" },
            errors: undefined,
        } as Awaited<ReturnType<typeof emitirNotaCreditoAction>>);

        render(
            <EmitirNotaCreditoDrawer opened onClose={vi.fn()} documento={documento} />,
        );

        // Cambiar modo (outer state) → el inner se remonta con la key nueva.
        fireEvent.click(screen.getByText("Anulación total"));

        fireEvent.change(
            screen.getByRole("textbox", { name: /razón/i }),
            { target: { value: "Anulación por error de facturación" } },
        );

        fireEvent.click(screen.getByRole("button", { name: /emitir nota/i }));

        await waitFor(() => expect(emitirMock).toHaveBeenCalledTimes(1));
        const payload = emitirMock.mock.calls[0][0];
        expect(payload.documentoOrigenId).toBe("doc-1");
        expect(payload.modo).toBe(2); // Anulacion
        expect(payload.lineas).toEqual([]);

        await waitFor(() =>
            expect(pushMock).toHaveBeenCalledWith("/emision/ventas/nc-9"),
        );
    });
});
