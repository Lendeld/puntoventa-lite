import { describe, expect, it } from 'vitest';
import { NextRequest } from 'next/server';
import { proxy } from '@/proxy';

function makeRequest(url: string, opts?: { cookie?: string; action?: string; ip?: string }) {
    const headers: Record<string, string> = {};
    if (opts?.cookie) headers['cookie'] = opts.cookie;
    if (opts?.action) headers['next-action'] = opts.action;
    if (opts?.ip) headers['x-forwarded-for'] = opts.ip;
    return new NextRequest(url, { headers });
}

describe('proxy', () => {
    it('redirige a login cuando no hay cookie de sesion en una ruta protegida', async () => {
        const request = new NextRequest('http://localhost/');

        const response = await proxy(request);

        expect(response.status).toBe(307);
        expect(response.headers.get('location')).toBe('http://localhost/login');
    });

    it('redirige a home cuando hay cookie de sesion y se intenta abrir login', async () => {
        const request = new NextRequest('http://localhost/login', {
            headers: {
                cookie: 'PVID=valor',
            },
        });

        const response = await proxy(request);

        expect(response.status).toBe(307);
        expect(response.headers.get('location')).toBe('http://localhost/');
    });

    it('no redirige los server actions aunque exista cookie de sesion en login', async () => {
        const request = new NextRequest('http://localhost/login', {
            method: 'POST',
            headers: {
                cookie: 'PVID=valor',
                'next-action': 'accion-servidor',
            },
        });

        const response = await proxy(request);

        expect(response.status).toBe(200);
        expect(response.headers.get('location')).toBeNull();
    });

    it('permite rutas publicas de auth sin cookie', async () => {
        const request = new NextRequest('http://localhost/cambiar-password');

        const response = await proxy(request);

        expect(response.status).toBe(200);
        expect(response.headers.get('location')).toBeNull();
    });

    it('devuelve 429 al superar el rate limit en ruta publica', async () => {
        const ip = '10.0.0.1';

        const responses = await Promise.all(
            Array.from({ length: 10 }, () =>
                proxy(makeRequest('http://localhost/login', { ip })),
            ),
        );
        for (const res of responses) {
            expect(res.status).toBe(200);
        }

        const res = await proxy(makeRequest('http://localhost/login', { ip }));
        expect(res.status).toBe(429);
        expect(Number(res.headers.get('Retry-After'))).toBeGreaterThan(0);
        expect(res.headers.get('X-RateLimit-Limit')).toBe('10');
    });

    it('rate limit aplica tambien a server actions en rutas publicas', async () => {
        const ip = '10.0.0.2';

        await Promise.all(
            Array.from({ length: 10 }, () =>
                proxy(makeRequest('http://localhost/login', { ip })),
            ),
        );

        const res = await proxy(makeRequest('http://localhost/login', { ip, action: 'login-action' }));
        expect(res.status).toBe(429);
    });

    it('IPs distintas no comparten rate limit', async () => {
        const ip1 = '10.1.0.1';
        const ip2 = '10.1.0.2';

        await Promise.all(
            Array.from({ length: 10 }, () =>
                proxy(makeRequest('http://localhost/login', { ip: ip1 })),
            ),
        );

        const res = await proxy(makeRequest('http://localhost/login', { ip: ip2 }));
        expect(res.status).toBe(200);
    });
});
