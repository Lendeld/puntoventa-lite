import type { ApiValidationErrors } from "@lib/types/base.types";
import { resolveApiErrorMessage } from "@lib/utils/apiErrors";

export function resolveLoginErrorMessage(
    status: number,
    errors?: ApiValidationErrors,
): string {
    if (status === 401) {
        return resolveApiErrorMessage(errors, {
            preferredKeys: ["Auth_CredencialesInvalidas"],
            fallback: "Credenciales inválidas.",
        });
    }

    if (status === 403) {
        return resolveApiErrorMessage(errors, {
            preferredKeys: ["Auth_UsuarioInactivo", "Auth_SinRolAsignado"],
            fallback: "No tienes permiso para acceder.",
        });
    }

    return resolveApiErrorMessage(errors);
}
