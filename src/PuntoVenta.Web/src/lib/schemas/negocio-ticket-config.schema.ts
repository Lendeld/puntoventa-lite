import { z } from "zod";
import {
    CONFIG_NOMBRE_MAX,
    ELEMENTO_ENCABEZADO_TEXTO_MAX,
    ELEMENTO_ENCABEZADO_TIPOS_FIJOS,
    LINEA_PIE_TEXTO_MAX,
    MAX_CONFIGS_POR_DESTINO,
    MAX_ELEMENTOS_TEXTO_ENCABEZADO,
    MAX_LINEAS,
    NEGOCIO_TICKET_CONFIG_FIELDS,
} from "@lib/constants/negocio-ticket-config.constants";

const MENSAJE_PIE_MAX = 240;

const elementoEncabezadoSchema = z
    .object({
        _key: z.string(),
        tipo: z.enum([
            "Nombre",
            "NombreComercial",
            "Correo",
            "Telefono",
            "Direccion",
            "IdentificacionFiscal",
            "Fecha",
            "Texto",
        ]),
        visible: z.boolean(),
        textoLibre: z.string().nullable(),
    })
    .superRefine((el, ctx) => {
        if (el.tipo !== "Texto") return;
        const texto = el.textoLibre?.trim() ?? "";
        if (texto.length === 0) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                path: ["textoLibre"],
                message: "El texto del elemento es requerido.",
            });
        } else if (texto.length > ELEMENTO_ENCABEZADO_TEXTO_MAX) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                path: ["textoLibre"],
                message: `El texto no puede exceder ${ELEMENTO_ENCABEZADO_TEXTO_MAX} caracteres.`,
            });
        }
    });

const lineaPieSchema = z.object({
    // Identificador estable solo de cliente (key de React en la lista).
    // No se envía al backend; el action mapea únicamente los campos de negocio.
    _key: z.string(),
    texto: z
        .string()
        .trim()
        .min(1, "El texto de la línea es requerido.")
        .max(
            LINEA_PIE_TEXTO_MAX,
            `El texto no puede exceder ${LINEA_PIE_TEXTO_MAX} caracteres.`,
        ),
    alineacion: z.enum(["Izquierda", "Centro", "Derecha"]),
    negrita: z.boolean(),
});

const configuracionPieSchema = z.object({
    _key: z.string(),
    nombre: z
        .string()
        .trim()
        .min(1, "El nombre de la configuración es requerido.")
        .max(
            CONFIG_NOMBRE_MAX,
            `El nombre no puede exceder ${CONFIG_NOMBRE_MAX} caracteres.`,
        ),
    destino: z.enum(["Pdf", "Ticket"]),
    // Lista vacía = aplica a todos los documentos.
    tiposDocumento: z.array(
        z.enum(["Factura", "Apartado", "NotaCredito", "NotaDebito", "Proforma"]),
    ),
    lineas: z
        .array(lineaPieSchema)
        .max(
            MAX_LINEAS,
            `Una configuración no puede tener más de ${MAX_LINEAS} líneas.`,
        ),
});

export const actualizarNegocioTicketConfigSchema = z.object({
    [NEGOCIO_TICKET_CONFIG_FIELDS.MENSAJE_PIE]: z
        .string()
        .trim()
        .max(
            MENSAJE_PIE_MAX,
            `El mensaje de pie no puede exceder ${MENSAJE_PIE_MAX} caracteres.`,
        )
        .or(z.literal("")),
    [NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_LOGO]: z.boolean(),
    [NEGOCIO_TICKET_CONFIG_FIELDS.APLICA_COPIA_CLIENTE_NEGOCIO]: z.boolean(),
    [NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_CODIGO_BARRAS]: z.boolean(),
    [NEGOCIO_TICKET_CONFIG_FIELDS.CONFIGURACIONES]: z
        .array(configuracionPieSchema)
        // Backstop de las reglas de negocio (la UI ya las previene): por destino
        // un tipo no se repite, "Todos" (sin tipos) es exclusivo, y máximo por destino.
        .superRefine((configs, ctx) => {
            const customCode = z.ZodIssueCode.custom;
            const destinos = ["Pdf", "Ticket"] as const;
            for (const destino of destinos) {
                const delDestino = configs.filter((c) => c.destino === destino);

                if (delDestino.length > MAX_CONFIGS_POR_DESTINO) {
                    ctx.addIssue({
                        code: customCode,
                        message: `No se pueden configurar más de ${MAX_CONFIGS_POR_DESTINO} configuraciones por destino.`,
                    });
                }

                const hayTodos = delDestino.some(
                    (c) => c.tiposDocumento.length === 0,
                );
                if (hayTodos && delDestino.length > 1) {
                    ctx.addIssue({
                        code: customCode,
                        message:
                            "Una configuración para todos los documentos debe ser la única de su destino.",
                    });
                }

                const vistos = new Set<string>();
                for (const c of delDestino) {
                    for (const tipo of c.tiposDocumento) {
                        if (vistos.has(tipo)) {
                            ctx.addIssue({
                                code: customCode,
                                message:
                                    "Un tipo de documento no puede estar en más de una configuración del mismo destino.",
                            });
                        }
                        vistos.add(tipo);
                    }
                }
            }
        }),
    [NEGOCIO_TICKET_CONFIG_FIELDS.ELEMENTOS_ENCABEZADO]: z
        .array(elementoEncabezadoSchema)
        // Backstop: los tipos fijos están todos y sin repetir; máximo de textos libres.
        .superRefine((elementos, ctx) => {
            const fijos: string[] = [];
            let textos = 0;
            for (const e of elementos) {
                if (e.tipo === "Texto") textos++;
                else fijos.push(e.tipo);
            }
            const set = new Set(fijos);
            if (set.size !== fijos.length) {
                ctx.addIssue({
                    code: z.ZodIssueCode.custom,
                    message: "Un elemento fijo del encabezado no puede repetirse.",
                });
            }
            if (ELEMENTO_ENCABEZADO_TIPOS_FIJOS.some((t) => !set.has(t))) {
                ctx.addIssue({
                    code: z.ZodIssueCode.custom,
                    message:
                        "El encabezado debe incluir todos los elementos fijos.",
                });
            }
            if (textos > MAX_ELEMENTOS_TEXTO_ENCABEZADO) {
                ctx.addIssue({
                    code: z.ZodIssueCode.custom,
                    message: `No se pueden agregar más de ${MAX_ELEMENTOS_TEXTO_ENCABEZADO} elementos de texto.`,
                });
            }
        }),
});
