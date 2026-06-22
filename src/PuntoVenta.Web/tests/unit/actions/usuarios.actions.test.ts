import { beforeEach, describe, expect, it, vi } from 'vitest';
import { actualizarUsuarioAction, crearUsuarioAction } from '@lib/actions/usuarios.actions';
import { USUARIO_FIELDS } from '@lib/constants/usuarios.constants';
import { actualizarUsuarioService, crearUsuarioService } from '@lib/services/usuarios.service';

vi.mock('@lib/services/usuarios.service', () => ({
  actualizarUsuarioService: vi.fn(),
  crearUsuarioService: vi.fn(),
  toggleEstadoUsuarioService: vi.fn(),
}));

const actualizarUsuarioServiceMock = vi.mocked(actualizarUsuarioService);
const crearUsuarioServiceMock = vi.mocked(crearUsuarioService);

const crearValues = {
  [USUARIO_FIELDS.NOMBRE_USUARIO]: 'ana.garcia',
  [USUARIO_FIELDS.NOMBRE]: 'Ana García',
  [USUARIO_FIELDS.IDENTIFICACION]: '109990001',
  [USUARIO_FIELDS.PASSWORD]: 'TempPass1!',
  [USUARIO_FIELDS.ROL_ID]: '',
  [USUARIO_FIELDS.CORREO]: '',
  [USUARIO_FIELDS.TELEFONO]: '',
};

const actualizarValues = {
  [USUARIO_FIELDS.ROL_ID]: '22222222-2222-4222-8222-222222222222',
  [USUARIO_FIELDS.ACTIVO]: false,
};

describe('usuarios.actions', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('crearUsuarioAction', () => {
    it('llama al servicio con campos mínimos y retorna 201', async () => {
      crearUsuarioServiceMock.mockResolvedValue({ data: 'nuevo-id', errors: undefined });

      const result = await crearUsuarioAction(crearValues);

      expect(crearUsuarioServiceMock).toHaveBeenCalledWith(
        expect.objectContaining({
          nombreUsuario: 'ana.garcia',
          nombre: 'Ana García',
          identificacion: '109990001',
          password: 'TempPass1!',
        }),
      );
      expect(result.status).toBe(201);
      expect(result.errors).toBeUndefined();
    });

    it('falla validación zod sin llamar al servicio si password corto', async () => {
      const result = await crearUsuarioAction({
        ...crearValues,
        [USUARIO_FIELDS.PASSWORD]: '123',
      });

      expect(crearUsuarioServiceMock).not.toHaveBeenCalled();
      expect(result.status).toBe(400);
    });

    it('propaga error del servicio', async () => {
      crearUsuarioServiceMock.mockResolvedValue({
        data: null,
        errors: {
          title: 'Conflict',
          status: 409,
          errors: { [USUARIO_FIELDS.NOMBRE_USUARIO]: 'El nombre de usuario ya existe.' },
        },
      });

      const result = await crearUsuarioAction(crearValues);

      expect(result.status).toBe(409);
    });
  });

  it('actualizarUsuarioAction arma payload correcto, retorna 204', async () => {
    actualizarUsuarioServiceMock.mockResolvedValue({
      data: null,
      errors: undefined,
    });

    const result = await actualizarUsuarioAction(
      'usuario-id',
      actualizarValues,
    );

    expect(actualizarUsuarioServiceMock).toHaveBeenCalledWith({
      id: 'usuario-id',
      activo: false,
      rolId: '22222222-2222-4222-8222-222222222222',
    });
    expect(result).toEqual({
      status: 204,
      errors: undefined,
    });
  });

  it('actualizarUsuarioAction frena req invalida antes servicio', async () => {
    const result = await actualizarUsuarioAction('usuario-id', {
      ...actualizarValues,
      [USUARIO_FIELDS.ROL_ID]: '',
    });

    expect(actualizarUsuarioServiceMock).not.toHaveBeenCalled();
    expect(result.status).toBe(400);
    expect(result.errors?.[USUARIO_FIELDS.ROL_ID]).toBe(
      'El rol es requerido.',
    );
  });
});
