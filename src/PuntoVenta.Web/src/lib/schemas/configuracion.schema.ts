import { z } from "zod";
import { NEGOCIO_FIELDS } from "@lib/constants/configuracion.constants";

const NOMBRE_MAX = 100;
const NOMBRE_COMERCIAL_MAX = 80;
const DIRECCION_MAX = 255;
const IDENTIFICACION_MAX = 20;
const CORREO_MAX = 160;
const TELEFONO_MAX = 20;

export const actualizarNegocioSchema = z.object({
    [NEGOCIO_FIELDS.NOMBRE]: z
        .string()
        .trim()
        .min(1, "El nombre del negocio es requerido.")
        .max(NOMBRE_MAX, `El nombre no puede exceder ${NOMBRE_MAX} caracteres.`),
    [NEGOCIO_FIELDS.NOMBRE_COMERCIAL]: z
        .string()
        .trim()
        .max(
            NOMBRE_COMERCIAL_MAX,
            `El nombre comercial no puede exceder ${NOMBRE_COMERCIAL_MAX} caracteres.`,
        )
        .or(z.literal("")),
    [NEGOCIO_FIELDS.DIRECCION]: z
        .string()
        .trim()
        .min(1, "La dirección es requerida.")
        .max(
            DIRECCION_MAX,
            `La dirección no puede exceder ${DIRECCION_MAX} caracteres.`,
        ),
    [NEGOCIO_FIELDS.TIPO_IDENTIFICACION_ID]: z
        .string()
        .trim()
        .max(2, "Selecciona un tipo de identificación válido.")
        .or(z.literal("")),
    [NEGOCIO_FIELDS.IDENTIFICACION]: z
        .string()
        .trim()
        .max(
            IDENTIFICACION_MAX,
            `La identificación no puede exceder ${IDENTIFICACION_MAX} caracteres.`,
        )
        .or(z.literal("")),
    [NEGOCIO_FIELDS.TELEFONO]: z
        .string()
        .trim()
        .max(
            TELEFONO_MAX,
            `El teléfono no puede exceder ${TELEFONO_MAX} caracteres.`,
        )
        .or(z.literal("")),
    [NEGOCIO_FIELDS.CORREO]: z
        .string()
        .trim()
        .max(
            CORREO_MAX,
            `El correo no puede exceder ${CORREO_MAX} caracteres.`,
        )
        .email("Ingresa un correo válido.")
        .or(z.literal("")),
    [NEGOCIO_FIELDS.APLICA_VENDEDORES]: z.boolean(),
    [NEGOCIO_FIELDS.APLICA_CAJAS]: z.boolean(),
    [NEGOCIO_FIELDS.TIPO_CAMBIO_PREDETERMINADO]: z
        .number({ message: "El tipo de cambio es requerido." })
        .positive("El tipo de cambio debe ser mayor a 0."),
}).superRefine((data, ctx) => {
    const tipo = data[NEGOCIO_FIELDS.TIPO_IDENTIFICACION_ID];
    const id = (data[NEGOCIO_FIELDS.IDENTIFICACION] ?? "").trim();

    if (!tipo || !id) return;

    const soloDigitos = /^\d+$/.test(id);

    const reglas: Record<string, { longitud: number | [number, number]; mensaje: string }> = {
        "01": { longitud: 9, mensaje: "Cédula física debe tener exactamente 9 dígitos." },
        "02": { longitud: 10, mensaje: "Cédula jurídica debe tener exactamente 10 dígitos." },
        "03": { longitud: [11, 12], mensaje: "DIMEX debe tener 11 o 12 dígitos." },
        "04": { longitud: 10, mensaje: "NITE debe tener exactamente 10 dígitos." },
    };

    const regla = reglas[tipo];
    if (!regla) return;

    const longitudValida = Array.isArray(regla.longitud)
        ? id.length >= regla.longitud[0] && id.length <= regla.longitud[1]
        : id.length === regla.longitud;

    if (!soloDigitos || !longitudValida) {
        ctx.addIssue({
            code: "custom",
            message: regla.mensaje,
            path: [NEGOCIO_FIELDS.IDENTIFICACION],
        });
    }
});
