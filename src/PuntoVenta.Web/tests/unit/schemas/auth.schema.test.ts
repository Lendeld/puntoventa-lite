import { describe, it, expect } from 'vitest';
import { cambiarPasswordSchema, loginSchema } from '@lib/schemas/auth.schema';
import { CAMBIAR_PASSWORD_FIELDS, LOGIN_FIELDS } from '@lib/constants/auth.constants';

describe('loginSchema', () => {
  const valido = {
    [LOGIN_FIELDS.NOMBRE_USUARIO]: 'admin',
    [LOGIN_FIELDS.PASSWORD]: 'secreto',
  };

  it('valida campos correctos', () => {
    expect(loginSchema.safeParse(valido).success).toBe(true);
  });

  it('falla si usuario vacío', () => {
    const result = loginSchema.safeParse({ ...valido, [LOGIN_FIELDS.NOMBRE_USUARIO]: '' });
    expect(result.success).toBe(false);
  });

  it('falla si password vacío', () => {
    const result = loginSchema.safeParse({ ...valido, [LOGIN_FIELDS.PASSWORD]: '' });
    expect(result.success).toBe(false);
  });

  it('falla si usuario supera 50 chars', () => {
    const result = loginSchema.safeParse({
      ...valido,
      [LOGIN_FIELDS.NOMBRE_USUARIO]: 'a'.repeat(51),
    });
    expect(result.success).toBe(false);
  });
});

describe('cambiarPasswordSchema', () => {
  const valido = {
    [CAMBIAR_PASSWORD_FIELDS.PASSWORD_ACTUAL]: 'Actual123!',
    [CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA]: 'Nueva456@',
    [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: 'Nueva456@',
  };

  it('valida campos correctos', () => {
    expect(cambiarPasswordSchema.safeParse(valido).success).toBe(true);
  });

  it('falla si password actual vacía', () => {
    const result = cambiarPasswordSchema.safeParse({
      ...valido,
      [CAMBIAR_PASSWORD_FIELDS.PASSWORD_ACTUAL]: '',
    });
    expect(result.success).toBe(false);
  });

  it('falla si password nueva vacía', () => {
    const result = cambiarPasswordSchema.safeParse({
      ...valido,
      [CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA]: '',
      [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: '',
    });
    expect(result.success).toBe(false);
  });

  it('falla si password nueva tiene menos de 8 caracteres', () => {
    const result = cambiarPasswordSchema.safeParse({
      ...valido,
      [CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA]: 'Ab1!',
      [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: 'Ab1!',
    });
    expect(result.success).toBe(false);
  });

  it('falla si password nueva sin mayúscula', () => {
    const result = cambiarPasswordSchema.safeParse({
      ...valido,
      [CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA]: 'nueva123!',
      [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: 'nueva123!',
    });
    expect(result.success).toBe(false);
  });

  it('falla si password nueva sin minúscula', () => {
    const result = cambiarPasswordSchema.safeParse({
      ...valido,
      [CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA]: 'NUEVA123!',
      [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: 'NUEVA123!',
    });
    expect(result.success).toBe(false);
  });

  it('falla si password nueva sin número', () => {
    const result = cambiarPasswordSchema.safeParse({
      ...valido,
      [CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA]: 'NuevaPass!',
      [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: 'NuevaPass!',
    });
    expect(result.success).toBe(false);
  });

  it('falla si password nueva sin símbolo especial', () => {
    const result = cambiarPasswordSchema.safeParse({
      ...valido,
      [CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA]: 'NuevaPass1',
      [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: 'NuevaPass1',
    });
    expect(result.success).toBe(false);
  });

  it('falla si contraseñas no coinciden', () => {
    const result = cambiarPasswordSchema.safeParse({
      ...valido,
      [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: 'Diferente1!',
    });
    expect(result.success).toBe(false);
  });
});
