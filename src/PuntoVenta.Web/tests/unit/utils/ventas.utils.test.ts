import { describe, expect, it } from "vitest";
import { calcularTotalesFactura, ESTADO_DOCUMENTO, puedeEmitirFactura, recalcularPagosDocumento } from "@lib/utils/ventas.utils";

describe("ventas.utils", () => {
    it("calcula a 5 dec internamente y redondea a 2 dec en el borde (display/cobro)", () => {
        // Línea USD 11.50 @13% → IVA 1.495 (5 dec). El subtotal/impuesto/total
        // que se MUESTRAN se redondean a 2 dec (impuesto 1.50, total 13.00),
        // igual que el total a pagar; el backend almacena 12.995 (5 dec).
        const linea = {
            ProductoId: "prod-1",
            TipoItem: "Bien" as const,
            Codigo: "P-001",
            Descripcion: "Producto",
            Cantidad: 1,
            PrecioUnitario: 11.5,
            MontoDescuento: 0,
            TarifaIvaImpuestoCodigo: "08",
            PorcentajeImpuesto: 13,
        };
        const totales = calcularTotalesFactura([linea], []);

        expect(totales.impuesto).toBe(1.5);
        expect(totales.total).toBe(13);

        const pagos = recalcularPagosDocumento(
            [
                {
                    MonedaCodigo: "USD",
                    TipoCambioAplicado: 1,
                    MedioPagoCodigo: "01",
                    MontoEntregado: totales.total,
                    MontoAplicadoMonedaPago: 0,
                    MontoAplicadoDocumento: 0,
                    MontoVueltoMonedaPago: 0,
                    MontoVueltoDocumento: 0,
                    Referencia: "",
                    Observacion: "",
                },
            ],
            "USD",
            500,
            totales.total,
        );

        expect(pagos[0].MontoAplicadoDocumento).toBe(13);
        expect(calcularTotalesFactura([linea], pagos).saldo).toBe(0);
    });

    it("precio-con-IVA redondo: 3 × 5752.21239 @13% da total exacto 19 500", () => {
        // Espejo del backend a 5 dec: 17 256.63717 + 2 243.36283 = 19 500.00000.
        // El céntimo que antes se perdía (19 499.99) ya no.
        const linea = {
            ProductoId: "prod-1",
            TipoItem: "Bien" as const,
            Codigo: "P-001",
            Descripcion: "Producto",
            Cantidad: 3,
            PrecioUnitario: 5752.21239,
            MontoDescuento: 0,
            TarifaIvaImpuestoCodigo: "08",
            PorcentajeImpuesto: 13,
        };
        const totales = calcularTotalesFactura([linea], []);

        expect(totales.total).toBe(19500);
    });

    it("recalcula totales al editar cantidad o descuento", () => {
        const base = calcularTotalesFactura([
            {
                ProductoId: "prod-1",
                TipoItem: "Bien",
                Codigo: "P-001",
                Descripcion: "Producto",
                Cantidad: 1,
                PrecioUnitario: 1000,
                MontoDescuento: 0,
                TarifaIvaImpuestoCodigo: "08",
                PorcentajeImpuesto: 13,
            },
        ], []);

        const editado = calcularTotalesFactura([
            {
                ProductoId: "prod-1",
                TipoItem: "Bien",
                Codigo: "P-001",
                Descripcion: "Producto",
                Cantidad: 2,
                PrecioUnitario: 1000,
                MontoDescuento: 100,
                TarifaIvaImpuestoCodigo: "08",
                PorcentajeImpuesto: 13,
            },
        ], []);

        expect(base.total).toBe(1130);
        expect(editado.subtotal).toBe(1900);
        expect(editado.impuesto).toBe(247);
        expect(editado.total).toBe(2147);
    });

    it("permite emitir contado solo con borrador válido y saldo en cero", () => {
        const canEmit = puedeEmitirFactura({
            estado: ESTADO_DOCUMENTO.BORRADOR,
            condicionVentaCodigo: "01",
            plazoCreditoDias: null,
            valid: true,
            totales: {
                subtotal: 1000,
                descuentos: 0,
                impuesto: 130,
                total: 1130,
                pagado: 1130,
                saldo: 0,
            },
        });

        expect(canEmit).toBe(true);
    });

    it("bloquea emitir crédito sin plazo", () => {
        const canEmit = puedeEmitirFactura({
            estado: ESTADO_DOCUMENTO.BORRADOR,
            condicionVentaCodigo: "02",
            plazoCreditoDias: null,
            valid: true,
            totales: {
                subtotal: 1000,
                descuentos: 0,
                impuesto: 0,
                total: 1000,
                pagado: 200,
                saldo: 800,
            },
        });

        expect(canEmit).toBe(false);
    });

    it("bloquea emitir si el documento ya fue emitido", () => {
        const canEmit = puedeEmitirFactura({
            estado: ESTADO_DOCUMENTO.EMITIDO,
            condicionVentaCodigo: "01",
            plazoCreditoDias: null,
            valid: true,
            totales: {
                subtotal: 1000,
                descuentos: 0,
                impuesto: 130,
                total: 1130,
                pagado: 1130,
                saldo: 0,
            },
        });

        expect(canEmit).toBe(false);
    });

    it("calcula pagado usando el monto aplicado a la factura", () => {
        const totales = calcularTotalesFactura([
            {
                ProductoId: "prod-1",
                TipoItem: "Bien",
                Codigo: "P-001",
                Descripcion: "Producto",
                Cantidad: 1,
                PrecioUnitario: 1000,
                MontoDescuento: 0,
                TarifaIvaImpuestoCodigo: "08",
                PorcentajeImpuesto: 13,
            },
        ], [
            {
                MonedaCodigo: "USD",
                TipoCambioAplicado: 540.12345,
                MedioPagoCodigo: "01",
                MontoEntregado: 3,
                MontoAplicadoMonedaPago: 2.0921,
                MontoAplicadoDocumento: 1130,
                MontoVueltoMonedaPago: 0.9079,
                MontoVueltoDocumento: 490.87822,
                Referencia: "",
                Observacion: "",
            },
        ]);

        expect(totales.pagado).toBe(1130);
        expect(totales.saldo).toBe(0);
    });
});
