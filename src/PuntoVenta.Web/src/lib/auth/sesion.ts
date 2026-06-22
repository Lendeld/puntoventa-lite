'use server';

import { getIronSession } from 'iron-session';
import { cookies } from 'next/headers';
import { sesionOptions } from './sesion.config';
import type { SesionData } from '@/lib/types/session.types';
import type { AuthTokensDto } from '@/lib/types/auth.types';

export async function obtenerSesion() {
    const cookieStore = await cookies();
    return getIronSession<SesionData>(cookieStore, sesionOptions);
}

export async function guardarTokensSesion(data: AuthTokensDto) {
    const sesion = await obtenerSesion();
    await guardarTokensSesionContexto(data, {
        requiereCambioPassword: sesion.requiereCambioPassword,
    });
}

export async function guardarTokensSesionContexto(
    data: AuthTokensDto,
    options?: {
        requiereCambioPassword?: boolean;
    },
) {
    const sesion = await obtenerSesion();
    sesion.accessToken = data.accessToken;
    sesion.accessTokenExpiracionUtc = data.accessTokenExpiracionUtc;
    sesion.refreshToken = data.refreshToken;
    sesion.refreshTokenExpiracionUtc = data.refreshTokenExpiracionUtc;
    sesion.requiereCambioPassword = options?.requiereCambioPassword ?? false;
    await sesion.save();
}

export async function destruirSesion() {
    const sesion = await obtenerSesion();
    sesion.destroy();
}
