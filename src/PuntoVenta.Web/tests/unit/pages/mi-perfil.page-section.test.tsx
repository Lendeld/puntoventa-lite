import { PropsWithChildren } from "react";
import { MantineProvider } from "@mantine/core";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "../../utils/render";
import MiPerfilPageSection from "@/app/(root)/mi-perfil/MiPerfilPageSection";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { USUARIO_FIELDS } from "@lib/constants/usuarios.constants";
import {
    actualizarUsuarioActualAction,
    cambiarPasswordUsuarioActualAction,
    establecerPinUsuarioActualAction,
} from "@lib/actions/auth.actions";
import { obtenerUsuarioActualService } from "@lib/services/auth.service";
import { AppNotifier } from "@components/ui/AppNotifier";

vi.mock("@lib/actions/auth.actions", () => ({
    actualizarUsuarioActualAction: vi.fn(),
    cambiarPasswordUsuarioActualAction: vi.fn(),
    establecerPinUsuarioActualAction: vi.fn(),
}));

vi.mock("@lib/services/auth.service", () => ({
    obtenerUsuarioActualService: vi.fn(),
}));

vi.mock("@components/ui/AppNotifier", () => ({
    AppNotifier: {
        success: vi.fn(),
    },
}));

const actualizarUsuarioActualActionMock = vi.mocked(actualizarUsuarioActualAction);
const cambiarPasswordUsuarioActualActionMock = vi.mocked(cambiarPasswordUsuarioActualAction);
const establecerPinUsuarioActualActionMock = vi.mocked(establecerPinUsuarioActualAction);
const obtenerUsuarioActualServiceMock = vi.mocked(obtenerUsuarioActualService);
const successMock = vi.mocked(AppNotifier.success);

function renderWithQueryClient(ui: React.ReactElement) {
    const queryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });

    const invalidateSpy = vi.spyOn(queryClient, "invalidateQueries");
    const wrapper = ({ children }: PropsWithChildren) => (
        // env="test" desactiva transiciones de Mantine: evita el timer que
        // disparaba setState tras el teardown ("window is not defined").
        <MantineProvider env="test">
            <QueryClientProvider client={queryClient}>
                {children}
            </QueryClientProvider>
        </MantineProvider>
    );

    return {
        invalidateSpy,
        ...render(ui, { wrapper }),
    };
}

describe("MiPerfilPageSection", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        obtenerUsuarioActualServiceMock.mockResolvedValue({
            data: {
                usuario: "owner.user",
                nombre: "Owner User",
                correo: "owner@demo.com",
                telefono: "8888-9999",
                identificacion: "123456789",
                debeCambiarPassword: false,
                deploymentMode: "Cloud",
                tienePin: false,
            },
            errors: undefined,
        });
    });

    it("muestra las secciones de perfil y guarda cambios personales", async () => {
        actualizarUsuarioActualActionMock.mockResolvedValue({
            status: 204,
            errors: undefined,
        });

        const { invalidateSpy } = renderWithQueryClient(<MiPerfilPageSection />);

        await waitFor(() =>
            expect(screen.getByText("Datos personales")).toBeInTheDocument(),
        );

        expect(screen.getByRole("tab", { name: "Cuenta" })).toBeInTheDocument();
        expect(screen.getByRole("tab", { name: "Contraseña" })).toBeInTheDocument();

        const user = userEvent.setup();
        const nombreInput = screen.getByDisplayValue("Owner User");
        await user.clear(nombreInput);
        await user.type(nombreInput, "Owner Updated");
        await user.click(screen.getByRole("button", { name: "Guardar cambios" }));

        await waitFor(() =>
            expect(actualizarUsuarioActualActionMock).toHaveBeenCalledWith({
                [USUARIO_FIELDS.NOMBRE]: "Owner Updated",
                [USUARIO_FIELDS.IDENTIFICACION]: "123456789",
                [USUARIO_FIELDS.CORREO]: "owner@demo.com",
                [USUARIO_FIELDS.TELEFONO]: "8888-9999",
            }),
        );

        await waitFor(() =>
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: QUERY_KEYS.auth.usuarioActual,
            }),
        );
        expect(successMock).toHaveBeenCalledWith({
            message: "Perfil actualizado exitosamente.",
        });
        expect(cambiarPasswordUsuarioActualActionMock).not.toHaveBeenCalled();
    });
});
