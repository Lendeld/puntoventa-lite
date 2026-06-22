import {
    guardarTokensSesion,
    obtenerSesion,
} from "@/lib/auth/sesion";
import { expiro } from "@/lib/utils/date";

function assertBaseUrlSeguro(): void {
    if (process.env.NODE_ENV !== "production" || process.env.PUNTO_VENTA_WEB_DEV_ENV_PATH) return;
    if (process.env.NEXT_PHASE === "phase-production-build") return;
    const baseUrl = process.env.BASE_URL_API ?? "";
    if (baseUrl.startsWith("https://")) return;
    // LocalHost (Electron) corre el API en http://127.0.0.1:PORT — no hay
    // certs para loopback. HTTP esta OK si es 127.0.0.1 o localhost.
    if (/^http:\/\/(127\.0\.0\.1|localhost)(:\d+)?(\/|$)/.test(baseUrl)) return;
    throw new Error("BASE_URL_API debe usar HTTPS en producción.");
}

type FetchConfig = {
    validateStatus?: (status: number) => boolean;
    skipAuth?: boolean;
    skipRefreshRetry?: boolean;
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
type FetchResponse<T = any> = {
    data: T;
    status: number;
    statusText: string;
};

export type TokenEstado = "ok" | "no-auth" | "transitorio";

let refreshEnCurso: Promise<TokenEstado> | null = null;

const REFRESH_DELAYS_MS = [2000, 5000];

function sleep(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
}

function joinUrl(baseUrl: string, path: string): string {
    const cleanBase = baseUrl.endsWith("/") ? baseUrl.slice(0, -1) : baseUrl;
    const cleanPath = path.startsWith("/") ? path : `/${path}`;
    return `${cleanBase}${cleanPath}`;
}

async function parseResponse<T>(response: Response): Promise<FetchResponse<T>> {
    const contentType = response.headers.get("content-type");
    const rawBody = response.status === 204 ? "" : await response.text();
    const hasBody = rawBody.trim().length > 0;
    const isJson = contentType?.toLowerCase().includes("json") ?? false;

    let data: T = null as T;

    if (hasBody) {
        if (isJson) {
            try {
                data = JSON.parse(rawBody) as T;
            } catch {
                data = rawBody as T;
            }
        } else {
            data = rawBody as T;
        }
    }

    return { data, status: response.status, statusText: response.statusText };
}

async function buildHeaders(
    method: string,
    body?: unknown,
    config?: FetchConfig,
): Promise<Record<string, string>> {
    const headers: Record<string, string> = {
        Accept: "application/json, application/problem+json",
    };

    if (body !== undefined && method !== "GET") {
        headers["Content-Type"] = "application/json";
    }

    if (!config?.skipAuth) {
        const sesion = await obtenerSesion();
        if (sesion?.accessToken) {
            headers["Authorization"] = `Bearer ${sesion.accessToken}`;
        }
    }

    return headers;
}

export async function asegurarAccessToken(): Promise<TokenEstado> {
    const sesion = await obtenerSesion();

    if (sesion.accessToken && !expiro(sesion.accessTokenExpiracionUtc)) {
        return "ok";
    }

    if (!sesion.refreshToken || expiro(sesion.refreshTokenExpiracionUtc)) {
        return "no-auth";
    }

    return renovarSesion();
}

async function renovarSesion(): Promise<TokenEstado> {
    if (refreshEnCurso) {
        return refreshEnCurso;
    }

    refreshEnCurso = ejecutarRefresh();

    try {
        return await refreshEnCurso;
    } finally {
        refreshEnCurso = null;
    }
}

async function ejecutarRefresh(): Promise<TokenEstado> {
    const sesion = await obtenerSesion();

    if (!sesion.refreshToken) {
        return "no-auth";
    }

    // Fail-fast de configuración: fuera del loop de retry para no enmascararlo como transitorio
    assertBaseUrlSeguro();

    for (let intento = 0; intento <= REFRESH_DELAYS_MS.length; intento++) {
        if (intento > 0) {
            await sleep(REFRESH_DELAYS_MS[intento - 1]);
        }

        try {
            const response = await fetch(joinUrl(process.env.BASE_URL_API ?? "", "/auth/refresh"), {
                method: "POST",
                headers: {
                    Accept: "application/json, application/problem+json",
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ refreshToken: sesion.refreshToken }),
                signal: AbortSignal.timeout(60000),
            });

            // 400/401 del API → token inválido real, no reintentar
            if (response.status === 400 || response.status === 401) {
                return "no-auth";
            }

            if (!response.ok) {
                // 429 (rate limiting) y 5xx (cold start/servidor) → transitorio, reintentar con backoff
                continue;
            }

            const payload = await parseResponse<{
                accessToken: string;
                accessTokenExpiracionUtc: string;
                refreshToken: string;
                refreshTokenExpiracionUtc: string;
            }>(response);

            // 200 con body sin tokens parseables (ej. proxy devuelve HTML): podría ser
            // un intermediario, no necesariamente token inválido → transitorio, no logout
            if (!payload.data?.accessToken || !payload.data?.refreshToken) {
                return "transitorio";
            }

            await guardarTokensSesion(payload.data);
            return "ok";
        } catch {
            // Red, timeout u otro error → transitorio, reintentar si quedan intentos
            if (intento < REFRESH_DELAYS_MS.length) {
                continue;
            }
        }
    }

    return "transitorio";
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function requestFormData<T = any>(
    url: string,
    formData: FormData,
    config?: FetchConfig,
): Promise<FetchResponse<T>> {
    const headers: Record<string, string> = {
        Accept: "application/json, application/problem+json",
    };

    if (!config?.skipAuth) {
        const estado = await asegurarAccessToken();
        if (estado === "no-auth") {
            return { data: null as T, status: 401, statusText: "Unauthorized" };
        }
        if (estado === "transitorio") {
            return { data: null as T, status: 503, statusText: "Service Unavailable" };
        }

        const sesion = await obtenerSesion();
        if (sesion?.accessToken) {
            headers["Authorization"] = `Bearer ${sesion.accessToken}`;
        }
    }

    assertBaseUrlSeguro();
    const fullUrl = joinUrl(process.env.BASE_URL_API ?? "", url);
    const response = await fetch(fullUrl, {
        method: "POST",
        headers,
        body: formData,
        signal: AbortSignal.timeout(60000),
    });

    // 429 (rate limiting) → transitorio, nunca auth inválida
    if (response.status === 429) {
        return { data: null as T, status: 503, statusText: "Service Unavailable" };
    }

    const validateStatus = config?.validateStatus ?? ((status: number) => status < 500);
    if (!validateStatus(response.status)) {
        throw new Error(`Request failed with status ${response.status}`);
    }

    return parseResponse<T>(response);
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function request<T = any>(
    method: string,
    url: string,
    body?: unknown,
    config?: FetchConfig,
): Promise<FetchResponse<T>> {
    if (!config?.skipAuth) {
        const estado = await asegurarAccessToken();
        if (estado === "no-auth") {
            return { data: null as T, status: 401, statusText: "Unauthorized" };
        }
        if (estado === "transitorio") {
            return { data: null as T, status: 503, statusText: "Service Unavailable" };
        }
    }

    const headers = await buildHeaders(method, body, config);
    assertBaseUrlSeguro();
    const fullUrl = joinUrl(process.env.BASE_URL_API ?? "", url);

    const response = await fetch(fullUrl, {
        method,
        headers,
        body: body != undefined ? JSON.stringify(body) : undefined,
        signal: AbortSignal.timeout(30000),
    });

    // 429 (rate limiting) de la petición original → transitorio, nunca auth inválida
    if (response.status === 429) {
        return { data: null as T, status: 503, statusText: "Service Unavailable" };
    }

    if (response.status === 401 && !config?.skipRefreshRetry && !config?.skipAuth) {
        const refreshEstado = await renovarSesion();

        if (refreshEstado === "ok") {
            const retryHeaders = await buildHeaders(method, body, {
                ...config,
                skipRefreshRetry: true,
            });

            const retryResponse = await fetch(fullUrl, {
                method,
                headers: retryHeaders,
                body: body != undefined ? JSON.stringify(body) : undefined,
                signal: AbortSignal.timeout(30000),
            });

            // 429 en el retry también → transitorio
            if (retryResponse.status === 429) {
                return { data: null as T, status: 503, statusText: "Service Unavailable" };
            }

            const validateStatus = config?.validateStatus ?? ((status: number) => status < 500);
            if (!validateStatus(retryResponse.status)) {
                throw new Error(`Request failed with status ${retryResponse.status}`);
            }

            return parseResponse<T>(retryResponse);
        }

        // El refresh falló por razón transitoria (red/5xx): devolver 503 sintético
        // para que los consumidores lo traten como error de servidor, no como sesión inválida.
        if (refreshEstado === "transitorio") {
            return { data: null as T, status: 503, statusText: "Service Unavailable" };
        }
    }

    const validateStatus = config?.validateStatus ?? ((status: number) => status < 500);

    if (!validateStatus(response.status)) {
        throw new Error(`Request failed with status ${response.status}`);
    }

    return parseResponse<T>(response);
}

const apiClient = {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    get: <T = any>(url: string, config?: FetchConfig) =>
        request<T>("GET", url, undefined, config),
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    post: <T = any>(url: string, body?: unknown, config?: FetchConfig) =>
        request<T>("POST", url, body, config),
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    put: <T = any>(url: string, body?: unknown, config?: FetchConfig) =>
        request<T>("PUT", url, body, config),
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    patch: <T = any>(url: string, body?: unknown, config?: FetchConfig) =>
        request<T>("PATCH", url, body, config),
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    delete: <T = any>(url: string, config?: FetchConfig) =>
        request<T>("DELETE", url, undefined, config),
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    postFormData: <T = any>(url: string, formData: FormData, config?: FetchConfig) =>
        requestFormData<T>(url, formData, config),
};

export default apiClient;
export type { FetchConfig, FetchResponse };
