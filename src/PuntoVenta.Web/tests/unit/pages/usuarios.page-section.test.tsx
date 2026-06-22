import userEvent from '@testing-library/user-event';
import { render, screen } from '../../utils/render';
import { afterEach, describe, expect, it, vi } from 'vitest';
import UsuariosPageSection from '@/app/(root)/usuarios/UsuariosPageSection';

const useUsuariosQueryMock = vi.fn();
const refetchMock = vi.fn();

vi.mock('@lib/hooks/useUsuariosQuery', () => ({
  useUsuariosQuery: (params: unknown) => useUsuariosQueryMock(params),
}));

vi.mock('@ui/table/DynamicSearchInput', () => ({
  DynamicSearchInput: ({ value, onChange, placeholder }: {
    value: string;
    onChange: (value: string) => void;
    placeholder: string;
  }) => (
    <input
      aria-label="Buscar usuario"
      placeholder={placeholder}
      value={value}
      onChange={(event) => onChange(event.target.value)}
    />
  ),
}));

vi.mock('@components/ui/table/StatusSegment', () => ({
  StatusSegment: ({ value, onChange }: {
    value: string;
    onChange: (value: 'todos' | 'activos' | 'inactivos') => void;
  }) => (
    <select
      aria-label="Estado"
      value={value}
      onChange={(event) => onChange(event.target.value as 'todos' | 'activos' | 'inactivos')}
    >
      <option value="activos">Activos</option>
      <option value="inactivos">Inactivos</option>
      <option value="todos">Todos</option>
    </select>
  ),
}));

vi.mock('@ui/table/TableRefreshButton', () => ({
  TableRefreshButton: ({ onClick }: { onClick: () => void }) => (
    <button type="button" onClick={onClick}>
      Refrescar
    </button>
  ),
}));

vi.mock('@ui/table/TablePagination', () => ({
  TablePagination: ({
    onPageChange,
    onPageSizeChange,
  }: {
    onPageChange: (page: number) => void;
    onPageSizeChange: (size: number) => void;
  }) => (
    <>
      <button type="button" onClick={() => onPageChange(4)}>
        Ir pagina 4
      </button>
      <button type="button" onClick={() => onPageSizeChange(50)}>
        Tamano 50
      </button>
    </>
  ),
}));

vi.mock('@ui/table/DataTable', () => ({
  DataTable: () => <div>tabla usuarios</div>,
}));

vi.mock('@ui/table/TableBody', () => ({
  TableBody: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));

vi.mock('@ui/table/TableFooter', () => ({
  TableFooter: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));

vi.mock('@ui/table/TableHeader', () => ({
  TableHeader: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));

vi.mock('@pages/usuarios/menu/MenuAcciones', () => ({
  default: ({ id }: { id: string }) => <div>acciones {id}</div>,
}));

vi.mock('@components/ui/AuditDateHoverCard', () => ({
  AuditDateHoverCard: ({ title }: { title: string }) => <span>{title}</span>,
}));

afterEach(() => {
  vi.clearAllMocks();
});

describe('UsuariosPageSection', () => {
  it('arma params iniciales, resetea pagina con filtros, refresca tabla', async () => {
    useUsuariosQueryMock.mockReturnValue({
      data: {
        items: [],
        pagina: 1,
        tamano: 15,
        totalRegistros: 0,
        totalPaginas: 1,
      },
      isFetching: false,
      isError: false,
      refetch: refetchMock,
    });

    const user = userEvent.setup();
    render(
      <UsuariosPageSection
        puedeEditarUsuarios={false}
        puedeCrearUsuarios={false}
      />,
    );

    expect(useUsuariosQueryMock).toHaveBeenLastCalledWith({
      numeroPagina: 1,
      tamanoPagina: 15,
      filtroDinamico: undefined,
      activo: true,
    });

    await user.click(screen.getByRole('button', { name: 'Ir pagina 4' }));
    expect(useUsuariosQueryMock).toHaveBeenLastCalledWith({
      numeroPagina: 4,
      tamanoPagina: 15,
      filtroDinamico: undefined,
      activo: true,
    });

    await user.type(screen.getByLabelText('Buscar usuario'), 'juan');
    expect(useUsuariosQueryMock).toHaveBeenLastCalledWith({
      numeroPagina: 1,
      tamanoPagina: 15,
      filtroDinamico: 'juan',
      activo: true,
    });

    await user.selectOptions(screen.getByLabelText('Estado'), 'inactivos');
    expect(useUsuariosQueryMock).toHaveBeenLastCalledWith({
      numeroPagina: 1,
      tamanoPagina: 15,
      filtroDinamico: 'juan',
      activo: false,
    });

    await user.click(screen.getByRole('button', { name: 'Tamano 50' }));
    expect(useUsuariosQueryMock).toHaveBeenLastCalledWith({
      numeroPagina: 1,
      tamanoPagina: 50,
      filtroDinamico: 'juan',
      activo: false,
    });

    await user.click(screen.getByRole('button', { name: 'Refrescar' }));
    expect(refetchMock).toHaveBeenCalled();
  });
});
