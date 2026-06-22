import { FacturacionResultadoDrawer } from "@/app/(root)/emision/facturacion/FacturacionResultadoDrawer";
import { useImprimirTicketAhora } from "@lib/printing/imprimir-ticket";
import type { DocumentoVentaDto } from "@lib/types/ventas.types";
import { fireEvent, render, screen } from "../../utils/render";
import { describe, expect, it, vi } from "vitest";

const imprimirMock = vi.fn();

vi.mock("@lib/printing/imprimir-ticket", () => ({
    useImprimirTicketAhora: vi.fn(() => imprimirMock),
}));

const documento: DocumentoVentaDto = {
    id: "doc-1",
    tipoDocumento: "Factura",
    estado: "Emitido",
    clienteId: null,
    clienteNombre: null,
    clienteIdentificacion: null,
    vendedorId: null,
    vendedorNombre: null,
    consecutivo: "FAC-0000000001",
    fechaDocumento: "2026-05-01",
    tipoDocumentoDetalle: "Factura",
    tipoDocumentoColor: "blue",
    estadoDetalle: "Emitido",
    estadoColor: "green",
    condicionVentaCodigo: "01",
    condicionVentaDetalleSnapshot: "Contado",
    totalComprobante: 1130,
    totalPagado: 1200,
    saldoPendiente: 0,
    montoRedondeo: 0,
    plazoCreditoDias: null,
    fechaVencimiento: null,
    monedaCodigo: "CRC",
    tipoCambio: 1,
    totalVenta: 1000,
    totalDescuentos: 0,
    totalImpuesto: 130,
    observaciones: null,
    fechaCancelacion: null,
    esCredito: false,
    creadoPor: "Caja Principal",
    lineas: [],
    pagos: [],
    referencias: [],
    documentoOrigen: null,
    documentosGenerados: [],
};

describe("FacturacionResultadoDrawer", () => {
    it("muestra factura emitida, vuelto y acciones principales", () => {
        imprimirMock.mockClear();
        render(
            <FacturacionResultadoDrawer
                opened
                documento={documento}
                vuelto={70}
                onClose={vi.fn()}
            />,
        );

        expect(screen.getByText(/factura emitida/i)).toBeInTheDocument();
        expect(screen.getByText("FAC-0000000001")).toBeInTheDocument();
        expect(screen.getByText("₡ 70.00")).toBeInTheDocument();
        expect(screen.getByRole("link", { name: /ver pdf/i })).toHaveAttribute(
            "href",
            "/pdf/ventas/doc-1",
        );

        fireEvent.click(screen.getByRole("button", { name: /imprimir ticket/i }));

        expect(useImprimirTicketAhora).toHaveBeenCalled();
        expect(imprimirMock).toHaveBeenCalledWith("doc-1");
    });

    it("limpia la confirmacion al cerrar", () => {
        const onClose = vi.fn();

        render(
            <FacturacionResultadoDrawer
                opened
                documento={documento}
                vuelto={0}
                onClose={onClose}
            />,
        );

        fireEvent.click(
            screen.getByRole("button", { name: /cerrar confirmacion/i }),
        );

        expect(onClose).toHaveBeenCalled();
    });
});
