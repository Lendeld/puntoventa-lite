"use server";

import { problemDetailForbidden, problemDetailInternalError, problemDetailNotFound, problemDetailUnauthorized } from "@/lib/constants/problemDetails.constants";
import { DataAPI, ErrorSeverity, ProblemDetail, RequesAPIParams } from "@/lib/types/base.types";
import apiClient, { type FetchResponse } from "@/lib/utils/apiClient";

// Cada respuesta arranca de una base nueva — nunca compartir un objeto mutable
// a nivel de módulo entre requests concurrentes.
function emptyDataAPI(): DataAPI<null> {
    return { data: null, errors: null };
}

// Normaliza la respuesta de error del backend a nuestro shape ProblemDetail.
// ASP.NET puede devolver `errors: { campo: ["msg1", "msg2"] }` (binding/JSON) o
// nuestro custom `errors: { campo: "msg" }`. Aplanamos a Record<string, string>.
function normalizeProblemDetail(
    data: unknown,
    status: number,
    statusText: string,
): ProblemDetail {
    const raw = (data ?? {}) as Record<string, unknown>;
    const rawErrors = (raw.errors ?? {}) as Record<string, unknown>;
    const flat: Record<string, string> = {};
    for (const [key, value] of Object.entries(rawErrors)) {
        if (Array.isArray(value)) {
            flat[key] = value.filter((v) => typeof v === "string").join(" ");
        } else if (typeof value === "string") {
            flat[key] = value;
        } else if (value != null) {
            flat[key] = String(value);
        }
    }
    const rawSeverity = typeof raw.severity === "string" ? raw.severity.toLowerCase() : undefined;
    const severity: ErrorSeverity | undefined =
        rawSeverity === "warning" || rawSeverity === "info" || rawSeverity === "error"
            ? rawSeverity
            : undefined;
    return {
        title: typeof raw.title === "string" ? raw.title : statusText,
        status,
        errors: Object.keys(flat).length > 0 ? flat : undefined,
        severity,
    };
}

function buildQueryString(query: Record<string, string | number | boolean | null | undefined>): string {
    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(query)) {
        if (value !== null && value !== undefined && value !== "") {
            params.append(key, String(value));
        }
    }
    const qs = params.toString();
    return qs ? `?${qs}` : "";
}

export async function requestAPI<T = unknown>({
    url,
    method,
    body,
    query,
    skipAuth,
}: RequesAPIParams): Promise<DataAPI<T>> {
    let request: Promise<FetchResponse<T>>;
    const fullUrl = query ? `${url}${buildQueryString(query)}` : url;

    switch (method) {
        case "GET":
            request = apiClient.get(fullUrl, { skipAuth });
            break;
        case "POST":
            request = apiClient.post(fullUrl, body, { skipAuth });
            break;
        case "PUT":
            request = apiClient.put(fullUrl, body, { skipAuth });
            break;
        case "PATCH":
            request = apiClient.patch(fullUrl, body, { skipAuth });
            break;
        case "DELETE":
            request = apiClient.delete(fullUrl, { skipAuth });
            break;
    }

    return request
        .then((response: FetchResponse<T>) => {
            if (response.status === 400 || response.status === 409 || response.status === 403) {

                if (!response.data && response.status === 403) {
                    return { ...emptyDataAPI(), errors: problemDetailForbidden };
                }

                return {
                    ...emptyDataAPI(),
                    errors: normalizeProblemDetail(response.data, response.status, response.statusText),
                };
            }
            if (response.status === 401) {
                return {
                    ...emptyDataAPI(),
                    errors: response.data
                        ? (response.data as unknown as ProblemDetail)
                        : problemDetailUnauthorized,
                };
            }
            if (response.status === 404) return { ...emptyDataAPI(), errors: problemDetailNotFound };

            // 429 (rate limiting) y cualquier 5xx (incluido el 503 sintético de transitorio)
            // son errores transitorios recuperables, independientemente del método.
            // Sin esto un GET con 429/503 pasaría como éxito.
            if (response.status === 429 || response.status >= 500) {
                return {
                    ...emptyDataAPI(),
                    errors: {
                        title: response.statusText,
                        status: response.status,
                    } as ProblemDetail,
                };
            }

            if (
                method !== "GET" &&
                response.status !== 200 &&
                response.status !== 201 &&
                response.status !== 204
            ) {
                return {
                    ...emptyDataAPI(),
                    errors: {
                        title: response.statusText,
                        status: response.status
                    } as ProblemDetail
                };
            }
            return { ...emptyDataAPI(), data: response.data };
        })
        .catch((error: unknown) => {
            if (process.env.NODE_ENV !== "production") {
                console.error(`[requestAPI] ${method} ${url}`, error);
            }
            return { ...emptyDataAPI(), errors: problemDetailInternalError };
        });
}
