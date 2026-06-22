export const BROADCAST_CHANNEL_NAME = "pv:auth";

export const LOGIN_FIELDS = {
    NOMBRE_USUARIO: "Usuario_NombreUsuario",
    PASSWORD: "Usuario_Password",
} as const;

export const OTP_FIELDS = {
    CODIGO: "Auth_CodigoOtp",
} as const;

export const CAMBIAR_PASSWORD_FIELDS = {
    PASSWORD_ACTUAL: "Usuario_PasswordActual",
    PASSWORD_NUEVA: "Usuario_PasswordNueva",
    CONFIRMAR_PASSWORD_NUEVA: "Usuario_ConfirmarPasswordNueva",
} as const;

export const ESTABLECER_PIN_FIELDS = {
    PASSWORD_ACTUAL: "Usuario_PasswordActual",
    PIN_NUEVO: "Usuario_Pin",
    CONFIRMAR_PIN_NUEVO: "Usuario_ConfirmarPin",
} as const;
