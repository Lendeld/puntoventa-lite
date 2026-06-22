import { NextResponse } from "next/server";
import { asegurarAccessToken } from "@lib/utils/apiClient";
import { validarTokenService } from "@lib/services/auth.service";

export async function GET() {
    const estado = await asegurarAccessToken();

    if (estado === "no-auth") {
        return NextResponse.json({ valido: false }, { status: 401 });
    }

    if (estado === "transitorio") {
        return NextResponse.json(
            { valido: false, transitorio: true },
            { status: 503 },
        );
    }

    try {
        const tokenValido = await validarTokenService();

        if (tokenValido.errors) {
            const status = tokenValido.errors.status;
            if (status === 401 || status === 403) {
                return NextResponse.json({ valido: false }, { status: 401 });
            }
            // Error de red/timeout/5xx → transitorio
            return NextResponse.json(
                { valido: false, transitorio: true },
                { status: 503 },
            );
        }
    } catch {
        return NextResponse.json(
            { valido: false, transitorio: true },
            { status: 503 },
        );
    }

    return NextResponse.json({ valido: true }, { status: 200 });
}
