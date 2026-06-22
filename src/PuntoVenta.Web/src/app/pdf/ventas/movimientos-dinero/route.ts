import { obtenerSesion } from "@lib/auth/sesion";
import { asegurarAccessToken } from "@lib/utils/apiClient";
import { NextRequest, NextResponse } from "next/server";

export const dynamic = "force-dynamic";

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
    const fechaDesde = url.searchParams.get("fechaDesde");
    const fechaHasta = url.searchParams.get("fechaHasta");
    const cajaId = url.searchParams.get("cajaId");

    if (!fechaDesde || !fechaHasta) {
        return NextResponse.json(
            { message: "Faltan parámetros fechaDesde/fechaHasta." },
            { status: 400 },
        );
    }

    const qs = new URLSearchParams({ fechaDesde, fechaHasta });
    if (cajaId) qs.set("cajaId", cajaId);

    const sesion = await obtenerSesion();
    let response: Response;

    try {
        response = await fetch(
            joinUrl(process.env.BASE_URL_API ?? "", `/ventas/reportes/movimientos-dinero/pdf?${qs.toString()}`),
            {
                method: "GET",
                headers: {
                    Accept: "application/pdf",
                    Authorization: `Bearer ${sesion.accessToken}`,
                },
                cache: "no-store",
                signal: AbortSignal.timeout(30000),
            },
        );
    } catch {
        return NextResponse.json(
            { message: "No fue posible conectar con el servicio de PDF." },
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
    headers.set("Content-Type", response.headers.get("content-type") ?? "application/pdf");
    headers.set(
        "Content-Disposition",
        response.headers.get("content-disposition") ?? 'inline; filename="reporte-movimientos-dinero.pdf"',
    );

    return new NextResponse(content, { status: 200, headers });
}
