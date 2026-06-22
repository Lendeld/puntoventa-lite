import { NextRequest, NextResponse } from 'next/server';

const RUTAS_PUBLICAS = ['/login', '/cambiar-password'];
const RUTAS_AUTH_FLOW = ['/cambiar-password'];
const COOKIE_SESION = 'PVID';

const RATE_LIMIT_WINDOW_MS = 60_000;
const RATE_LIMIT_MAX = 10;
const rateLimitStore = new Map<string, { count: number; windowStart: number }>();

function checkRateLimit(ip: string): { allowed: boolean; retryAfter: number } {
    const now = Date.now();
    const entry = rateLimitStore.get(ip);

    if (!entry || now - entry.windowStart > RATE_LIMIT_WINDOW_MS) {
        rateLimitStore.set(ip, { count: 1, windowStart: now });
        return { allowed: true, retryAfter: 0 };
    }

    if (entry.count >= RATE_LIMIT_MAX) {
        const retryAfter = Math.ceil((RATE_LIMIT_WINDOW_MS - (now - entry.windowStart)) / 1000);
        return { allowed: false, retryAfter };
    }

    entry.count++;
    return { allowed: true, retryAfter: 0 };
}

export async function proxy(request: NextRequest) {
    const { pathname } = request.nextUrl;

    // Lockdown app-only: si ELECTRON_CLIENT_TOKEN esta seteado, el
    // request debe traer X-Electron-Token con el mismo valor. Electron
    // BrowserWindow inyecta el header via webRequest hook. Browser
    // externo accediendo al puerto local no lo trae -> 401.
    const tokenEsperado = process.env.ELECTRON_CLIENT_TOKEN;
    if (tokenEsperado && request.headers.get('x-electron-token') !== tokenEsperado) {
        return new NextResponse('Solo accesible desde la app', { status: 401 });
    }

    const esServerAction = request.headers.has('next-action');
    const esPublica = RUTAS_PUBLICAS.some((ruta) => pathname.startsWith(ruta));
    const tieneSesion = request.cookies.has(COOKIE_SESION);

    if (esPublica) {
        const ip =
            request.headers.get('x-forwarded-for')?.split(',')[0].trim() ??
            request.headers.get('x-real-ip') ??
            '127.0.0.1';
        const { allowed, retryAfter } = checkRateLimit(ip);
        if (!allowed) {
            return new NextResponse('Demasiadas solicitudes', {
                status: 429,
                headers: {
                    'Retry-After': String(retryAfter),
                    'X-RateLimit-Limit': String(RATE_LIMIT_MAX),
                    'Content-Type': 'text/plain; charset=utf-8',
                },
            });
        }
    }

    if (esServerAction) {
        return NextResponse.next();
    }

    // Sin sesión intentando acceder a ruta protegida → login
    if (!esPublica && !tieneSesion) {
        return NextResponse.redirect(new URL('/login', request.url));
    }

    // Con cookie de sesión intentando acceder a login -> home
    if (pathname === '/login' && tieneSesion) {
        return NextResponse.redirect(new URL('/', request.url));
    }

    const response = NextResponse.next();
    const esAuthFlow = RUTAS_AUTH_FLOW.some((ruta) => pathname.startsWith(ruta));
    if (pathname === '/login' || esAuthFlow || !esPublica) {
        response.headers.set('Cache-Control', 'no-store');
    }
    return response;
}

export const config = {
    // Excluye assets estáticos del `public/` (icons, logos, fuentes, etc.) por
    // extensión. Sin esto, un GET a `/icons/logo.svg` cae en el guard de sesión
    // y redirige a /login (307); el optimizador de `next/image` hace un fetch
    // interno del origen, recibe ese redirect en vez de una imagen, y responde
    // 400 "The requested resource isn't a valid image".
    matcher: [
        '/((?!api|pdf|_next/static|_next/image|favicon.ico|.*\\.(?:png|jpe?g|gif|svg|webp|avif|ico|bmp|woff2?|ttf|otf|eot)$).*)',
    ],
};
