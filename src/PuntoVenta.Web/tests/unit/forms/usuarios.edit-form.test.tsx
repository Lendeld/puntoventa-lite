import { PropsWithChildren } from 'react';
import { MantineProvider } from '@mantine/core';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import userEvent from '@testing-library/user-event';
import { render, screen, waitFor } from '../../utils/render';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import EditUsuarioForm from '@/app/(root)/usuarios/edit/EditUsuarioForm';
import { QUERY_KEYS } from '@lib/constants/queryKeys.constants';
import { USUARIO_FIELDS } from '@lib/constants/usuarios.constants';
import { actualizarUsuarioAction } from '@lib/actions/usuarios.actions';
import { obtenerUsuarioPorIdService } from '@lib/services/usuarios.service';
import { obtenerUsuarioActualService } from '@lib/services/auth.service';
import { AppNotifier } from '@components/ui/AppNotifier';
import { modals } from '@mantine/modals';

vi.mock('@/components/ui/selects/RoleSelect', () => ({
  RoleSelect: ({ label, value, onChange, disabled }: {
    label: string;
    value: string | null;
    onChange: (value: string | null) => void;
    disabled?: boolean;
  }) => (
    <label>
      {label}
      <select
        aria-label={label}
        value={value ?? ''}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value || null)}
      >
        <option value="">Sin rol</option>
        <option value="11111111-1111-4111-8111-111111111111">Administrador</option>
      </select>
    </label>
  ),
}));

vi.mock('@lib/actions/usuarios.actions', () => ({
  actualizarUsuarioAction: vi.fn(),
}));

vi.mock('@lib/services/usuarios.service', () => ({
  obtenerUsuarioPorIdService: vi.fn(),
}));

vi.mock('@lib/services/auth.service', () => ({
  obtenerUsuarioActualService: vi.fn(),
}));

vi.mock('@components/ui/AppNotifier', () => ({
  AppNotifier: {
    success: vi.fn(),
  },
}));

vi.mock('@mantine/modals', () => ({
  modals: {
    closeAll: vi.fn(),
  },
}));

const actualizarUsuarioActionMock = vi.mocked(actualizarUsuarioAction);
const obtenerUsuarioPorIdServiceMock = vi.mocked(obtenerUsuarioPorIdService);
const obtenerUsuarioActualServiceMock = vi.mocked(obtenerUsuarioActualService);

function mockUsuarioActual(usuario: string) {
  obtenerUsuarioActualServiceMock.mockResolvedValue({
    // Solo se usa `usuario` (username) para saber si edito mi propia cuenta.
    data: { usuario } as never,
    errors: undefined,
  });
}
const successMock = vi.mocked(AppNotifier.success);
const closeAllMock = vi.mocked(modals.closeAll);

function renderWithQueryClient(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');
  const wrapper = ({ children }: PropsWithChildren) => (
    <MantineProvider>
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    </MantineProvider>
  );

  return {
    invalidateSpy,
    ...render(ui, { wrapper }),
  };
}

