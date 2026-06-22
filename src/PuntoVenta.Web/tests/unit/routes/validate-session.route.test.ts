import { beforeEach, describe, expect, it, vi } from "vitest";

const asegurarAccessTokenMock = vi.fn();
const validarTokenServiceMock = vi.fn();

vi.mock("@/lib/utils/apiClient", () => ({
    asegurarAccessToken: asegurarAccessTokenMock,
}));

vi.mock("@/lib/services/auth.service", () => ({
    validarTokenService: validarTokenServiceMock,
}));

describe("validate-session route", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("responde 401 sin destruir sesion cuando asegurarAccessToken retorna no-auth", async () => {
        asegurarAccessTokenMock.mockResolvedValue("no-auth");

        const { GET } = await import("@/app/api/auth/validate-session/route");
        const response = await GET();

        expect(validarTokenServiceMock).not.toHaveBeenCalled();
        expect(response.status).toBe(401);
        await expect(response.json()).resolves.toEqual({ valido: false });
    });

    it("responde 503 cuando asegurarAccessToken retorna transitorio", async () => {
        asegurarAccessTokenMock.mockResolvedValue("transitorio");

        const { GET } = await import("@/app/api/auth/validate-session/route");
        const response = await GET();

        expect(validarTokenServiceMock).not.toHaveBeenCalled();
        expect(response.status).toBe(503);
        await expect(response.json()).resolves.toEqual({ valido: false, transitorio: true });
    });

    it("responde 401 cuando el token ya no es valido en API (403 negocio suspendido)", async () => {
        asegurarAccessTokenMock.mockResolvedValue("ok");
        validarTokenServiceMock.mockResolvedValue({
            data: null,
            errors: {
                title: "Acceso denegado",
                status: 403,
                errors: {
                    Auth_NegocioSuspendidoOAccesoRevocado:
                        "El negocio actual está suspendido o tu acceso fue revocado.",
                },
            },
        });

        const { GET } = await import("@/app/api/auth/validate-session/route");
        const response = await GET();

        expect(validarTokenServiceMock).toHaveBeenCalledTimes(1);
        expect(response.status).toBe(401);
        await expect(response.json()).resolves.toEqual({ valido: false });
    });

    it("responde 401 cuando el token ya no es valido en API (401 real)", async () => {
        asegurarAccessTokenMock.mockResolvedValue("ok");
        validarTokenServiceMock.mockResolvedValue({
            data: null,
            errors: {
                title: "No autorizado",
                status: 401,
                errors: {},
            },
        });

        const { GET } = await import("@/app/api/auth/validate-session/route");
        const response = await GET();

        expect(validarTokenServiceMock).toHaveBeenCalledTimes(1);
        expect(response.status).toBe(401);
        await expect(response.json()).resolves.toEqual({ valido: false });
    });

    it("responde 503 cuando el service falla con error de red/5xx", async () => {
        asegurarAccessTokenMock.mockResolvedValue("ok");
        validarTokenServiceMock.mockResolvedValue({
            data: null,
            errors: {
                title: "Error interno",
                status: 500,
                errors: {},
            },
        });

        const { GET } = await import("@/app/api/auth/validate-session/route");
        const response = await GET();

        expect(validarTokenServiceMock).toHaveBeenCalledTimes(1);
        expect(response.status).toBe(503);
        await expect(response.json()).resolves.toEqual({ valido: false, transitorio: true });
    });

    it("responde 503 cuando el service lanza excepcion", async () => {
        asegurarAccessTokenMock.mockResolvedValue("ok");
        validarTokenServiceMock.mockRejectedValue(new Error("network error"));

        const { GET } = await import("@/app/api/auth/validate-session/route");
        const response = await GET();

        expect(response.status).toBe(503);
        await expect(response.json()).resolves.toEqual({ valido: false, transitorio: true });
    });

    it("retorna valido cuando access token y contexto tenant siguen activos", async () => {
        asegurarAccessTokenMock.mockResolvedValue("ok");
        validarTokenServiceMock.mockResolvedValue({
            data: null,
            errors: null,
        });

        const { GET } = await import("@/app/api/auth/validate-session/route");
        const response = await GET();

        expect(validarTokenServiceMock).toHaveBeenCalledTimes(1);
        expect(response.status).toBe(200);
        await expect(response.json()).resolves.toEqual({ valido: true });
    });
});
