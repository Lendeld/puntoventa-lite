import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  actualizarUsuarioService,
  obtenerUsuarioPorIdService,
  obtenerUsuariosService,
} from '@lib/services/usuarios.service';
import { requestAPI } from '@lib/utils/requestApi';

vi.mock('@lib/utils/requestApi', () => ({
  requestAPI: vi.fn(),
}));

const requestAPIMock = vi.mocked(requestAPI);

describe('usuarios.service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    requestAPIMock.mockResolvedValue({ data: null, errors: undefined });
  });

  it('obtenerUsuariosService manda query paginada', async () => {
    await obtenerUsuariosService({
      numeroPagina: 3,
      tamanoPagina: 25,
      filtroDinamico: 'juan',
      activo: true,
    });

    expect(requestAPIMock).toHaveBeenCalledWith({
      url: '/usuarios',
      method: 'GET',
      query: {
        NumeroPagina: 3,
        TamanoPagina: 25,
        FiltroDinamico: 'juan',
        Activo: true,
      },
    });
  });

  it('obtenerUsuarioPorIdService pide detalle por id', async () => {
    await obtenerUsuarioPorIdService('abc-123');

    expect(requestAPIMock).toHaveBeenCalledWith({
      url: '/usuarios/abc-123',
      method: 'GET',
    });
  });

  it('actualizarUsuarioService usa ruta PUT con id', async () => {
    await actualizarUsuarioService({
      id: 'user-1',
      activo: false,
      rolId: 'rol-id',
    });

    expect(requestAPIMock).toHaveBeenCalledWith({
      url: '/usuarios/user-1',
      method: 'PUT',
      body: {
        activo: false,
        rolId: 'rol-id',
      },
    });
  });
});
