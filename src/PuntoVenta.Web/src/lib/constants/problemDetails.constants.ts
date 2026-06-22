import { ProblemDetail } from "@/lib/types/base.types";

export const problemDetailInternalError: ProblemDetail = {
    title: "Internal Server Error",
    status: 500,
    errors: undefined
} as const;

export const problemDetailUnauthorized: ProblemDetail = {
    title: "Unauthorized",
    status: 401,
    errors: undefined
} as const;

export const problemDetailNotFound: ProblemDetail = {
    title: "Not Found",
    status: 404,
    errors: undefined
} as const;

export const problemDetailForbidden: ProblemDetail = {
    title: "Forbidden",
    status: 403,
    errors: undefined
} as const;
