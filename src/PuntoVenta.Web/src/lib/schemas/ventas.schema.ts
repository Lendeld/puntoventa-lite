import { z } from "zod";
import { CONDICION_VENTA_CONTADO, CONDICIONES_VENTA_CREDITO, MONEDA_DEFAULT, VENTA_FIELDS } from "@lib/constants/ventas.constants";
import { calcularTotalesFactura } from "@lib/utils/ventas.utils";

const MONEDA_MAX = 3;
const OBSERVACIONES_MAX = 500;
const REFERENCIA_MAX = 100;
const OBSERVACION_PAGO_MAX = 250;
const MONEDA_PAGO_MAX = 3;

const lineaSchema = z.object({
    ProductoId: z.string().uuid("Selecciona un producto válido."),
    TipoItem: z.enum(["Bien", "Servicio"]),
    Codigo: z.string().trim().min(1),
    Descripcion: z.string().trim().min(1),
    Cantidad: z.number().positive("La cantidad debe ser mayor a 0."),
    PrecioUnitario: z.number().positive("El precio unitario debe ser mayor a 0."),
    MontoDescuento: z.number().min(0, "El descuento no puede ser negativo."),
    TarifaIvaImpuestoCodigo: z.string().nullable(),
    PorcentajeImpuesto: z.number().min(0),
}).superRefine((linea, ctx) => {
    const montoBruto = linea.Cantidad * linea.PrecioUnitario;

    if (linea.MontoDescuento > montoBruto) {
        ctx.addIssue({
            code: "custom",
            path: ["MontoDescuento"],
            message: "El descuento no puede ser mayor al subtotal de la línea.",
        });
    }
});

const pagoSchema = z.object({
    MonedaCodigo: z.string().trim().min(1, "Selecciona una moneda.").max(MONEDA_PAGO_MAX),
    TipoCambioAplicado: z.number().positive("El tipo de cambio aplicado debe ser mayor a 0."),
    MedioPagoCodigo: z.string().trim().min(1, "Selecciona un medio de pago."),
    MontoEntregado: z.number().positive("El monto entregado debe ser mayor a 0."),
    MontoAplicadoMonedaPago: z.number().positive("El monto aplicado debe ser mayor a 0."),
    MontoAplicadoDocumento: z.number().positive("El monto aplicado debe ser mayor a 0."),
    MontoVueltoMonedaPago: z.number().min(0, "El vuelto no puede ser negativo."),
    MontoVueltoDocumento: z.number().min(0, "El vuelto no puede ser negativo."),
    Referencia: z.string().trim().max(REFERENCIA_MAX, `La referencia no puede exceder ${REFERENCIA_MAX} caracteres.`),
    Observacion: z.string().trim().max(OBSERVACION_PAGO_MAX, `La observación no puede exceder ${OBSERVACION_PAGO_MAX} caracteres.`),
}).superRefine((pago, ctx) => {
    const entregado = Math.round(pago.MontoEntregado * 100000);
    const suma = Math.round((pago.MontoAplicadoMonedaPago + pago.MontoVueltoMonedaPago) * 100000);

    if (entregado !== suma) {
        ctx.addIssue({
            code: "custom",
            path: ["MontoEntregado"],
            message: "El monto entregado debe ser igual al monto aplicado más el vuelto.",
        });
    }
});

const facturaBaseSchema = z.object({
    [VENTA_FIELDS.CLIENTE_ID]: z.string(),
    [VENTA_FIELDS.VENDEDOR_ID]: z.string(),
    [VENTA_FIELDS.CAJA_ID]: z.string().optional().default(""),
    [VENTA_FIELDS.CONDICION_VENTA_CODIGO]: z
        .string()
        .trim()
        .min(1, "La condición de venta es requerida."),
    [VENTA_FIELDS.FECHA_DOCUMENTO]: z
        .string()
        .trim()
        .min(1, "La fecha del documento es requerida.")
        .refine((value) => !Number.isNaN(Date.parse(value)), "La fecha del documento es inválida."),
    [VENTA_FIELDS.FECHA_VENCIMIENTO]: z
        .string()
        .trim()
        .optional()
        .default("")
        .refine((value) => value === "" || !Number.isNaN(Date.parse(value)), "La fecha de vencimiento es inválida."),
    [VENTA_FIELDS.MONEDA_CODIGO]: z
        .string()
        .trim()
        .min(1, "La moneda es requerida.")
        .max(MONEDA_MAX, `La moneda no puede exceder ${MONEDA_MAX} caracteres.`)
        .default(MONEDA_DEFAULT),
    [VENTA_FIELDS.TIPO_CAMBIO]: z.number().positive("El tipo de cambio debe ser mayor a 0."),
    [VENTA_FIELDS.PLAZO_CREDITO_DIAS]: z.number().int().min(1, "El plazo de crédito debe ser mayor a 0.").nullable(),
    [VENTA_FIELDS.OBSERVACIONES]: z
        .string()
        .trim()
        .max(OBSERVACIONES_MAX, `Las observaciones no pueden exceder ${OBSERVACIONES_MAX} caracteres.`),
    [VENTA_FIELDS.LINEAS]: z.array(lineaSchema).min(1, "El documento debe tener al menos una línea."),
    [VENTA_FIELDS.PAGOS]: z.array(pagoSchema),
});

