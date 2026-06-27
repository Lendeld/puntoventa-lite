import { obtenerSesion } from "@lib/auth/sesion";
import { asegurarAccessToken } from "@lib/utils/apiClient";
import { NextRequest, NextResponse } from "next/server";

export const dynamic = "force-dynamic";

const XLSX_CONTENT_TYPE =
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

function joinUrl(baseUrl: string, path: string): string {
    const cleanBase = baseUrl.endsWith("/") ? baseUrl.slice(0, -1) : baseUrl;
    const cleanPath = path.startsWith("/") ? path : `/${path}`;
    return `${cleanBase}${cleanPath}`;
}

export async function GET(request: NextRequest) {
    const tokenEstado = await asegurarAccessToken();
    if (tokenEstado === "no-auth") {
        const proto = request.headers.get("x-forwarded-proto") ?? "https";
        const host = request.headers.get("x-forwarded-host") ?? request.headers.get("host") ?? "";
        const base = host ? `${proto}://${host}` : new URL(request.url).origin;
        return NextResponse.redirect(new URL("/login", base));
    }

    const url = new URL(request.url);
    // ponytail: allow-list only; any other param is silently dropped
    const qs = new URLSearchParams();
    const codigo = url.searchParams.get("Codigo");
    const categoriaId = url.searchParams.get("CategoriaId");
    if (codigo) qs.set("Codigo", codigo);
    if (categoriaId) qs.set("CategoriaId", categoriaId);

    const sesion = await obtenerSesion();
    let response: Response;

    try {
        const endpoint = `/productos/reportes/inventario/excel${qs.size ? `?${qs.toString()}` : ""}`;
        response = await fetch(
            joinUrl(process.env.BASE_URL_API ?? "", endpoint),
            {
                method: "GET",
                headers: {
                    Accept: XLSX_CONTENT_TYPE,
                    Authorization: `Bearer ${sesion.accessToken}`,
                },
                cache: "no-store",
                signal: AbortSignal.timeout(30000),
            },
        );
    } catch {
        return NextResponse.json(
            { message: "No fue posible conectar con el servicio de Excel." },
            { status: 502 },
        );
    }

    if (!response.ok) {
        const message = await response.text().catch(() => "");
        return NextResponse.json(
            { message: message || "No fue posible obtener el reporte." },
            { status: response.status },
        );
    }

    const content = await response.arrayBuffer();
    const headers = new Headers();
    headers.set("Content-Type", response.headers.get("content-type") ?? XLSX_CONTENT_TYPE);
    headers.set(
        "Content-Disposition",
        response.headers.get("content-disposition") ??
            `attachment; filename="reporte-inventario.xlsx"`,
    );
    headers.set("Cache-Control", "no-store");

    return new NextResponse(content, { status: 200, headers });
}
