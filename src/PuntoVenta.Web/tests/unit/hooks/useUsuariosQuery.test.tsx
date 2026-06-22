import { PropsWithChildren } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { QUERY_KEYS } from '@lib/constants/queryKeys.constants';
import { useUsuariosQuery } from '@lib/hooks/useUsuariosQuery';
import { obtenerUsuariosService } from '@lib/services/usuarios.service';

vi.mock('@lib/services/usuarios.service', () => ({
  obtenerUsuariosService: vi.fn(),
}));

const obtenerUsuariosServiceMock = vi.mocked(obtenerUsuariosService);

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  return {
    queryClient,
    wrapper: ({ children }: PropsWithChildren) => (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    ),
  };
}

describe('useUsuariosQuery', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('retorna lista cuando servicio responde OK', async () => {
    const params = {
      numeroPagina: 1,
      tamanoPagina: 15,
      filtroDinamico: 'juan',
      activo: true,
    };
    const data = {
      items: [
        {
          id: '1',
          nombreUsuario: 'juan',
          nombre: 'Juan Perez',
          identificacion: '123',
          negocioId: 'negocio-1',
          rolId: null,
          rolNombre: null,
          esPropietario: false,
          correo: null,
          telefono: null,
          debeCambiarPassword: false,
          activo: true,
          fechaCreacion: '2026-01-01',
          fechaModificacion: null,
          creadoPor: 'admin',
          modificadoPor: null,
        },
      ],
      pagina: 1,
      tamano: 15,
      totalRegistros: 1,
      totalPaginas: 1,
    };

    obtenerUsuariosServiceMock.mockResolvedValue({
      data,
      errors: undefined,
    });

    const { wrapper, queryClient } = createWrapper();
    const { result } = renderHook(() => useUsuariosQuery(params), { wrapper });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(data);
    expect(obtenerUsuariosServiceMock).toHaveBeenCalledWith(params);
    expect(queryClient.getQueryData(QUERY_KEYS.usuarios.lista(params))).toEqual(
      data,
    );
  });

  it('marca error cuando servicio retorna problem details', async () => {
    const params = {
      numeroPagina: 1,
      tamanoPagina: 15,
      filtroDinamico: undefined,
      activo: true,
    };
    const problem = {
      status: 500,
      title: 'Boom',
      errors: { general: 'fallo' },
    };

    obtenerUsuariosServiceMock.mockResolvedValue({
      data: null,
      errors: problem,
    });

    const { wrapper } = createWrapper();
    const { result } = renderHook(() => useUsuariosQuery(params), { wrapper });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toEqual(problem);
  });
});
