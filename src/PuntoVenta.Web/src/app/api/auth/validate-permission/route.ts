import { NextRequest, NextResponse } from "next/server";
import { validarPermisoService } from "@lib/services/auth.service";
import { asegurarAccessToken } from "@lib/utils/apiClient";

export async function GET(request: NextRequest) {
    const estado = await asegurarAccessToken();

    if (estado === "no-auth") {
        return NextResponse.json({ tienePermiso: false }, { status: 401 });
    }

    if (estado === "transitorio") {
        return NextResponse.json({ tienePermiso: false }, { status: 503 });
    }

    const clave = request.nextUrl.searchParams.get("permiso")?.trim();

    if (!clave) {
        return NextResponse.json({ tienePermiso: false }, { status: 200 });
    }

    const response = await validarPermisoService(clave);

    if (response.errors) {
        if (response.errors.status === 401) {
            return NextResponse.json({ tienePermiso: false }, { status: 401 });
        }

        return NextResponse.json({ tienePermiso: false }, { status: 200 });
    }

    return NextResponse.json({ tienePermiso: true }, { status: 200 });
}
