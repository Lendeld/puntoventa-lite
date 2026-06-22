import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "../../utils/render";
import { PermisoClient } from "@/components/auth/PermisoClient";
import { ROUTES } from "@lib/constants/routes.constants";

const fetchMock = vi.fn();

describe("PermisoClient", () => {
    beforeEach(() => {
        fetchMock.mockReset();
        vi.stubGlobal("fetch", fetchMock);
    });

    it("renderiza children si permiso existe", async () => {
        fetchMock.mockResolvedValue({
            ok: true,
            status: 200,
            json: vi.fn().mockResolvedValue({
                tienePermiso: true,
            }),
        });

        render(
            <PermisoClient permiso="roles:crear" fallback={<span>no</span>}>
                <span>si</span>
            </PermisoClient>,
        );

        await waitFor(() => {
            expect(screen.getByText("si")).toBeInTheDocument();
        });

        expect(fetchMock).toHaveBeenCalledWith(
            `${ROUTES.API_VALIDATE_PERMISSION}?permiso=roles%3Acrear`,
            expect.objectContaining({
                method: "GET",
            }),
        );
    });

    it("renderiza fallback si permiso no existe", async () => {
        fetchMock.mockResolvedValue({
            ok: false,
            status: 403,
            json: vi.fn(),
        });

        render(
            <PermisoClient permiso="roles:crear" fallback={<span>no</span>}>
                <span>si</span>
            </PermisoClient>,
        );

        await waitFor(() => {
            expect(screen.getByText("no")).toBeInTheDocument();
        });
    });
});
