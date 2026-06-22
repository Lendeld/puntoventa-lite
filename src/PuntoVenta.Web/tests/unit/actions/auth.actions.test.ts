import { beforeEach, describe, expect, it, vi } from "vitest";
import { actualizarUsuarioActualAction } from "@lib/actions/auth.actions";
import { USUARIO_FIELDS } from "@lib/constants/usuarios.constants";
import { actualizarUsuarioActualService } from "@lib/services/auth.service";

vi.mock("@lib/services/auth.service", () => ({
    actualizarUsuarioActualService: vi.fn(),
    cambiarPasswordUsuarioActualService: vi.fn(),
    cambiarNegocioActualService: vi.fn(),
    confirmOtpService: vi.fn(),
    disableOtpService: vi.fn(),
    loginService: vi.fn(),
    seleccionarNegocioService: vi.fn(),
    setupOtpService: vi.fn(),
    validarTokenService: vi.fn(),
    verifyOtpService: vi.fn(),
}));

const actualizarUsuarioActualServiceMock = vi.mocked(actualizarUsuarioActualService);

describe("auth.actions", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("actualizarUsuarioActualAction arma payload correcto y retorna 204", async () => {
        actualizarUsuarioActualServiceMock.mockResolvedValue({
            data: null,
            errors: undefined,
        });

        const result = await actualizarUsuarioActualAction({
            [USUARIO_FIELDS.NOMBRE]: " Juan Perez ",
            [USUARIO_FIELDS.IDENTIFICACION]: " 1-1111-1111 ",
            [USUARIO_FIELDS.CORREO]: " juan@demo.com ",
            [USUARIO_FIELDS.TELEFONO]: " 8888-9999 ",
        });

        expect(actualizarUsuarioActualServiceMock).toHaveBeenCalledWith({
            nombre: "Juan Perez",
            identificacion: "1-1111-1111",
            correo: "juan@demo.com",
            telefono: "8888-9999",
        });
        expect(result).toEqual({
            status: 204,
            errors: undefined,
        });
    });
});
