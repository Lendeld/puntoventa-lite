import { obtenerSesion } from "@lib/auth/sesion";
import { asegurarAccessToken } from "@lib/utils/apiClient";
import { NextRequest, NextResponse } from "next/server";

function joinUrl(baseUrl: string, path: string): string {
    const cleanBase = baseUrl.endsWith("/") ? baseUrl.slice(0, -1) : baseUrl;
    const cleanPath = path.startsWith("/") ? path : `/${path}`;
    return `${cleanBase}${cleanPath}`;
}

export async function GET(
    request: NextRequest,
    context: { params: Promise<{ id: string; pagoId: string }> },
) {
    const tokenEstado = await asegurarAccessToken();
    if (tokenEstado === "no-auth") {
        const proto = request.headers.get("x-forwarded-proto") ?? "https";
        const host = request.headers.get("x-forwarded-host") ?? request.headers.get("host") ?? "";
        const base = host ? `${proto}://${host}` : new URL(request.url).origin;
        return NextResponse.redirect(new URL("/login", base));
    }

    const [{ id, pagoId }, sesion] = await Promise.all([context.params, obtenerSesion()]);

    const response = await fetch(joinUrl(process.env.BASE_URL_API ?? "", `/ventas/${id}/abonos/${pagoId}/pdf`), {
        method: "GET",
        headers: {
            Accept: "application/pdf",
            Authorization: `Bearer ${sesion.accessToken}`,
        },
        cache: "no-store",
        signal: AbortSignal.timeout(30000),
    });

    if (!response.ok) {
        const message = await response.text().catch(() => "");
        return NextResponse.json(
            { message: message || "No fue posible obtener el PDF del recibo." },
            { status: response.status },
        );
    }

    const content = await response.arrayBuffer();
    const headers = new Headers();
    headers.set("Content-Type", response.headers.get("content-type") ?? "application/pdf");
    headers.set(
        "Content-Disposition",
        response.headers.get("content-disposition") ?? `inline; filename="recibo-abono-${pagoId}.pdf"`,
    );

    return new NextResponse(content, { status: 200, headers });
}
