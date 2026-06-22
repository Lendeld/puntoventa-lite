import type { core } from "zod";
import type { ApiValidationErrors } from "@lib/types/base.types";

type ZodIssuesToErrorsOptions = {
    separator?: string;
};

export function zodIssuesToErrors(
    issues: core.$ZodIssue[],
    options?: ZodIssuesToErrorsOptions,
): ApiValidationErrors {
    const separator = options?.separator ?? "_";

    return issues.reduce<ApiValidationErrors>((acc, issue) => {
        const key = issue.path.map(String).join(separator);

        if (!key || acc[key]) {
            return acc;
        }

        acc[key] = issue.message;
        return acc;
    }, {});
}
