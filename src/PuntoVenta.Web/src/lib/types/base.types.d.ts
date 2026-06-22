import { z } from "zod";

export type FormValues<T> = z.infer<T>;

export type ApiValidationErrors = Record<string, string>;

export interface ApiError {
    type: string;
    title: string;
    status: number;
    errors: ApiValidationErrors;
}

export type ErrorSeverity = "error" | "warning" | "info";

export interface ProblemDetail {
    title: string
    status: number,
    errors: ApiValidationErrors | undefined;
    severity?: ErrorSeverity;
}

export interface DataAPI<T = unknown> {
    data: T | undefined | null;
    errors: ProblemDetail | undefined | null;
}

export interface RequesAPIParams {
    url: string;
    method: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
    body?: unknown;
    query?: Record<string, string | number | boolean | null | undefined>;
    skipAuth?: boolean;
}

export interface ActionResult {
    status: number;
    errors: ApiValidationErrors | undefined;
    severity?: ErrorSeverity;
}

export interface ActionResultData<T = unknown> extends DataAPI<T>, ActionResult {
    status: number; data: T | undefined | null;
}

export type EstadoFiltro = "todos" | "activos" | "inactivos";

export interface ColumnDefinition<T> {
    key: string;
    header: ReactNode;
    cell: (row: T) => ReactNode;
    width?: number | string;
    align?: "left" | "center" | "right";
}

export interface PagedResult<T> {
    items: T[];
    pagina: number;
    tamano: number;
    totalRegistros: number;
    totalPaginas: number;
}
