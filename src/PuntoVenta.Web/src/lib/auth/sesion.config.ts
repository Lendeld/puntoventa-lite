import type { SessionOptions } from 'iron-session';
import type { SesionData } from '@/lib/types/session.types';

const sessionSecret = process.env.SESSION_SECRET;
if (process.env.NODE_ENV === 'production' && !process.env.PUNTO_VENTA_WEB_DEV_ENV_PATH && (!sessionSecret || sessionSecret.length < 32)) {
  throw new Error('SESSION_SECRET no definido o menor a 32 caracteres');
}
const secret = sessionSecret ?? 'dev-secret-local-no-usar-en-produccion!!!';

export const sesionOptions: SessionOptions = {
  cookieName: 'PVID',
  password: secret,
  // ttl 0 = cookie sin expiración práctica (iron-session usa max-age máximo).
  // La sesión real la gobierna el refresh token sliding del backend, que es
  // revocable — la cookie solo transporta los tokens cifrados.
  ttl: 0,
  cookieOptions: {
    secure: process.env.NODE_ENV === 'production',
    httpOnly: true,
    sameSite: 'lax',
  },
};

export type { SesionData };
