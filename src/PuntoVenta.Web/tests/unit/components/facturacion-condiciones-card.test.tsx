import { FacturacionCondicionesCard } from "@/app/(root)/emision/facturacion/FacturacionCondicionesCard";
import { VENTA_FIELDS } from "@lib/constants/ventas.constants";
import { fireEvent, render, screen } from "../../utils/render";
import { describe, expect, it, vi } from "vitest";

vi.mock("@lib/hooks/useCondicionesVentaActivasQuery", () => ({
    useCondicionesVentaActivasQuery: () => ({
        data: [{ codigo: "01", detalle: "Contado" }],
        isLoading: false,
    }),
}));

describe("FacturacionCondicionesCard", () => {
    it("notifica el cambio de moneda", () => {
        const onFieldChange = vi.fn();

        render(
            <FacturacionCondicionesCard
                values={{
                    [VENTA_FIELDS.CONDICION_VENTA_CODIGO]: "01",
                    [VENTA_FIELDS.MONEDA_CODIGO]: "CRC",
                    [VENTA_FIELDS.TIPO_CAMBIO]: 500,
                }}
                errors={{}}
                disabled={false}
                clienteSeleccionado={null}
                aplicaVendedores={false}
                vendedores={[]}
                aplicaCajas={false}
                cajas={[]}
                onClienteChange={vi.fn()}
                onClienteSeleccionadoChange={vi.fn()}
                onFieldChange={onFieldChange}
            />,
        );

        fireEvent.click(screen.getByText("Dolares"));

        expect(onFieldChange).toHaveBeenCalledWith(
            VENTA_FIELDS.MONEDA_CODIGO,
            "USD",
        );
    });
});
