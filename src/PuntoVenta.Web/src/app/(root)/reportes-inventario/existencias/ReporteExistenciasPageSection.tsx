"use client";

import {
    Button,
    Divider,
    Paper,
    Select,
    Stack,
    Text,
    TextInput,
    ThemeIcon,
} from "@mantine/core";
import { IconArrowLeft, IconFileSpreadsheet, IconReportMoney } from "@tabler/icons-react";
import Link from "next/link";
import { parseAsString, useQueryState } from "nuqs";
import { useCategoriasActivasQuery } from "@lib/hooks/useCategoriasActivasQuery";
import { useProveedoresActivosQuery } from "@lib/hooks/useProveedoresActivosQuery";
import { ROUTES } from "@lib/constants/routes.constants";

export function ReporteExistenciasPageSection() {
    const [codigo, setCodigo] = useQueryState("codigo", parseAsString.withDefault(""));
    const [categoria, setCategoria] = useQueryState("categoria", parseAsString.withDefault(""));
    const [proveedor, setProveedor] = useQueryState("proveedor", parseAsString.withDefault(""));

    const { data: categorias } = useCategoriasActivasQuery();
    const categoriaOptions = [
        { value: "", label: "Todas las categorías" },
        ...(categorias ?? []).map((c) => ({ value: c.id, label: c.nombre })),
    ];

    const { data: proveedores } = useProveedoresActivosQuery();
    const proveedorOptions = [
        { value: "", label: "Todos los proveedores" },
        ...(proveedores ?? []).map((p) => ({ value: p.id, label: p.nombre })),
    ];

    function descargarExcel() {
        const qs = new URLSearchParams();
        if (codigo.trim()) qs.set("Codigo", codigo.trim());
        if (categoria) qs.set("CategoriaId", categoria);
        if (proveedor) qs.set("ProveedorId", proveedor);
        const a = document.createElement("a");
        a.href = `/excel/inventario/existencias${qs.size ? `?${qs.toString()}` : ""}`;
        a.download = "";
        a.rel = "noopener";
        document.body.appendChild(a);
        a.click();
        a.remove();
    }

    return (
        <Stack gap="lg" className="w-full">
            <Button
                component={Link}
                href={ROUTES.REPORTES_INVENTARIO}
                variant="light"
                size="xs"
                leftSection={<IconArrowLeft size={14} />}
                className="self-start"
            >
                Volver a reportes
            </Button>

            <div className="flex w-full flex-1 items-start justify-center py-2 sm:py-6">
                <Paper
                    p="xl"
                    withBorder
                    radius="lg"
                    shadow="sm"
                    className="w-full max-w-140"
                >
                    <Stack gap="lg">
                        <Stack gap="sm" align="center" className="text-center">
                            <ThemeIcon
                                size={56}
                                radius="xl"
                                variant="light"
                                color="accentPV"
                            >
                                <IconReportMoney size={28} />
                            </ThemeIcon>
                            <Stack gap={4} align="center">
                                <Text fw={700} size="xl">
                                    Existencias
                                </Text>
                                <Text size="sm" c="dimmed" className="max-w-prose">
                                    Stock actual valorizado por producto: existencia, costo, precio
                                    e impuesto. Los resultados se exportan directamente a Excel.
                                </Text>
                            </Stack>
                        </Stack>

                        <Divider />

                        <Stack gap="md">
                            <Text size="xs" fw={600} c="dimmed" tt="uppercase" className="tracking-wide">
                                Filtros
                            </Text>
                            <TextInput
                                label="Código"
                                placeholder="Opcional"
                                value={codigo}
                                onChange={(e) => void setCodigo(e.currentTarget.value)}
                            />
                            <Select
                                label="Categoría"
                                data={categoriaOptions}
                                value={categoria}
                                onChange={(v) => void setCategoria(v ?? "")}
                                comboboxProps={{ withinPortal: true }}
                                allowDeselect={false}
                                searchable
                            />
                            <Select
                                label="Proveedor"
                                data={proveedorOptions}
                                value={proveedor}
                                onChange={(v) => void setProveedor(v ?? "")}
                                comboboxProps={{ withinPortal: true }}
                                allowDeselect={false}
                                searchable
                            />
                        </Stack>

                        <Button
                            fullWidth
                            size="md"
                            onClick={descargarExcel}
                            leftSection={<IconFileSpreadsheet size={18} />}
                        >
                            Exportar a Excel
                        </Button>
                    </Stack>
                </Paper>
            </div>
        </Stack>
    );
}
