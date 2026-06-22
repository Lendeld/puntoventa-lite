import { render, waitFor } from "../../utils/render";
import { describe, it, expect, vi, beforeEach } from "vitest";
import {
    TokenValidator,
    BROADCAST_CHANNEL_NAME,
} from "@/components/auth/TokenValidator";
import { tokenValidatorBrowser } from "@/components/auth/tokenValidatorBrowser";

const replaceMock = vi.fn();
const reloadMock = vi.fn();
const fetchMock = vi.fn();

vi.mock("next/navigation", () => ({
    useRouter: () => ({
        replace: replaceMock,
    }),
}));

describe("TokenValidator", () => {
    beforeEach(() => {
        vi.useRealTimers();
        vi.restoreAllMocks();
        replaceMock.mockReset();
        reloadMock.mockReset();
        fetchMock.mockReset();

        vi.stubGlobal("fetch", fetchMock);
        vi.spyOn(tokenValidatorBrowser, "reloadPage").mockImplementation(
            reloadMock,
        );
        vi.spyOn(tokenValidatorBrowser, "redirectToLogout").mockImplementation(
            replaceMock,
        );
    });

    it("redirige a login cuando token no es valido (401 real inmediato)", async () => {
        fetchMock.mockResolvedValue({ ok: false, status: 401 });

        render(<TokenValidator />);

        await waitFor(() => {
            expect(replaceMock).toHaveBeenCalledTimes(1);
        }, { timeout: 2000 });
    });

    it("no redirige por un error transitorio de red agotando reintentos", async () => {
        vi.useFakeTimers();
        fetchMock.mockRejectedValue(new Error("network"));

        render(<TokenValidator />);

        // Avanzar el tiempo suficiente para cubrir 2s + 8s + 20s de backoff
        await vi.advanceTimersByTimeAsync(31_000);

        expect(replaceMock).not.toHaveBeenCalled();
        vi.useRealTimers();
    });

    it("no redirige por un error 503 transitorio del API", async () => {
        vi.useFakeTimers();
        fetchMock.mockResolvedValue({ ok: false, status: 503 });

        render(<TokenValidator />);

        await vi.advanceTimersByTimeAsync(31_000);

        expect(replaceMock).not.toHaveBeenCalled();
        vi.useRealTimers();
    });

    it("recarga pagina cuando vuelve desde bfcache", async () => {
        fetchMock.mockResolvedValue({ ok: true });

        render(<TokenValidator />);

        window.onpageshow?.(
            new PageTransitionEvent("pageshow", { persisted: true }),
        );

        expect(reloadMock).toHaveBeenCalledTimes(1);
    });

    it("ejecuta validar al recibir mensaje check del BroadcastChannel", async () => {
        fetchMock.mockResolvedValue({ ok: true });

        render(<TokenValidator />);

        // Esperar la primera validacion inicial
        await waitFor(() => {
            expect(fetchMock).toHaveBeenCalledTimes(1);
        }, { timeout: 2000 });

        // Simular mensaje "check" del canal
        const channel = new BroadcastChannel(BROADCAST_CHANNEL_NAME);
        channel.postMessage({ type: "check" });
        channel.close();

        await waitFor(() => {
            expect(fetchMock).toHaveBeenCalledTimes(2);
        }, { timeout: 2000 });

        expect(replaceMock).not.toHaveBeenCalled();
    });

    it("fuerza logout al recibir mensaje logout del BroadcastChannel", async () => {
        fetchMock.mockResolvedValue({ ok: true });

        render(<TokenValidator />);

        const channel = new BroadcastChannel(BROADCAST_CHANNEL_NAME);
        channel.postMessage({ type: "logout" });
        channel.close();

        await waitFor(() => {
            expect(replaceMock).toHaveBeenCalledTimes(1);
        }, { timeout: 2000 });
    });

    it("no desloguea cuando primer intento es transitorio pero el reintento tiene exito", async () => {
        vi.useFakeTimers();
        fetchMock
            .mockResolvedValueOnce({ ok: false, status: 503 })
            .mockResolvedValueOnce({ ok: true });

        render(<TokenValidator />);

        // Avanzar el primer delay del backoff (~2s)
        await vi.advanceTimersByTimeAsync(2500);

        expect(replaceMock).not.toHaveBeenCalled();
        expect(fetchMock).toHaveBeenCalledTimes(2);
        vi.useRealTimers();
    });
});
