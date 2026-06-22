import EmitirNotaDebitoDrawer from "@/app/(root)/emision/ventas/[id]/EmitirNotaDebitoDrawer";
import { emitirNotaDebitoAction } from "@lib/actions/ventas.actions";
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
const errorMock = vi.fn();
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
    emitirNotaDebitoAction: vi.fn(),
}));

vi.mock("@components/ui/AppNotifier", () => ({
    AppNotifier: {
        warning: (...a: unknown[]) => warningMock(...a),
        success: (...a: unknown[]) => successMock(...a),
        error: (...a: unknown[]) => errorMock(...a),
    },
}));

const emitirMock = vi.mocked(emitirNotaDebitoAction);

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
    lineas: [
        linea({ id: "l-1", productoId: "prod-1", descripcion: "Producto uno" }),
        // Línea sin productoId: debe filtrarse (no elegible para ND).
        linea({ id: "l-2", productoId: null, descripcion: "Servicio sin producto" }),
    ],
} as unknown as DocumentoVentaDto;

function getMontoCargoInput(): HTMLInputElement {
    // Primer NumberInput de la tabla de líneas (monto a cobrar).
    return screen.getAllByRole("textbox").find((el) =>
        (el as HTMLInputElement).getAttribute("inputmode") === "numeric" ||
        (el as HTMLInputElement).getAttribute("inputmode") === "decimal",
    ) as HTMLInputElement;
}

describe("EmitirNotaDebitoDrawer", () => {
    beforeEach(() => {
        pushMock.mockClear();
        refreshMock.mockClear();
        warningMock.mockClear();
        successMock.mockClear();
        emitirMock.mockReset();
    });

    it("muestra solo las líneas con producto", () => {
        render(
            <EmitirNotaDebitoDrawer opened onClose={vi.fn()} documento={documento} />,
        );

        expect(screen.getByText("Producto uno")).toBeInTheDocument();
        expect(
            screen.queryByText("Servicio sin producto"),
        ).not.toBeInTheDocument();
    });

    it("valida que haya al menos una línea con cargo antes de emitir", () => {
        render(
            <EmitirNotaDebitoDrawer opened onClose={vi.fn()} documento={documento} />,
        );

        fireEvent.click(screen.getByRole("button", { name: /emitir nota/i }));

        expect(warningMock).toHaveBeenCalled();
        expect(emitirMock).not.toHaveBeenCalled();
    });

    it("emite la ND con el cargo y la razón, y navega al nuevo documento", async () => {
        emitirMock.mockResolvedValue({
            status: 200,
            data: { id: "nd-9" },
            errors: undefined,
        } as Awaited<ReturnType<typeof emitirNotaDebitoAction>>);

        render(
            <EmitirNotaDebitoDrawer opened onClose={vi.fn()} documento={documento} />,
        );

        fireEvent.change(getMontoCargoInput(), { target: { value: "500" } });
        fireEvent.change(
            screen.getByRole("textbox", { name: /razón/i }),
            { target: { value: "Cargo por flete" } },
        );

        fireEvent.click(screen.getByRole("button", { name: /emitir nota/i }));

        await waitFor(() => expect(emitirMock).toHaveBeenCalledTimes(1));
        const payload = emitirMock.mock.calls[0][0];
        expect(payload.documentoOrigenId).toBe("doc-1");
        expect(payload.razon).toBe("Cargo por flete");
        expect(payload.lineas).toHaveLength(1);
        expect(payload.lineas[0].productoId).toBe("prod-1");

        await waitFor(() =>
            expect(pushMock).toHaveBeenCalledWith("/emision/ventas/nd-9"),
        );
    });
});
