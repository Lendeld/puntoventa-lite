import { describe, it, expect } from 'vitest';
import { z } from 'zod';
import { zodResolver } from '@lib/utils/zodResolver';

describe('zodResolver', () => {
  const schema = z.object({
    nombre: z.string().min(1, 'Requerido').max(10, 'Máximo 10'),
    email: z.string().email('Email inválido'),
  });

  const resolver = zodResolver(schema);

  it('retorna {} si válido', () => {
    const result = resolver({ nombre: 'test', email: 'a@b.com' });
    expect(result).toEqual({});
  });

  it('retorna error por campo faltante', () => {
    const result = resolver({ nombre: '', email: 'a@b.com' });
    expect(result.nombre).toBe('Requerido');
  });

  it('retorna error de email inválido', () => {
    const result = resolver({ nombre: 'test', email: 'no-email' });
    expect(result.email).toBe('Email inválido');
  });

  it('retorna múltiples errores independientes', () => {
    const result = resolver({ nombre: '', email: 'no-email' });
    expect(result.nombre).toBe('Requerido');
    expect(result.email).toBe('Email inválido');
  });

  it('retorna primer error por campo (no duplica)', () => {
    const result = resolver({ nombre: 'x'.repeat(11), email: 'a@b.com' });
    expect(result.nombre).toBe('Máximo 10');
  });
});
