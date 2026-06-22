import type { ApiValidationErrors, DataAPI, ProblemDetail } from "@lib/types/base.types";

export type DataAPIErr = { data: null | undefined; errors: ProblemDetail };

export function isErr<T>(res: DataAPI<T>): res is DataAPIErr {
    return Boolean(res.errors);
}

interface ResolveApiErrorMessageOptions {
    preferredKeys?: string[];
    fallback?: string;
}

export function resolveApiErrorMessage(
    errors?: ApiValidationErrors,
    options: ResolveApiErrorMessageOptions = {},
): string {
    const { preferredKeys = [], fallback = "Error inesperado." } = options;

    if (!errors) {
        return fallback;
    }

    for (const key of preferredKeys) {
        const message = errors[key];
        if (message) {
            return message;
        }
    }

    const [firstMessage] = Object.values(errors);
    return firstMessage ?? fallback;
}
