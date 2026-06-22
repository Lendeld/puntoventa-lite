import DetalleVentaPageSection from "@/app/(root)/emision/ventas/[id]/DetalleVentaPageSection";
import type { DocumentoVentaDto } from "@lib/types/ventas.types";
import userEvent from "@testing-library/user-event";
import { render, screen } from "../../utils/render";
import { describe, expect, it, vi } from "vitest";

vi.mock("@lib/printing/venta-printing", () => ({
    getVentaPdfUrl: (documentoId: string) => `/api/ventas/${documentoId}/pdf`,
    getAbonoPdfUrl: (documentoId: string, pagoId: string) =>
        `/api/ventas/${documentoId}/abonos/${pagoId}/pdf`,
    printTicket: vi.fn(),
}));

vi.mock("@lib/hooks/useMediosPagoActivosQuery", () => ({
    useMediosPagoActivosQuery: () => ({ data: [] }),
}));

vi.mock("next/navigation", () => ({
    useRouter: () => ({
        push: vi.fn(),
        replace: vi.fn(),
        back: vi.fn(),
        forward: vi.fn(),
        refresh: vi.fn(),
        prefetch: vi.fn(),
    }),
    usePathname: () => "/emision/ventas/proforma-1",
    useSearchParams: () => new URLSearchParams(),
}));

const documento: DocumentoVentaDto = {
    id: "proforma-1",
    tipoDocumento: "Proforma",
    estado: "Borrador",
    clienteId: "cliente-1",
    clienteNombre: "Cliente Demo",
    clienteIdentificacion: "101010101",
    vendedorId: "vendedor-1",
    vendedorNombre: "Vendedor Demo",
    consecutivo: "PRO-0000000001",
    fechaDocumento: "2026-05-01T14:35:00Z",
    tipoDocumentoDetalle: "Proforma",
    tipoDocumentoColor: "violet",
    estadoDetalle: "Facturada",
    estadoColor: "green",
    condicionVentaCodigo: "01",
    condicionVentaDetalleSnapshot: "Contado",
    totalComprobante: 1130,
    totalPagado: 0,
    saldoPendiente: 0,
    montoRedondeo: 0,
    plazoCreditoDias: null,
    fechaVencimiento: null,
    monedaCodigo: "CRC",
    tipoCambio: 1,
    totalVenta: 1000,
    totalDescuentos: 0,
    totalImpuesto: 130,
    observaciones: "Observacion de venta",
    fechaCancelacion: null,
    esCredito: false,
    creadoPor: "Caja Principal",
    lineas: [
        {
            id: "linea-1",
            productoId: "producto-1",
            tipoItem: "Bien",
            codigo: "P-001",
            descripcion: "Producto Demo",
            unidadMedidaCodigo: "Unid",
            cantidad: 2,
            precioUnitario: 500,
            montoDescuento: 0,
            subtotal: 1000,
            montoImpuesto: 130,
            totalLinea: 1130,
            devuelveInventario: false,
            noAplicaExistencias: false,
            permiteModificarPrecioUnitario: false,
            cantidadDevueltaEnNotasCredito: 0,
            subtotalAcumuladoNotasCredito: 0,
        },
    ],
    pagos: [],
    referencias: [],
    documentoOrigen: null,
    documentosGenerados: [
        {
            id: "factura-1",
            tipoDocumento: "Factura",
            estado: "Emitido",
            consecutivo: "FAC-0000000001",
            fechaDocumento: "2026-05-01T15:00:00Z",
            tipoDocumentoDetalle: "Factura",
            tipoDocumentoColor: "blue",
            estadoDetalle: "Emitido",
            estadoColor: "green",
            totalComprobante: 1130,
            totalPagado: 1130,
            monedaCodigo: "CRC",
            montoNotasCreditoAplicadas: 0,
        },
    ],
};

