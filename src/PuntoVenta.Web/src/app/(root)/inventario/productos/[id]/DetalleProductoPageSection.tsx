"use client";

import AjustarStockButton from "@pages/inventario/productos/AjustarStockButton";
import { useCategoriasActivasQuery } from "@lib/hooks/useCategoriasActivasQuery";
import { useTarifasIvaActivasQuery } from "@lib/hooks/useTarifasIvaActivasQuery";
import { redondear } from "@lib/utils/number.utils";
import type { ProductoDto } from "@lib/types/productos.types";
import {
    Badge,
    Button,
    Card,
    Divider,
    Grid,
    Group,
    Stack,
    Text,
    ThemeIcon,
} from "@mantine/core";
import {
    IconArrowLeft,
    IconBoxSeam,
    IconCash,
    IconCategory,
    IconEdit,
    IconInfoCircle,
    IconSettings,
} from "@tabler/icons-react";
import Link from "next/link";
import type { ReactNode } from "react";

interface Permisos {
    editar: boolean;
    ajustarStock: boolean;
}

interface Props {
    producto: ProductoDto;
    permisos: Permisos;
}

function formatColones(value: number | null | undefined) {
    if (value == null) return null;
    return `₡ ${value.toLocaleString("es-CR", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 5,
    })}`;
}

function LabelValue({
    label,
    value,
}: {
    label: string;
    value: ReactNode;
}) {
    return (
        <Stack gap={2}>
            <Text c="dimmed" size="xs" tt="uppercase" fw={700}>
                {label}
            </Text>
            <Text size="sm" fw={500}>
                {value || "No indicado"}
            </Text>
        </Stack>
    );
}

function DetailCard({
    title,
    description,
    icon,
    children,
    className = "",
}: {
    title: string;
    description?: string;
    icon?: ReactNode;
    children: ReactNode;
    className?: string;
}) {
    return (
        <Card
            radius="lg"
            p="lg"
            className={`${className}`}
        >
            <Stack gap="md">
                <Group gap="xs">
                    {icon && (
                        <ThemeIcon variant="light" color="accentPV">
                            {icon}
                        </ThemeIcon>
                    )}
                    <Stack gap={2}>
                        <Text fw={700}>{title}</Text>
                        {description && (
                            <Text size="sm" c="dimmed">
                                {description}
                            </Text>
                        )}
                    </Stack>
                </Group>
                {children}
            </Stack>
        </Card>
    );
}

