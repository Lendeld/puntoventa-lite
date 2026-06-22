import { describe, expect, it, vi } from "vitest";
import { render, screen } from "../../utils/render";
import { NavItem } from "@/components/ui/navigation/NavItem";

const usePathnameMock = vi.fn();

vi.mock("next/navigation", () => ({
    usePathname: () => usePathnameMock(),
}));

describe("NavItem", () => {
    it("no marca como activa una ruta exacta cuando el pathname es una subruta", () => {
        usePathnameMock.mockReturnValue("/inventario/productos");

        render(
            <NavItem
                item={{
                    label: "Existencias",
                    href: "/inventario",
                    icon: "IconBox",
                    exact: true,
                }}
            />,
        );

        expect(screen.getByRole("button", { name: "Existencias" })).not.toHaveClass(
            "bg-theme-accent-soft",
        );
    });

    it("mantiene activa una ruta hija cuando el pathname pertenece a su modulo", () => {
        usePathnameMock.mockReturnValue("/inventario/productos/123");

        render(
            <NavItem
                item={{
                    label: "Productos",
                    href: "/inventario/productos",
                    icon: "IconPackage",
                }}
            />,
        );

        expect(screen.getByRole("button", { name: "Productos" })).toHaveClass(
            "bg-theme-accent-soft",
        );
    });

    it("mantiene activo Reportes Ventas en la subruta /reportes-ventas/ventas-rango", () => {
        usePathnameMock.mockReturnValue("/reportes-ventas/ventas-rango");

        render(
            <NavItem
                item={{
                    label: "Reportes Ventas",
                    href: "/reportes-ventas",
                    icon: "IconReportAnalytics",
                }}
            />,
        );

        expect(
            screen.getByRole("button", { name: "Reportes Ventas" }),
        ).toHaveClass("bg-theme-accent-soft");
    });
});