describe("DetalleVentaPageSection", () => {
    it("renderiza encabezado, lineas, totales y factura generada", () => {
        render(<DetalleVentaPageSection documento={documento} />);

        expect(screen.getByRole("heading", { name: "PRO-0000000001" })).toBeInTheDocument();
        expect(screen.getByText("Cliente Demo")).toBeInTheDocument();
        expect(screen.getByText("Producto Demo")).toBeInTheDocument();
        expect(screen.getAllByText("₡ 1,130.00").length).toBeGreaterThan(0);
        expect(screen.getByText("Factura generada desde esta proforma")).toBeInTheDocument();
        expect(screen.getByText("FAC-0000000001")).toBeInTheDocument();
    });

    it("aplica restricciones de crédito y muestra pagos anulados", async () => {
        const user = userEvent.setup();
        const facturaCredito: DocumentoVentaDto = {
            ...documento,
            id: "factura-credito-1",
            tipoDocumento: "Factura",
            estado: "Emitido",
            tipoDocumentoDetalle: "Factura",
            consecutivo: "FAC-0000000123",
            condicionVentaCodigo: "02",
            condicionVentaDetalleSnapshot: "Crédito",
            totalComprobante: 15000,
            totalPagado: 5000,
            saldoPendiente: 10000,
            esCredito: true,
            pagos: [
                {
                    id: "pago-1",
                    numeroAbono: 1,
                    monedaCodigo: "CRC",
                    tipoCambioAplicado: 1,
                    medioPagoCodigo: "01",
                    medioPagoDetalleSnapshot: "Efectivo",
                    montoEntregado: 5000,
                    montoAplicadoMonedaPago: 5000,
                    montoAplicadoDocumento: 5000,
                    montoVueltoMonedaPago: 0,
                    montoVueltoDocumento: 0,
                    fechaPago: "2026-06-21T12:00:00Z",
                    fechaRegistroUtc: "2026-06-21T12:05:00Z",
                    usuarioRegistroId: "user-1",
                    usuarioRegistroNombre: "Caja Demo",
                    referencia: null,
                    observacion: null,
                    anulado: false,
                    fechaAnulacionUtc: null,
                    usuarioAnulaId: null,
                    usuarioAnulaNombre: null,
                    motivoAnulacion: null,
                },
                {
                    id: "pago-2",
                    numeroAbono: 2,
                    monedaCodigo: "CRC",
                    tipoCambioAplicado: 1,
                    medioPagoCodigo: "02",
                    medioPagoDetalleSnapshot: "Transferencia",
                    montoEntregado: 2500,
                    montoAplicadoMonedaPago: 2500,
                    montoAplicadoDocumento: 2500,
                    montoVueltoMonedaPago: 0,
                    montoVueltoDocumento: 0,
                    fechaPago: "2026-06-22T12:00:00Z",
                    fechaRegistroUtc: "2026-06-22T12:05:00Z",
                    usuarioRegistroId: "user-2",
                    usuarioRegistroNombre: "Caja Demo",
                    referencia: "TRX-22",
                    observacion: null,
                    anulado: true,
                    fechaAnulacionUtc: "2026-06-22T14:00:00Z",
                    usuarioAnulaId: "admin-1",
                    usuarioAnulaNombre: "Admin",
                    motivoAnulacion: "Duplicado",
                },
            ],
            documentosGenerados: [],
        };

        render(
            <DetalleVentaPageSection
                documento={facturaCredito}
                permisos={{
                    emitirNotaCredito: true,
                    emitirNotaDebito: true,
                    abonarCredito: true,
                    anularAbono: true,
                    abonar: false,
                    extender: false,
                    convertir: false,
                    cancelar: false,
                }}
            />,
        );

        expect(screen.getByRole("button", { name: "Abonar" })).toBeInTheDocument();
        expect(screen.queryByRole("button", { name: "Emitir NC" })).not.toBeInTheDocument();
        expect(screen.queryByRole("button", { name: "Emitir ND" })).not.toBeInTheDocument();
        expect(screen.getByText("NC bloqueada: abonos activos")).toBeInTheDocument();
        expect(screen.getByText("ND no aplica a crédito")).toBeInTheDocument();
        const acciones = screen.getAllByRole("button", { name: "Acciones" });
        await user.click(acciones[1]);
        expect(screen.getByRole("menuitem", { name: "Anular abono" })).toBeInTheDocument();
        expect(screen.getByText("Abono #1")).toBeInTheDocument();
        expect(screen.getByText("Abono #2")).toBeInTheDocument();
        expect(screen.getByText("Anulado")).toBeInTheDocument();
        expect(screen.getByText("Motivo: Duplicado")).toBeInTheDocument();
    });
});
