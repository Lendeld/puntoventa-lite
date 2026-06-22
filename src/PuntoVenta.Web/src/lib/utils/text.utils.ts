export function normalizeText(value: string): string {
    return value.trim();
}

export function normalizeOptionalText(
    value: string | null | undefined,
): string | undefined {
    const trimmed = value?.trim();
    return trimmed ? trimmed : undefined;
}

export function normalizeNullableText(
    value: string | null | undefined,
): string | null {
    return normalizeOptionalText(value) ?? null;
}
