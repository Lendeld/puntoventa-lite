import { FacturacionLineaMobileCard } from "@/app/(root)/emision/facturacion/FacturacionLineaEditor";
import type { DocumentoVentaLineaForm } from "@lib/types/ventas.types";
import { fireEvent, render, screen } from "../../utils/render";
import { describe, expect, it, vi } from "vitest";

const linea: DocumentoVentaLineaForm = {
    Id: "linea-1",
    ProductoId: "producto-1",
    TipoItem: "Bien",
    Codigo: "P-001",
    Descripcion: "Producto de prueba",
    Cantidad: 2,
    PrecioUnitario: 500,
    MontoDescuento: 100,
    TarifaIvaImpuestoCodigo: "08",
    PorcentajeImpuesto: 13,
    PermiteModificarPrecioUnitario: true,
};

describe("FacturacionLineaMobileCard", () => {
    it("muestra el total calculado y permite eliminar la linea", () => {
        const onRemoveLinea = vi.fn();

        render(
            <FacturacionLineaMobileCard
                linea={linea}
                index={0}
                monedaCodigo="CRC"
                disabled={false}
                onUpdateLinea={vi.fn()}
                onRemoveLinea={onRemoveLinea}
                getFieldError={() => null}
            />,
        );

        expect(screen.getByText("Producto de prueba")).toBeInTheDocument();
        expect(screen.getByText("₡ 1,017.00")).toBeInTheDocument();

        fireEvent.click(screen.getByRole("button", { name: /eliminar línea 1/i }));

        expect(onRemoveLinea).toHaveBeenCalledWith(0);
    });
});
