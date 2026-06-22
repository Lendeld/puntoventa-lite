import { http, HttpResponse } from 'msw';

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

export const handlers = [
  http.post(`${API_BASE}/auth/login`, () => {
    return HttpResponse.json({
      requiresOtp: false,
      accessToken: 'mock-jwt-token',
      accessTokenExpiracionUtc: new Date(Date.now() + 15 * 60 * 1000).toISOString(),
      refreshToken: 'mock-refresh-token',
      refreshTokenExpiracionUtc: new Date(Date.now() + 30 * 24 * 3600 * 1000).toISOString(),
    });
  }),

  http.post(`${API_BASE}/auth/refresh`, () => {
    return HttpResponse.json({
      accessToken: 'mock-jwt-token-refresh',
      accessTokenExpiracionUtc: new Date(Date.now() + 15 * 60 * 1000).toISOString(),
      refreshToken: 'mock-refresh-token-2',
      refreshTokenExpiracionUtc: new Date(Date.now() + 30 * 24 * 3600 * 1000).toISOString(),
    });
  }),

  http.post(`${API_BASE}/auth/logout`, () => {
    return new HttpResponse(null, { status: 204 });
  }),
];
