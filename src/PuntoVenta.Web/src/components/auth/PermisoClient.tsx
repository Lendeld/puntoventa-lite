"use client";

import { usePermisoQuery } from "@lib/hooks/usePermisoQuery";

type Props = {
    permiso: string;
    children: React.ReactNode;
    fallback?: React.ReactNode;
};

export function PermisoClient({ permiso, children, fallback = null }: Props) {
    const { data: allowed, isLoading } = usePermisoQuery(permiso);

    if (isLoading) return <>{fallback}</>;

    return allowed ? <>{children}</> : <>{fallback}</>;
}
