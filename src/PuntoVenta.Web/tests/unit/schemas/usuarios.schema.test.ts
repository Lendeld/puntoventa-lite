import { describe, it, expect } from 'vitest';
import { actualizarUsuarioSchema, crearUsuarioSchema } from '@lib/schemas/usuarios.schema';
import { USUARIO_FIELDS } from '@lib/constants/usuarios.constants';

describe('crearUsuarioSchema', () => {
  const valido = {
    [USUARIO_FIELDS.NOMBRE_USUARIO]: 'juan.perez',
    [USUARIO_FIELDS.NOMBRE]: 'Juan Pérez',
    [USUARIO_FIELDS.IDENTIFICACION]: '102340001',
    [USUARIO_FIELDS.PASSWORD]: 'Temporal123!',
    [USUARIO_FIELDS.ROL_ID]: '',
    [USUARIO_FIELDS.CORREO]: '',
    [USUARIO_FIELDS.TELEFONO]: '',
  };

  it('valida campos correctos sin campos opcionales', () => {
    expect(crearUsuarioSchema.safeParse(valido).success).toBe(true);
  });

  it('falla si nombreUsuario vacío', () => {
    const result = crearUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.NOMBRE_USUARIO]: '',
    });
    expect(result.success).toBe(false);
  });

  it('falla si nombreUsuario supera 50 caracteres', () => {
    const result = crearUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.NOMBRE_USUARIO]: 'a'.repeat(51),
    });
    expect(result.success).toBe(false);
  });

  it('falla si nombre vacío', () => {
    const result = crearUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.NOMBRE]: '',
    });
    expect(result.success).toBe(false);
  });

  it('acepta identificacion vacía (opcional)', () => {
    expect(crearUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.IDENTIFICACION]: '',
    }).success).toBe(true);
  });

  it('falla si password no cumple el formato fuerte', () => {
    const result = crearUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.PASSWORD]: '123',
    });
    expect(result.success).toBe(false);
  });

  it('falla si password sin símbolo', () => {
    const result = crearUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.PASSWORD]: 'Temporal123',
    });
    expect(result.success).toBe(false);
  });

  it('falla si correo tiene formato inválido', () => {
    const result = crearUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.CORREO]: 'no-es-correo',
    });
    expect(result.success).toBe(false);
  });

  it('acepta correo vacío (opcional)', () => {
    expect(crearUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.CORREO]: '',
    }).success).toBe(true);
  });

  it('acepta rolId vacío (opcional)', () => {
    expect(crearUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.ROL_ID]: '',
    }).success).toBe(true);
  });

  it('falla si rolId tiene valor pero no es UUID', () => {
    const result = crearUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.ROL_ID]: 'no-es-uuid',
    });
    expect(result.success).toBe(false);
  });
});

describe('actualizarUsuarioSchema', () => {
  const valido = {
    [USUARIO_FIELDS.ROL_ID]: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    [USUARIO_FIELDS.ACTIVO]: true,
  };

  it('valida campos correctos', () => {
    expect(actualizarUsuarioSchema.safeParse(valido).success).toBe(true);
  });

  it('falla si rolId vacío', () => {
    const result = actualizarUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.ROL_ID]: '',
    });
    expect(result.success).toBe(false);
  });

  it('falla si rolId no es UUID', () => {
    const result = actualizarUsuarioSchema.safeParse({
      ...valido,
      [USUARIO_FIELDS.ROL_ID]: 'no-es-uuid',
    });
    expect(result.success).toBe(false);
  });
});
