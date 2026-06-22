import { z } from "zod";
import {
    CAMBIAR_PASSWORD_FIELDS,
    ESTABLECER_PIN_FIELDS,
    LOGIN_FIELDS,
} from "@/lib/constants/auth.constants";
import { USUARIO_FIELDS } from "@lib/constants/usuarios.constants";

const NOMBRE_USUARIO_MAX = 50;
const NOMBRE_MAX = 150;
const IDENTIFICACION_MAX = 50;
const CORREO_MAX = 256;
const TELEFONO_MAX = 20;

export const loginSchema = z.object({
    [LOGIN_FIELDS.NOMBRE_USUARIO]: z
        .string()
        .min(1, "El usuario es requerido")
        .max(
            NOMBRE_USUARIO_MAX,
            `El usuario no puede superar ${NOMBRE_USUARIO_MAX} caracteres`,
        ),
    [LOGIN_FIELDS.PASSWORD]: z.string().min(1, "La contraseña es requerida"),
});

export const cambiarPasswordSchema = z
    .object({
        [CAMBIAR_PASSWORD_FIELDS.PASSWORD_ACTUAL]: z
            .string()
            .min(1, "La contraseña actual es requerida."),
        [CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA]: z
            .string()
            .min(1, "La nueva contraseña es requerida.")
            .min(8, "La contraseña debe tener al menos 8 caracteres.")
            .regex(/[A-Z]/, "La contraseña debe contener al menos una letra mayúscula.")
            .regex(/[a-z]/, "La contraseña debe contener al menos una letra minúscula.")
            .regex(/[0-9]/, "La contraseña debe contener al menos un número.")
            .regex(/[$&+,:;=?@#|'<>.^*()%!-]/, "La contraseña debe contener al menos un símbolo especial."),
        [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: z
            .string()
            .min(1, "La confirmación de contraseña es requerida."),
    })
    .refine(
        (values) =>
            values[CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA] ===
            values[CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA],
        {
            path: [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA],
            message: "Las contraseñas no coinciden.",
        },
    );

export const establecerPinSchema = z
    .object({
        [ESTABLECER_PIN_FIELDS.PASSWORD_ACTUAL]: z
            .string()
            .min(1, "La contraseña actual es requerida."),
        [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: z
            .string()
            .regex(/^\d{6}$/, "El PIN debe ser exactamente 6 dígitos numéricos."),
        [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: z
            .string()
            .min(1, "La confirmación del PIN es requerida."),
    })
    .refine(
        (values) =>
            values[ESTABLECER_PIN_FIELDS.PIN_NUEVO] ===
            values[ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO],
        {
            path: [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO],
            message: "Los PINs no coinciden.",
        },
    );

export const actualizarPerfilUsuarioActualSchema = z.object({
    [USUARIO_FIELDS.NOMBRE]: z
        .string()
        .trim()
        .min(1, "El nombre es requerido.")
        .max(NOMBRE_MAX, `El nombre no puede exceder ${NOMBRE_MAX} caracteres.`),
    [USUARIO_FIELDS.IDENTIFICACION]: z
        .string()
        .trim()
        .max(
            IDENTIFICACION_MAX,
            `La identificación no puede exceder ${IDENTIFICACION_MAX} caracteres.`,
        )
        .optional()
        .default(""),
    [USUARIO_FIELDS.CORREO]: z
        .string()
        .trim()
        .email("El correo no es válido.")
        .max(CORREO_MAX, `El correo no puede exceder ${CORREO_MAX} caracteres.`)
        .or(z.literal(""))
        .optional(),
    [USUARIO_FIELDS.TELEFONO]: z
        .string()
        .trim()
        .max(TELEFONO_MAX, `El teléfono no puede exceder ${TELEFONO_MAX} caracteres.`)
        .optional(),
});
