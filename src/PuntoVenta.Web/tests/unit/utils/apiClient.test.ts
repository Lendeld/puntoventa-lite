import { beforeEach, describe, expect, it, vi } from "vitest";

const obtenerSesionMock = vi.fn();
const guardarTokensSesionMock = vi.fn();

vi.mock("@/lib/auth/sesion", () => ({
    obtenerSesion: obtenerSesionMock,
    guardarTokensSesion: guardarTokensSesionMock,
}));

describe("apiClient", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.resetModules();
        process.env.BASE_URL_API = "https://api.demo.local";

        if (!AbortSignal.timeout) {
            Object.defineProperty(AbortSignal, "timeout", {
                value: () => AbortSignal.abort(),
                configurable: true,
            });
        }
    });

    it("comparte un solo refresh cuando expira el access token en solicitudes concurrentes", async () => {
        const sesion = {
            accessToken: "access-expirado",
            accessTokenExpiracionUtc: "2026-01-01T00:00:00.000Z",
            refreshToken: "refresh-vigente",
            refreshTokenExpiracionUtc: "2099-01-01T00:00:00.000Z",
            otpPendiente: false,
            seleccionNegocioPendiente: false,
        };

        obtenerSesionMock.mockImplementation(async () => sesion);
        guardarTokensSesionMock.mockImplementation(async (tokens) => {
            sesion.accessToken = tokens.accessToken;
            sesion.accessTokenExpiracionUtc = tokens.accessTokenExpiracionUtc;
            sesion.refreshToken = tokens.refreshToken;
            sesion.refreshTokenExpiracionUtc = tokens.refreshTokenExpiracionUtc;
        });

        let resolverFetch!: (response: Response) => void;
        const fetchMock = vi.fn(
            () =>
                new Promise<Response>((resolve) => {
                    resolverFetch = resolve;
                }),
        );

        vi.stubGlobal("fetch", fetchMock);

        const { asegurarAccessToken } = await import("@/lib/utils/apiClient");
        const refresh1 = asegurarAccessToken();
        const refresh2 = asegurarAccessToken();
        await Promise.resolve();
        await Promise.resolve();

        expect(fetchMock).toHaveBeenCalledTimes(1);
        resolverFetch(
            new Response(
                JSON.stringify({
                    accessToken: "access-nuevo",
                    accessTokenExpiracionUtc: "2099-01-02T00:00:00.000Z",
                    refreshToken: "refresh-nuevo",
                    refreshTokenExpiracionUtc: "2099-02-01T00:00:00.000Z",
                }),
                {
                    status: 200,
                    headers: {
                        "content-type": "application/json",
                    },
                },
            ),
        );

        await expect(refresh1).resolves.toBe("ok");
        await expect(refresh2).resolves.toBe("ok");
        expect(guardarTokensSesionMock).toHaveBeenCalledTimes(1);
        expect(sesion.refreshToken).toBe("refresh-nuevo");
    });

    it("retorna 401 controlado sin llamar al API cuando no puede asegurar access token", async () => {
        obtenerSesionMock.mockResolvedValue({
            accessToken: "access-expirado",
            accessTokenExpiracionUtc: "2026-01-01T00:00:00.000Z",
            refreshToken: undefined,
            refreshTokenExpiracionUtc: undefined,
            otpPendiente: false,
            seleccionNegocioPendiente: false,
        });

        const fetchMock = vi.fn();
        vi.stubGlobal("fetch", fetchMock);

        const { default: apiClient } = await import("@/lib/utils/apiClient");
        const response = await apiClient.get("/auth/validar-token");

        expect(response.status).toBe(401);
        expect(response.data).toBeNull();
        expect(fetchMock).not.toHaveBeenCalled();
    });

    it("asegurarAccessToken retorna 'no-auth' cuando no hay refresh token", async () => {
        obtenerSesionMock.mockResolvedValue({
            accessToken: "access-expirado",
            accessTokenExpiracionUtc: "2026-01-01T00:00:00.000Z",
            refreshToken: undefined,
            refreshTokenExpiracionUtc: undefined,
            otpPendiente: false,
            seleccionNegocioPendiente: false,
        });

        const { asegurarAccessToken } = await import("@/lib/utils/apiClient");
        const estado = await asegurarAccessToken();
        expect(estado).toBe("no-auth");
    });

    it("asegurarAccessToken retorna 'ok' cuando el access token es vigente", async () => {
        obtenerSesionMock.mockResolvedValue({
            accessToken: "access-vigente",
            accessTokenExpiracionUtc: "2099-01-01T00:00:00.000Z",
            refreshToken: "refresh-vigente",
            refreshTokenExpiracionUtc: "2099-01-01T00:00:00.000Z",
            otpPendiente: false,
            seleccionNegocioPendiente: false,
        });

        const fetchMock = vi.fn();
        vi.stubGlobal("fetch", fetchMock);

        const { asegurarAccessToken } = await import("@/lib/utils/apiClient");
        const estado = await asegurarAccessToken();

        expect(estado).toBe("ok");
        expect(fetchMock).not.toHaveBeenCalled();
    });

    it("ejecutarRefresh retorna 'no-auth' cuando el API responde 401", async () => {
        obtenerSesionMock.mockResolvedValue({
            accessToken: "access-expirado",
            accessTokenExpiracionUtc: "2026-01-01T00:00:00.000Z",
            refreshToken: "refresh-invalido",
            refreshTokenExpiracionUtc: "2099-01-01T00:00:00.000Z",
            otpPendiente: false,
            seleccionNegocioPendiente: false,
        });

        const fetchMock = vi.fn().mockResolvedValue(
            new Response(null, { status: 401 }),
        );
        vi.stubGlobal("fetch", fetchMock);

        const { asegurarAccessToken } = await import("@/lib/utils/apiClient");
        const estado = await asegurarAccessToken();

        expect(estado).toBe("no-auth");
        // Solo 1 intento — 401 real no se reintenta
        expect(fetchMock).toHaveBeenCalledTimes(1);
    });

    it("request retorna 503 sintetico cuando el refresh es transitorio", async () => {
        vi.useFakeTimers();
        obtenerSesionMock.mockResolvedValue({
            accessToken: "access-expirado",
            accessTokenExpiracionUtc: "2026-01-01T00:00:00.000Z",
            refreshToken: "refresh-vigente",
            refreshTokenExpiracionUtc: "2099-01-01T00:00:00.000Z",
            otpPendiente: false,
            seleccionNegocioPendiente: false,
        });

        // Simular que el API siempre retorna 503 (cold start)
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(null, { status: 503 }),
        );
        vi.stubGlobal("fetch", fetchMock);

        const { default: apiClient } = await import("@/lib/utils/apiClient");

        const responsePromise = apiClient.get("/alguna-ruta");

        // Avanzar los timers para los reintentos del refresh
        await vi.runAllTimersAsync();

        const response = await responsePromise;

        expect(response.status).toBe(503);
        vi.useRealTimers();
    });

    it("request retorna 503 sintetico cuando la peticion original recibe 429 (rate limiting)", async () => {
        obtenerSesionMock.mockResolvedValue({
            accessToken: "access-vigente",
            accessTokenExpiracionUtc: "2099-01-01T00:00:00.000Z",
            refreshToken: "refresh-vigente",
            refreshTokenExpiracionUtc: "2099-01-01T00:00:00.000Z",
            otpPendiente: false,
            seleccionNegocioPendiente: false,
        });

        // El API responde 429 (Too Many Requests) a la peticion original
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(null, { status: 429, statusText: "Too Many Requests" }),
        );
        vi.stubGlobal("fetch", fetchMock);

        const { default: apiClient } = await import("@/lib/utils/apiClient");
        const response = await apiClient.get("/alguna-ruta");

        // 429 debe mapearse a 503 sintetico, nunca a 401 (no logout)
        expect(response.status).toBe(503);
        expect(response.status).not.toBe(401);
        // Solo 1 llamada al API (no dispara refresh de sesion)
        expect(fetchMock).toHaveBeenCalledTimes(1);
    });

    it("ejecutarRefresh reintenta ante 429 y retorna transitorio si se agotan los intentos", async () => {
        vi.useFakeTimers();
        obtenerSesionMock.mockResolvedValue({
            accessToken: "access-expirado",
            accessTokenExpiracionUtc: "2026-01-01T00:00:00.000Z",
            refreshToken: "refresh-vigente",
            refreshTokenExpiracionUtc: "2099-01-01T00:00:00.000Z",
            otpPendiente: false,
            seleccionNegocioPendiente: false,
        });

        // El endpoint de refresh siempre responde 429
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(null, { status: 429, statusText: "Too Many Requests" }),
        );
        vi.stubGlobal("fetch", fetchMock);

        const { asegurarAccessToken } = await import("@/lib/utils/apiClient");

        const estadoPromise = asegurarAccessToken();

        // Avanzar timers para cubrir los backoffs del retry (2s + 5s)
        await vi.advanceTimersByTimeAsync(10_000);

        const estado = await estadoPromise;

        // 429 agotando reintentos → transitorio, NUNCA no-auth
        expect(estado).toBe("transitorio");
        expect(estado).not.toBe("no-auth");
        // 3 intentos en total (intento 0 + 2 reintentos)
        expect(fetchMock).toHaveBeenCalledTimes(3);

        vi.useRealTimers();
    });
});
