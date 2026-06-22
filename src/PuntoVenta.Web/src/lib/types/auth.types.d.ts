import type { FormValues } from "@lib/types/base.types";
import type {
    actualizarPerfilUsuarioActualSchema,
    cambiarPasswordSchema,
    establecerPinSchema,
    loginSchema,
} from "@/lib/schemas/auth.schema";

export type LoginFormValues = FormValues<typeof loginSchema>;
export type CambiarPasswordFormValues = FormValues<typeof cambiarPasswordSchema>;
export type EstablecerPinFormValues = FormValues<typeof establecerPinSchema>;
export type ActualizarPerfilUsuarioActualFormValues =
    FormValues<typeof actualizarPerfilUsuarioActualSchema>;

export interface ActualizarPerfilUsuarioActualDto {
    nombre: string;
    identificacion: string;
    correo?: string | null;
    telefono?: string | null;
}

export interface PaginaMenuDto {
    nombre: string;
    ruta: string;
    icono?: string;
}

export interface AuthTokensDto {
    accessToken: string;
    accessTokenExpiracionUtc: string;
    refreshToken: string;
    refreshTokenExpiracionUtc: string;
}

export interface AuthFlowResponse extends Partial<AuthTokensDto> {
    requiresPasswordChange: boolean;
}

export type DeploymentMode = "Cloud" | "LocalHost";

export interface UsuarioActualDto {
    usuario: string;
    nombre: string;
    identificacion: string;
    correo: string | null;
    telefono: string | null;
    debeCambiarPassword: boolean;
    deploymentMode: DeploymentMode;
    tienePin: boolean;
}
