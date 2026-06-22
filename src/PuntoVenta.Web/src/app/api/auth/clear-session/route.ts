import { NextResponse } from 'next/server';
import { destruirSesion, obtenerSesion } from '@lib/auth/sesion';
import { logoutService } from '@lib/services/auth.service';

export async function GET(request: Request) {
    const sesion = await obtenerSesion();
    if (sesion.refreshToken || sesion.accessToken) {
        try {
            await logoutService(sesion.refreshToken);
        } catch {
            // If the API is unavailable, we still need to clear the local session.
        }
    }

    await destruirSesion();
    const host = request.headers.get('x-forwarded-host') ?? request.headers.get('host') ?? new URL(request.url).host;
    const proto = request.headers.get('x-forwarded-proto') ?? 'https';
    return NextResponse.redirect(new URL('/login', `${proto}://${host}`));
}