export default function DetalleProductoPageSection({
    producto,
    permisos,
}: Props) {
    const { data: categorias } = useCategoriasActivasQuery();
    const { data: tarifas } = useTarifasIvaActivasQuery();

    const categoriaNombre = categorias?.find((c) => c.id === producto.categoriaId)?.nombre;
    const tarifa = tarifas?.find(
        (t) => t.codigo === producto.tarifaIvaImpuestoCodigo,
    );
    const porcentajeIva = tarifa?.porcentaje ?? 0;
    const precioVenta = redondear(producto.precioUnitario * (1 + porcentajeIva / 100));

    const tipoLabel = producto.tipoItem === "Bien" ? "Bien" : "Servicio";

    return (
        <Stack gap="lg">
            <Group justify="space-between" align="center" gap="md">
                <Button
                    component={Link}
                    href="/inventario/productos"
                    variant="light"
                    size="xs"
                    leftSection={<IconArrowLeft size={14} />}
                    w="fit-content"
                >
                    Volver a productos
                </Button>

                <Group gap="xs" justify="flex-end">
                    {permisos.ajustarStock &&
                        producto.tipoItem === "Bien" &&
                        !producto.noAplicaExistencias && (
                            <AjustarStockButton
                                id={producto.id}
                                nombre={producto.nombre}
                            />
                        )}
                    {permisos.editar && (
                        <Button
                            component={Link}
                            href={`/inventario/productos/${producto.id}/editar`}
                            leftSection={<IconEdit size={16} />}
                        >
                            Editar
                        </Button>
                    )}
                </Group>
            </Group>

            <Grid gap="md">
                <Grid.Col span={{ base: 12, md: 8 }}>
                    <DetailCard
                        title="Información general"
                        description="Datos principales del producto."
                        icon={<IconInfoCircle size={18} />}
                        className="h-full"
                    >
                        <Stack gap="lg">
                            <Grid>
                                <Grid.Col span={{ base: 12, sm: 6, md: 4 }}>
                                    <LabelValue
                                        label="Código"
                                        value={producto.codigo}
                                    />
                                </Grid.Col>
                                <Grid.Col span={{ base: 12, sm: 6, md: 4 }}>
                                    <LabelValue
                                        label="Código de barras"
                                        value={producto.codigoBarras}
                                    />
                                </Grid.Col>
                                <Grid.Col span={{ base: 12, sm: 6, md: 4 }}>
                                    <Stack gap={2}>
                                        <Text c="dimmed" size="xs" tt="uppercase" fw={700}>
                                            Tipo
                                        </Text>
                                        <Badge color="accentPV" variant="light" w="fit-content">
                                            {tipoLabel}
                                        </Badge>
                                    </Stack>
                                </Grid.Col>
                                <Grid.Col span={{ base: 12, md: 8 }}>
                                    <LabelValue
                                        label="Nombre"
                                        value={producto.nombre}
                                    />
                                </Grid.Col>
                                <Grid.Col span={12}>
                                    <LabelValue
                                        label="Descripción"
                                        value={producto.descripcion}
                                    />
                                </Grid.Col>
                            </Grid>
                        </Stack>
                    </DetailCard>
                </Grid.Col>

                <Grid.Col span={{ base: 12, md: 4 }}>
                    <DetailCard
                        title="Clasificación"
                        description="Categoría asociada."
                        icon={<IconCategory size={18} />}
                        className="h-full"
                    >
                        <Stack gap="md">
                            <LabelValue
                                label="Categoría"
                                value={categoriaNombre}
                            />
                        </Stack>
                    </DetailCard>
                </Grid.Col>
            </Grid>

            <DetailCard
                title="Precios e impuestos"
                description="Configuración de precios y tributación."
                icon={<IconCash size={18} />}
            >
                <Stack gap="lg">
                    <Grid>
                        <Grid.Col span={{ base: 12, sm: 12 }}>
                            <LabelValue
                                label="Tarifa IVA"
                                value={
                                    tarifa
                                        ? `${tarifa.detalle} (${tarifa.porcentaje}%)`
                                        : "Sin tarifa"
                                }
                            />
                        </Grid.Col>
                    </Grid>
                    <Divider />
                    <Grid>
                        <Grid.Col span={{ base: 12, sm: 4 }}>
                            <LabelValue
                                label="Precio de costo"
                                value={formatColones(producto.precioCosto)}
                            />
                        </Grid.Col>
                        <Grid.Col span={{ base: 12, sm: 4 }}>
                            <LabelValue
                                label="Precio unitario (neto)"
                                value={formatColones(producto.precioUnitario)}
                            />
                        </Grid.Col>
                        <Grid.Col span={{ base: 12, sm: 4 }}>
                            <LabelValue
                                label={`Precio venta${porcentajeIva ? ` (+${porcentajeIva}% IVA)` : ""}`}
                                value={formatColones(precioVenta)}
                            />
                        </Grid.Col>
                    </Grid>
                </Stack>
            </DetailCard>

            {producto.tipoItem === "Bien" && !producto.noAplicaExistencias && (
                <DetailCard
                    title="Existencia"
                    description="Stock disponible."
                    icon={<IconBoxSeam size={18} />}
                >
                    <Group gap="md" align="baseline">
                        <Text fw={700} size="xl">
                            {producto.existenciaTotal.toLocaleString("es-CR", {
                                minimumFractionDigits: 0,
                                maximumFractionDigits: 5,
                            })}
                        </Text>
                    </Group>
                </DetailCard>
            )}

            <DetailCard
                title="Configuración"
                description="Banderas operativas del producto."
                icon={<IconSettings size={18} />}
            >
                <Grid>
                    <Grid.Col span={{ base: 12, sm: 6 }}>
                        <LabelValue
                            label="No aplica existencias"
                            value={producto.noAplicaExistencias ? "Sí" : "No"}
                        />
                    </Grid.Col>
                    <Grid.Col span={{ base: 12, sm: 6 }}>
                        <LabelValue
                            label="Permite modificar precio al facturar"
                            value={
                                producto.permiteModificarPrecioUnitario
                                    ? "Sí"
                                    : "No"
                            }
                        />
                    </Grid.Col>
                </Grid>
            </DetailCard>
        </Stack>
    );
}
