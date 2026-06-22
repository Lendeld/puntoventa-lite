export function expiro(iso?: string) {
    if (!iso) return true;
    return new Date(iso).getTime() <= Date.now();
}