function validarCondicionCredito(values: z.infer<typeof facturaBaseSchema>, ctx: z.RefinementCtx) {
    const condicion = values[VENTA_FIELDS.CONDICION_VENTA_CODIGO];
    const plazoCredito = values[VENTA_FIELDS.PLAZO_CREDITO_DIAS];

    if ((CONDICIONES_VENTA_CREDITO as readonly string[]).includes(condicion)) {
        if (!plazoCredito || plazoCredito <= 0) {
            ctx.addIssue({
                code: "custom",
                path: [VENTA_FIELDS.PLAZO_CREDITO_DIAS],
                message: "El plazo de crédito debe ser mayor a 0.",
            });
        }
    }
}

export const crearBorradorFacturaSchema = facturaBaseSchema.superRefine((values, ctx) => {
    validarCondicionCredito(values, ctx);
});

export const crearFacturaSchema = facturaBaseSchema.superRefine((values, ctx) => {
    validarCondicionCredito(values, ctx);

    const condicion = values[VENTA_FIELDS.CONDICION_VENTA_CODIGO];
    const pagos = values[VENTA_FIELDS.PAGOS];
    const totales = calcularTotalesFactura(values[VENTA_FIELDS.LINEAS], pagos);

    if (condicion === CONDICION_VENTA_CONTADO) {
        if (totales.total === 0) {
            return;
        }

        if (pagos.length === 0) {
            ctx.addIssue({
                code: "custom",
                path: [VENTA_FIELDS.PAGOS],
                message: "El documento requiere al menos un medio de pago.",
            });
        } else if (totales.saldo !== 0) {
            ctx.addIssue({
                code: "custom",
                path: [VENTA_FIELDS.PAGOS],
                message: "La suma de los medios de pago debe coincidir con el total de la factura.",
            });
        }
    }

    if (pagos.length > 4) {
        ctx.addIssue({
            code: "custom",
            path: [VENTA_FIELDS.PAGOS],
            message: "La factura no puede tener más de 4 medios de pago.",
        });
    }
});

export const crearApartadoSchema = facturaBaseSchema.superRefine((values, ctx) => {
    const fechaDocumento = values[VENTA_FIELDS.FECHA_DOCUMENTO];
    const fechaVencimiento = values[VENTA_FIELDS.FECHA_VENCIMIENTO];
    validarCondicionCredito(values, ctx);

    if (!fechaVencimiento) {
        ctx.addIssue({
            code: "custom",
            path: [VENTA_FIELDS.FECHA_VENCIMIENTO],
            message: "La fecha de vencimiento del apartado es requerida.",
        });
        return;
    }

    if (dayStart(fechaVencimiento) < dayStart(fechaDocumento)) {
        ctx.addIssue({
            code: "custom",
            path: [VENTA_FIELDS.FECHA_VENCIMIENTO],
            message: "La fecha de vencimiento debe ser igual o posterior a la fecha del documento.",
        });
    }

    if (dayStart(fechaVencimiento) < dayStart(new Date().toISOString())) {
        ctx.addIssue({
            code: "custom",
            path: [VENTA_FIELDS.FECHA_VENCIMIENTO],
            message: "La fecha de vencimiento no puede ser anterior a hoy.",
        });
    }

    const totales = calcularTotalesFactura(values[VENTA_FIELDS.LINEAS], values[VENTA_FIELDS.PAGOS]);
    if (totales.pagado > totales.total) {
        ctx.addIssue({
            code: "custom",
            path: [VENTA_FIELDS.PAGOS],
            message: "Los pagos no pueden superar el total del apartado.",
        });
    }
});

function dayStart(value: string) {
    const date = new Date(value);
    date.setHours(0, 0, 0, 0);
    return date.getTime();
}

export const abonarApartadoSchema = z.object({
    fechaPago: z
        .string()
        .trim()
        .min(1, "La fecha de pago es requerida.")
        .refine((value) => !Number.isNaN(Date.parse(value)), "La fecha de pago es inválida.")
        .refine((value) => new Date(value).getTime() <= Date.now(), "La fecha de pago no puede ser futura."),
    medioPagoCodigo: z.string().trim().min(1, "El método de pago es requerido."),
    monto: z.number().positive("El monto debe ser mayor a 0."),
    referencia: z
        .string()
        .trim()
        .max(REFERENCIA_MAX, `La referencia no puede exceder ${REFERENCIA_MAX} caracteres.`)
        .optional()
        .default(""),
    observacion: z
        .string()
        .trim()
        .max(OBSERVACION_PAGO_MAX, `La observación no puede exceder ${OBSERVACION_PAGO_MAX} caracteres.`)
        .optional()
        .default(""),
});

export const extenderVencimientoApartadoSchema = z.object({
    fechaVencimiento: z
        .string()
        .trim()
        .min(1, "La fecha de vencimiento es requerida.")
        .refine((value) => !Number.isNaN(Date.parse(value)), "La fecha de vencimiento es inválida.")
        .refine((value) => dayStart(value) >= dayStart(new Date().toISOString()), "La fecha de vencimiento no puede ser pasada."),
});