describe('EditUsuarioForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Por defecto el usuario actual es otro (no el editado).
    mockUsuarioActual('alguien.mas');
  });

  it('hidrata usuario, habilita submit al editar, guarda cambios', async () => {
    obtenerUsuarioPorIdServiceMock.mockResolvedValue({
      data: {
        id: 'user-1',
        nombreUsuario: 'juan.user',
        nombre: 'Juan Perez',
        identificacion: '123456',
        negocioId: 'negocio-1',
        rolId: '11111111-1111-4111-8111-111111111111',
        rolNombre: 'Administrador',
        esPropietario: false,
        correo: 'juan@demo.com',
        telefono: '8888-9999',
        activo: true,
        fechaCreacion: '2026-01-01',
        fechaModificacion: null,
        creadoPor: 'admin',
        modificadoPor: null,
      },
      errors: undefined,
    });
    actualizarUsuarioActionMock.mockResolvedValue({
      status: 204,
      errors: undefined,
    });

    const { invalidateSpy } = renderWithQueryClient(<EditUsuarioForm id="user-1" />);

    await waitFor(() =>
      expect(screen.getByText('@juan.user')).toBeInTheDocument(),
    );

    const submit = screen.getByRole('button', { name: 'Guardar cambios' });
    expect(submit).toBeDisabled();

    const user = userEvent.setup();
    await user.selectOptions(screen.getByLabelText('Rol'), '11111111-1111-4111-8111-111111111111');
    await user.click(screen.getByLabelText('Usuario activo'));
    await user.click(submit);

    await waitFor(() =>
      expect(actualizarUsuarioActionMock).toHaveBeenCalledWith('user-1', {
        [USUARIO_FIELDS.ROL_ID]: '11111111-1111-4111-8111-111111111111',
        [USUARIO_FIELDS.ACTIVO]: false,
      }),
    );

    await waitFor(() =>
      expect(invalidateSpy).toHaveBeenCalledWith({
        queryKey: QUERY_KEYS.usuarios.all,
      }),
    );
    expect(successMock).toHaveBeenCalledWith({
      message: 'Usuario actualizado exitosamente.',
    });
    expect(closeAllMock).toHaveBeenCalled();
  });

  it('muestra error cuando detalle usuario falla', async () => {
    obtenerUsuarioPorIdServiceMock.mockResolvedValue({
      data: null,
      errors: {
        status: 404,
        title: 'No encontrado',
        errors: undefined,
      },
    });

    renderWithQueryClient(<EditUsuarioForm id="user-404" />);

    await waitFor(() =>
      expect(
        screen.getByText('Error al cargar datos del usuario.'),
      ).toBeInTheDocument(),
    );
    expect(actualizarUsuarioActionMock).not.toHaveBeenCalled();
  });

  function mockPropietario() {
    obtenerUsuarioPorIdServiceMock.mockResolvedValue({
      data: {
        id: 'owner-1',
        nombreUsuario: 'owner.user',
        nombre: 'Owner User',
        identificacion: '789456',
        negocioId: 'negocio-1',
        rolId: '11111111-1111-4111-8111-111111111111',
        rolNombre: 'Administrador',
        esPropietario: true,
        correo: 'owner@demo.com',
        telefono: '8888-9999',
        activo: true,
        fechaCreacion: '2026-01-01',
        fechaModificacion: null,
        creadoPor: 'admin',
        modificadoPor: null,
      },
      errors: undefined,
    });
  }

  it('otro usuario ve al propietario en solo lectura (rol y activo bloqueados)', async () => {
    mockPropietario();
    mockUsuarioActual('alguien.mas');

    renderWithQueryClient(<EditUsuarioForm id="owner-1" />);

    await waitFor(() =>
      expect(screen.getByText('@owner.user')).toBeInTheDocument(),
    );

    expect(screen.getByLabelText('Rol')).toBeDisabled();
    expect(screen.getByLabelText('Usuario activo')).toBeDisabled();
    expect(screen.getByText('Propietario')).toBeInTheDocument();
    expect(
      screen.getByText(/Solo el propietario puede modificarla/),
    ).toBeInTheDocument();
  });

  it('el propio propietario tiene rol y activo bloqueados (no se autodesactiva)', async () => {
    mockPropietario();
    mockUsuarioActual('owner.user');

    renderWithQueryClient(<EditUsuarioForm id="owner-1" />);

    await waitFor(() =>
      expect(screen.getByText('@owner.user')).toBeInTheDocument(),
    );

    expect(screen.getByLabelText('Rol')).toBeDisabled();
    expect(screen.getByLabelText('Usuario activo')).toBeDisabled();
    // No es "ajeno": no aparece la alerta de solo lectura.
    expect(
      screen.queryByText(/Solo el propietario puede modificarla/),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText('El propietario no puede desactivar su propia cuenta.'),
    ).toBeInTheDocument();
  });

  it('muestra datos globales en solo lectura', async () => {
    obtenerUsuarioPorIdServiceMock.mockResolvedValue({
      data: {
        id: 'user-2',
        nombreUsuario: 'readonly.user',
        nombre: 'Readonly User',
        identificacion: '123123',
        negocioId: 'negocio-1',
        rolId: '11111111-1111-4111-8111-111111111111',
        rolNombre: 'Administrador',
        esPropietario: false,
        correo: 'readonly@demo.com',
        telefono: '8888-9999',
        activo: true,
        fechaCreacion: '2026-01-01',
        fechaModificacion: null,
        creadoPor: 'admin',
        modificadoPor: null,
      },
      errors: undefined,
    });

    renderWithQueryClient(<EditUsuarioForm id="user-2" />);

    await waitFor(() =>
      expect(screen.getByText('@readonly.user')).toBeInTheDocument(),
    );

    expect(screen.getByText('Readonly User')).toBeInTheDocument();
    expect(screen.getByText('readonly@demo.com')).toBeInTheDocument();
    expect(screen.getByText('8888-9999')).toBeInTheDocument();
    expect(screen.getByText('123123')).toBeInTheDocument();
  });
});
