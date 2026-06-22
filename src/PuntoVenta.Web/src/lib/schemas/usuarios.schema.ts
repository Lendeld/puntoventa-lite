import { z } from "zod";
import { USUARIO_FIELDS } from "@lib/constants/usuarios.constants";

export const crearUsuarioSchema = z.object({
    [USUARIO_FIELDS.NOMBRE_USUARIO]: z
        .string()
        .min(1, "El nombre de usuario es requerido.")
        .max(50, "Máximo 50 caracteres."),
    [USUARIO_FIELDS.NOMBRE]: z
        .string()
        .min(1, "El nombre es requerido.")
        .max(150, "Máximo 150 caracteres."),
    [USUARIO_FIELDS.IDENTIFICACION]: z
        .string()
        .max(50, "Máximo 50 caracteres.")
        .optional()
        .default(""),
    [USUARIO_FIELDS.PASSWORD]: z
        .string()
        .min(1, "La contraseña temporal es requerida.")
        .min(8, "La contraseña debe tener al menos 8 caracteres.")
        .max(100, "Máximo 100 caracteres.")
        .regex(/[A-Z]/, "La contraseña debe contener al menos una letra mayúscula.")
        .regex(/[a-z]/, "La contraseña debe contener al menos una letra minúscula.")
        .regex(/[0-9]/, "La contraseña debe contener al menos un número.")
        .regex(/[$&+,:;=?@#|'<>.^*()%!-]/, "La contraseña debe contener al menos un símbolo especial."),
    [USUARIO_FIELDS.ROL_ID]: z.preprocess(
        (value) => value ?? "",
        z.string().uuid("El rol seleccionado no es válido.").or(z.literal("")),
    ),
    [USUARIO_FIELDS.CORREO]: z.preprocess(
        (value) => value ?? "",
        z
            .string()
            .max(256, "Máximo 256 caracteres.")
            .email("El correo no es válido.")
            .or(z.literal("")),
    ),
    [USUARIO_FIELDS.TELEFONO]: z
        .string()
        .max(20, "Máximo 20 caracteres.")
        .optional()
        .default(""),
});

export const actualizarUsuarioSchema = z.object({
    [USUARIO_FIELDS.ROL_ID]: z.preprocess(
        (value) => value ?? "",
        z
            .string()
            .min(1, "El rol es requerido.")
            .uuid("El rol seleccionado no es válido."),
    ),
    [USUARIO_FIELDS.ACTIVO]: z.boolean(),
});
