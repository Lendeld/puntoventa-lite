"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { editarProductoAction } from "@lib/actions/productos.actions";
import { PRODUCTO_FIELDS } from "@lib/constants/productos.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { useProductoPrecio } from "@lib/hooks/useProductoPrecio";
import { editarProductoSchema } from "@lib/schemas/productos.schema";
import { obtenerProductoPorIdService } from "@lib/services/productos.service";
import type { EditarProductoFormValues } from "@lib/types/productos.types";
import { zodResolver } from "@lib/utils/zodResolver";
import { ProductoFormFields } from "@pages/inventario/productos/ProductoFormFields";
import { Alert, Button, Grid, Group, Loader, Stack } from "@mantine/core";
import { useForm } from "@mantine/form";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useRef } from "react";

interface Props {
    id: string;
    puedeGestionarNoAplicaExistencias: boolean;
}

export default function EditProductoForm({
    id,
    puedeGestionarNoAplicaExistencias,
}: Props) {
    const queryClient = useQueryClient();
    const { replace } = useRouter();
    const hydratedRef = useRef<string | null>(null);

    const form = useForm<EditarProductoFormValues>({
        initialValues: {
            [PRODUCTO_FIELDS.CODIGO]: "",
            [PRODUCTO_FIELDS.CODIGO_BARRAS]: "",
            [PRODUCTO_FIELDS.NOMBRE]: "",
            [PRODUCTO_FIELDS.DESCRIPCION]: "",
            [PRODUCTO_FIELDS.TIPO_ITEM]: 1,
            [PRODUCTO_FIELDS.PRECIO_UNITARIO]: 0,
            [PRODUCTO_FIELDS.PRECIO_COSTO]: undefined,
            [PRODUCTO_FIELDS.CATEGORIA_ID]: "",
            [PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO]: undefined,
            [PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS]: false,
            [PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO]: false,
        },
        validate: zodResolver(editarProductoSchema),
    });

    const precio = useProductoPrecio(form);
    const { tarifasData, initFromProducto } = precio;

    const {
        data: producto,
        isLoading: loadingProducto,
        isError: isProductoError,
    } = useQuery({
        queryKey: QUERY_KEYS.productos.detalle(id),
        queryFn: async () => {
            const res = await obtenerProductoPorIdService(id);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        staleTime: 0,
        refetchOnMount: "always",
    });

    useEffect(() => {
        if (!producto || !tarifasData) return;
        if (hydratedRef.current === producto.id) return;

        form.setValues({
            [PRODUCTO_FIELDS.CODIGO]: producto.codigo,
            [PRODUCTO_FIELDS.CODIGO_BARRAS]: producto.codigoBarras ?? "",
            [PRODUCTO_FIELDS.NOMBRE]: producto.nombre,
            [PRODUCTO_FIELDS.DESCRIPCION]: producto.descripcion ?? "",
            [PRODUCTO_FIELDS.TIPO_ITEM]: producto.tipoItem === "Bien" ? 1 : 2,
            [PRODUCTO_FIELDS.PRECIO_UNITARIO]: producto.precioUnitario,
            [PRODUCTO_FIELDS.PRECIO_COSTO]: producto.precioCosto ?? undefined,
            [PRODUCTO_FIELDS.CATEGORIA_ID]: producto.categoriaId ?? "",
            [PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO]:
                producto.tarifaIvaImpuestoCodigo ?? undefined,
            [PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS]: producto.noAplicaExistencias,
            [PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO]:
                producto.permiteModificarPrecioUnitario,
        });
        form.resetDirty();
        hydratedRef.current = producto.id;

        initFromProducto(
            producto.precioUnitario,
            producto.tarifaIvaImpuestoCodigo ?? undefined,
        );
    // form.setValues/resetDirty de Mantine no son estables; deps solo [producto, tarifasData].
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [producto, tarifasData]);

    const { execute, loading, error, setError } =
        useActionHandler<EditarProductoFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.productos.all,
                });
                AppNotifier.success({
                    message: "Producto actualizado exitosamente.",
                });
                replace(`/inventario/productos/${id}`);
            },
        });

    if (loadingProducto) {
        return (
            <Stack align="center" py="xl">
                <Loader color="accentPV" />
            </Stack>
        );
    }

    if (isProductoError || !producto) {
        return (
            <Alert
                icon={<IconAlertCircle size={16} />}
                variant="light"
                color="red"
            >
                Error al cargar datos del producto.
            </Alert>
        );
    }

    return (
        <form
            onSubmit={form.onSubmit(
                (v) => execute(() => editarProductoAction(id, v)),
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
                    precioDecimalScale={2}
                    precioFixedDecimalScale
                />

                <Grid.Col span={12}>
                    <Group justify="flex-end" mt="xs">
                        <Button
                            component={Link}
                            href={`/inventario/productos/${id}`}
                            variant="light"
                        >
                            Cancelar
                        </Button>
                        <Button
                            type="submit"
                            loading={loading}
                            disabled={!form.isDirty()}
                        >
                            Guardar cambios
                        </Button>
                    </Group>
                </Grid.Col>
            </Grid>
        </form>
    );
}
