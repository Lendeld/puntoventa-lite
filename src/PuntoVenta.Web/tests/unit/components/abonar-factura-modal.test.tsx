import { AbonarFacturaModal } from "@/app/(root)/emision/cobros/credito/AbonarFacturaModal";
import { render, screen } from "../../utils/render";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

const registrarAbonoFacturaActionMock = vi.fn();

vi.mock("@lib/actions/ventas.actions", () => ({
    registrarAbonoFacturaAction: (...args: unknown[]) =>
        registrarAbonoFacturaActionMock(...args),
}));

vi.mock("@lib/hooks/useMediosPagoActivosQuery", () => ({
    useMediosPagoActivosQuery: () => ({
        data: [
            { codigo: "01", detalle: "Efectivo" },
            { codigo: "02", detalle: "Transferencia" },
        ],
    }),
}));

vi.mock("@components/ui/AppNotifier", () => ({
    AppNotifier: {
        success: vi.fn(),
        error: vi.fn(),
        warning: vi.fn(),
    },
}));

describe("AbonarFacturaModal", () => {
    it("permite abonar sin referencia para medios distintos de efectivo", async () => {
        registrarAbonoFacturaActionMock.mockResolvedValue({
            status: 200,
            errors: undefined,
            data: { pagoId: "pago-1" },
        });

        const user = userEvent.setup();

        render(
            <AbonarFacturaModal
                factura={{
                    id: "factura-1",
                    consecutivo: "FAC-0001",
                    fechaDocumento: "2026-06-21T12:00:00Z",
                    fechaVencimiento: "2026-06-30T12:00:00Z",
                    plazoCreditoDias: 8,
                    clienteId: "cliente-1",
                    clienteNombre: "Cliente Demo",
                    clienteIdentificacion: "101010101",
                    condicionVentaCodigo: "02",
                    condicionVentaDetalleSnapshot: "Crédito",
                    totalComprobante: 15000,
                    totalPagado: 5000,
                    saldoPendiente: 10000,
                    diasAtraso: 0,
                    esVencida: false,
                }}
                onClose={vi.fn()}
                onSuccess={vi.fn()}
            />,
        );

        await user.click(screen.getByRole("combobox", { name: "Medio de pago" }));
        await user.click(screen.getByRole("option", { name: "Transferencia" }));

        expect(screen.getByLabelText("Referencia")).not.toBeRequired();

        await user.click(screen.getByRole("button", { name: "Registrar abono" }));

        expect(registrarAbonoFacturaActionMock).toHaveBeenCalledWith(
            "factura-1",
            expect.objectContaining({
                medioPagoCodigo: "02",
                referencia: null,
            }),
        );
        expect(await screen.findByText("Abono registrado")).toBeInTheDocument();
        expect(screen.getByRole("link", { name: "Ver PDF" })).toHaveAttribute(
            "href",
            "/pdf/ventas/factura-1/abonos/pago-1",
        );
    });
});
