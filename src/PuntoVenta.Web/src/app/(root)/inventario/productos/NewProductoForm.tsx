"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { crearProductoAction } from "@lib/actions/productos.actions";
import { PRODUCTO_FIELDS } from "@lib/constants/productos.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { useProductoPrecio } from "@lib/hooks/useProductoPrecio";
import { crearProductoSchema } from "@lib/schemas/productos.schema";
import type { CrearProductoFormValues } from "@lib/types/productos.types";
import { zodResolver } from "@lib/utils/zodResolver";
import { ProductoFormFields } from "@pages/inventario/productos/ProductoFormFields";
import { Alert, Button, Grid, Group } from "@mantine/core";
import { useForm } from "@mantine/form";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";

interface Props {
    puedeGestionarNoAplicaExistencias: boolean;
}

export function NewProductoForm({ puedeGestionarNoAplicaExistencias }: Props) {
    const queryClient = useQueryClient();
    const { replace } = useRouter();

    const form = useForm<CrearProductoFormValues>({
        initialValues: {
            [PRODUCTO_FIELDS.CODIGO]: "",
            [PRODUCTO_FIELDS.CODIGO_BARRAS]: "",
            [PRODUCTO_FIELDS.NOMBRE]: "",
            [PRODUCTO_FIELDS.DESCRIPCION]: "",
            [PRODUCTO_FIELDS.TIPO_ITEM]: 1,
            [PRODUCTO_FIELDS.PRECIO_UNITARIO]: 0,
            [PRODUCTO_FIELDS.PRECIO_COSTO]: undefined,
            [PRODUCTO_FIELDS.CATEGORIA_ID]: "",
            [PRODUCTO_FIELDS.PROVEEDOR_ID]: "",
            [PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO]: "",
            [PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS]: false,
            [PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO]: false,
            [PRODUCTO_FIELDS.EXISTENCIA_INICIAL]: undefined,
        },
        validate: zodResolver(crearProductoSchema),
    });

    const precio = useProductoPrecio(form);

    const { execute, loading, error, setError } =
        useActionHandler<CrearProductoFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.productos.all,
                });
                AppNotifier.success({
                    message: "Producto guardado exitosamente.",
                });
                replace("/inventario/productos");
            },
        });

    return (
        <form
            onSubmit={form.onSubmit(
                (v) => execute(() => crearProductoAction(v)),
                () => setError(null),
            )}
            noValidate
        >
            <Grid gap="md">
                {error && (
                    <Grid.Col span={12}>
                        <Alert
                            icon={<IconAlertCircle size={16} />}
                            variant="light"
                            color="red"
                        >
                            {error}
                        </Alert>
                    </Grid.Col>
                )}

                <ProductoFormFields
                    form={form}
                    puedeGestionarNoAplicaExistencias={puedeGestionarNoAplicaExistencias}
                    precioVenta={precio.precioVenta}
                    porcentaje={precio.porcentaje}
                    handleTarifaChange={precio.handleTarifaChange}
                    handlePrecioUnitarioChange={precio.handlePrecioUnitarioChange}
                    handlePrecioVentaChange={precio.handlePrecioVentaChange}
                    precioDecimalScale={5}
                    mostrarExistenciaInicial
                />

                <Grid.Col span={12}>
                    <Group justify="flex-end" mt="xs">
                        <Button
                            component={Link}
                            href="/inventario/productos"
                            variant="light"
                        >
                            Cancelar
                        </Button>
                        <Button type="submit" loading={loading}>
                            Crear producto
                        </Button>
                    </Group>
                </Grid.Col>
            </Grid>
        </form>
    );
}
