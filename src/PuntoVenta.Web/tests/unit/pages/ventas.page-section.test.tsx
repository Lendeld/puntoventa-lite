import userEvent from "@testing-library/user-event";
import VentasPageSection from "@/app/(root)/emision/ventas/VentasPageSection";
import { render, screen } from "../../utils/render";
import { describe, expect, it, vi } from "vitest";

const useDocumentosVentaQueryMock = vi.fn();
const useCatalogosVentasQueryMock = vi.fn();

vi.mock("@lib/hooks/useDocumentosVentaQuery", () => ({
    useDocumentosVentaQuery: (params: unknown) => useDocumentosVentaQueryMock(params),
    useCatalogosVentasQuery: () => useCatalogosVentasQueryMock(),
}));

vi.mock("@components/ui/dates/AppDatePickerInput", () => ({
    AppDatePickerInput: () => <div>Filtro fechas</div>,
}));

vi.mock("@components/ui/AuditDateHoverCard", () => ({
    AuditDateHoverCard: ({ date }: { date: string }) => <span>{date}</span>,
}));

vi.mock("@lib/printing/venta-printing", () => ({
    getVentaPdfUrl: (documentoId: string) => `/api/ventas/${documentoId}/pdf`,
    printTicket: vi.fn(),
}));

describe("VentasPageSection", () => {
    it("muestra accion para navegar al detalle de venta", async () => {
        useDocumentosVentaQueryMock.mockReturnValue({
            data: {
                items: [
                    {
                        id: "venta-1",
                        tipoDocumento: 1,
                        estado: 2,
                        clienteId: null,
                        clienteNombre: "Cliente Demo",
                        clienteIdentificacion: "101010101",
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
                        totalPagado: 1130,
                        saldoPendiente: 0,
                        creadoPor: "Caja Principal",
                    },
                ],
                pagina: 1,
                tamano: 15,
                totalRegistros: 1,
                totalPaginas: 1,
            },
            isFetching: false,
            isError: false,
            refetch: vi.fn(),
        });
        useCatalogosVentasQueryMock.mockReturnValue({
            data: {
                tiposDocumento: [],
                estadosDocumento: [],
            },
        });

        const user = userEvent.setup();
        render(<VentasPageSection />);

        await user.click(screen.getByLabelText("Abrir acciones de venta"));

        expect((await screen.findByText("Ver detalle")).closest("a")).toHaveAttribute(
            "href",
            "/emision/ventas/venta-1",
        );
    });
});
