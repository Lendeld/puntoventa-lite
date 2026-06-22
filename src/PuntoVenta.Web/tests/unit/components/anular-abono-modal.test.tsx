import { AnularAbonoModal } from "@/app/(root)/emision/ventas/[id]/AnularAbonoModal";
import { render, screen } from "../../utils/render";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

const anularAbonoFacturaActionMock = vi.fn();
const refreshMock = vi.fn();

vi.mock("@lib/actions/ventas.actions", () => ({
    anularAbonoFacturaAction: (...args: unknown[]) =>
        anularAbonoFacturaActionMock(...args),
}));

vi.mock("next/navigation", () => ({
    useRouter: () => ({
        refresh: refreshMock,
        push: vi.fn(),
        replace: vi.fn(),
        back: vi.fn(),
        forward: vi.fn(),
        prefetch: vi.fn(),
    }),
}));

vi.mock("@components/ui/AppNotifier", () => ({
    AppNotifier: {
        success: vi.fn(),
        error: vi.fn(),
        warning: vi.fn(),
    },
}));

describe("AnularAbonoModal", () => {
    it("valida motivo obligatorio y muestra resultado al anular", async () => {
        const user = userEvent.setup();
        anularAbonoFacturaActionMock.mockResolvedValue({
            status: 200,
            errors: undefined,
            data: { pagoId: "pago-1" },
        });

        render(
            <AnularAbonoModal
                documentoId="factura-1"
                pagoId="pago-1"
                montoAplicado={4500}
                monedaCodigo="CRC"
                consecutivo="FAC-0001"
                onClose={vi.fn()}
            />,
        );

        await user.type(
            screen.getByPlaceholderText("Anular Abono"),
            "Anular Abono",
        );
        await user.type(
            screen.getByPlaceholderText("Indica el motivo por el que se anula este abono"),
            "no",
        );
        await user.click(screen.getByRole("button", { name: "Anular abono" }));

        expect(screen.getByText("El motivo debe tener al menos 3 caracteres.")).toBeInTheDocument();
        expect(anularAbonoFacturaActionMock).not.toHaveBeenCalled();

        await user.clear(
            screen.getByPlaceholderText("Indica el motivo por el que se anula este abono"),
        );
        await user.type(
            screen.getByPlaceholderText("Indica el motivo por el que se anula este abono"),
            "Cliente reportó transferencia duplicada",
        );
        await user.click(screen.getByRole("button", { name: "Anular abono" }));

        expect(anularAbonoFacturaActionMock).toHaveBeenCalledWith(
            "factura-1",
            "pago-1",
            "Cliente reportó transferencia duplicada",
        );
        expect(await screen.findByText("Abono anulado")).toBeInTheDocument();
        expect(screen.getByRole("button", { name: "Imprimir ticket" })).toBeInTheDocument();
        expect(screen.getByRole("link", { name: "Ver PDF" })).toHaveAttribute(
            "href",
            "/pdf/ventas/factura-1/abonos/pago-1",
        );
    });
});
