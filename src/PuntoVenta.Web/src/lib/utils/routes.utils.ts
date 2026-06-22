export type BreadcrumbRoute = {
    pattern: string;
    label: string;
    title_page: string;
    href: string | null;
};

export function isLeafActive(
    pathname: string,
    item: { href: string; exact?: boolean }
): boolean {
    return item.exact
        ? pathname === item.href
        : pathname === item.href || pathname.startsWith(item.href + "/");
}

export function matchRoute(pathname: string, pattern: string): boolean {
    const pathParts = pathname.split("/").filter(Boolean);
    const patternParts = pattern.split("/").filter(Boolean);

    return (
        pathParts.length === patternParts.length &&
        pathParts.every(
            (part, i) => patternParts[i] === "*" || patternParts[i] === part
        )
    );
}

export function getBreadcrumbs(
    pathname: string,
    routes: readonly BreadcrumbRoute[]
): BreadcrumbRoute[] {
    const parts = pathname.split("/").filter(Boolean);

    if (parts.length === 0) return [];

    const prefixes = parts.map((_, i) => "/" + parts.slice(0, i + 1).join("/"));
    const crumbs: BreadcrumbRoute[] = [];

    for (const prefix of prefixes) {
        // Matching es por patrón con comodines (`*`), no por igualdad — no se
        // puede indexar en un Map por clave exacta.
        // react-doctor-disable-next-line react-doctor/js-index-maps
        const match = routes.find((r) => matchRoute(prefix, r.pattern));
        if (match) crumbs.push(match);
    }

    return crumbs;
}
