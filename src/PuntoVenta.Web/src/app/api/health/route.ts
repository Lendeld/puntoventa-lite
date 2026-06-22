import { NextResponse } from "next/server";

function joinUrl(baseUrl: string, path: string): string {
    const cleanBase = baseUrl.endsWith("/") ? baseUrl.slice(0, -1) : baseUrl;
    const cleanPath = path.startsWith("/") ? path : `/${path}`;
    return `${cleanBase}${cleanPath}`;
}

export async function GET() {
    try {
        await fetch(joinUrl(process.env.BASE_URL_API ?? "", "/health"), {
            method: "GET",
            signal: AbortSignal.timeout(5000),
        });
    } catch {
        // Ignorar errores — solo queremos despertar el API (warm-up)
    }

    return NextResponse.json({ ok: true });
}
