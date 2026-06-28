"use client";

import { CategoriaSelect } from "@ui/selects/CategoriaSelect";
import { ProveedorSelect } from "@ui/selects/ProveedorSelect";
import { TarifaIvaSelect } from "@ui/selects/TarifaIvaSelect";
import { PRODUCTO_FIELDS } from "@lib/constants/productos.constants";
import type {
    CrearProductoFormValues,
    EditarProductoFormValues,
} from "@lib/types/productos.types";
import {
    Checkbox,
    Grid,
    NumberInput,
    Select,
    TextInput,
    Textarea,
} from "@mantine/core";
import type { UseFormReturnType } from "@mantine/form";

const TIPO_ITEM_OPTIONS = [
    { value: "1", label: "Bien" },
    { value: "2", label: "Servicio" },
];

const PRECIO_VENTA_CLASSES = { input: "font-semibold" };

type ProductoFormValues = CrearProductoFormValues & EditarProductoFormValues;

interface Props {
    form: UseFormReturnType<ProductoFormValues>;
    puedeGestionarNoAplicaExistencias: boolean;
    precioVenta: number | string;
    porcentaje: number | null;
    handleTarifaChange: (val: string | null) => void;
    handlePrecioUnitarioChange: (val: number | string) => void;
    handlePrecioVentaChange: (val: number | string) => void;
    precioDecimalScale: number;
    precioFixedDecimalScale?: boolean;
    // Solo en alta: permite cargar el stock inicial sin un segundo paso de ajuste.
    mostrarExistenciaInicial?: boolean;
}

// Los campos comparten el mismo form y reglas fiscales derivadas; mantenerlos juntos evita una API de props duplicada.
// react-doctor-disable-next-line react-doctor/no-giant-component
export function ProductoFormFields({
    form,
    puedeGestionarNoAplicaExistencias,
    precioVenta,
    porcentaje,
    handleTarifaChange,
    handlePrecioUnitarioChange,
    handlePrecioVentaChange,
    precioDecimalScale,
    precioFixedDecimalScale = false,
    mostrarExistenciaInicial = false,
}: Props) {
    const tarifaIvaCodigo = form.values[PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO];
    const esBien = form.values[PRODUCTO_FIELDS.TIPO_ITEM] === 1;
    const aplicaExistencias =
        esBien && !form.values[PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS];

    return (
        <>
            <Grid.Col span={6}>
                <TextInput
                    label="Código"
                    placeholder="ABC-001"
                    required
                    maxLength={20}
                    onKeyDown={(e) => {
                        if (e.key === "Enter") e.preventDefault();
                    }}
                    key={form.key(PRODUCTO_FIELDS.CODIGO)}
                    {...form.getInputProps(PRODUCTO_FIELDS.CODIGO)}
                />
            </Grid.Col>
            <Grid.Col span={6}>
                <TextInput
                    label="Código de barras"
                    placeholder="Escanee o escriba el código"
                    maxLength={50}
                    onKeyDown={(e) => {
                        if (e.key === "Enter") e.preventDefault();
                    }}
                    key={form.key(PRODUCTO_FIELDS.CODIGO_BARRAS)}
                    {...form.getInputProps(PRODUCTO_FIELDS.CODIGO_BARRAS)}
                />
            </Grid.Col>

            <Grid.Col span={12}>
                <TextInput
                    label="Nombre"
                    placeholder="Nombre del producto"
                    required
                    maxLength={150}
                    key={form.key(PRODUCTO_FIELDS.NOMBRE)}
                    {...form.getInputProps(PRODUCTO_FIELDS.NOMBRE)}
                />
            </Grid.Col>

            <Grid.Col span={12}>
                <Textarea
                    label="Descripción"
                    placeholder="Descripción del producto"
                    autosize
                    minRows={3}
                    maxLength={500}
                    key={form.key(PRODUCTO_FIELDS.DESCRIPCION)}
                    {...form.getInputProps(PRODUCTO_FIELDS.DESCRIPCION)}
                />
            </Grid.Col>

            <Grid.Col span={6}>
                <Select
                    label="Tipo de item"
                    required
                    data={TIPO_ITEM_OPTIONS}
                    value={String(form.values[PRODUCTO_FIELDS.TIPO_ITEM])}
                    onChange={(val) => {
                        const nextValue = parseInt(val ?? "1") as 1 | 2;
                        form.setFieldValue(PRODUCTO_FIELDS.TIPO_ITEM, nextValue);
                        if (nextValue !== 1) {
                            form.setFieldValue(PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS, false);
                        }
                    }}
                    error={form.errors[PRODUCTO_FIELDS.TIPO_ITEM]}
                    allowDeselect={false}
                />
            </Grid.Col>
            <Grid.Col span={6} />
            <Grid.Col span={6}>
                <CategoriaSelect
                    label="Categoría"
                    placeholder="Seleccionar categoría"
                    value={form.values[PRODUCTO_FIELDS.CATEGORIA_ID] || null}
                    onChange={(val) =>
                        form.setFieldValue(PRODUCTO_FIELDS.CATEGORIA_ID, val ?? "")
                    }
                    error={form.errors[PRODUCTO_FIELDS.CATEGORIA_ID]}
                />
            </Grid.Col>
            <Grid.Col span={6}>
                <ProveedorSelect
                    label="Proveedor"
                    placeholder="Sin proveedor"
                    value={form.values[PRODUCTO_FIELDS.PROVEEDOR_ID] || null}
                    onChange={(val) =>
                        form.setFieldValue(PRODUCTO_FIELDS.PROVEEDOR_ID, val ?? "")
                    }
                    error={form.errors[PRODUCTO_FIELDS.PROVEEDOR_ID]}
                />
            </Grid.Col>

            {puedeGestionarNoAplicaExistencias && esBien && (
                <Grid.Col span={12}>
                    <Checkbox
                        label="No aplica existencias"
                        description="Si está activo, este bien no validará stock ni generará movimientos de inventario al facturar."
                        key={form.key(PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS)}
                        {...form.getInputProps(PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS, {
                            type: "checkbox",
                        })}
                    />
                </Grid.Col>
            )}

            {mostrarExistenciaInicial && aplicaExistencias && (
                <Grid.Col span={6}>
                    <NumberInput
                        label="Existencia inicial"
                        description="Stock de arranque. Genera un movimiento 'Stock inicial'."
                        placeholder="0"
                        min={0}
                        decimalScale={5}
                        value={form.values[PRODUCTO_FIELDS.EXISTENCIA_INICIAL] ?? ""}
                        onChange={(val) =>
                            form.setFieldValue(
                                PRODUCTO_FIELDS.EXISTENCIA_INICIAL,
                                val === ""
                                    ? undefined
                                    : typeof val === "number"
                                      ? val
                                      : parseFloat(String(val)) || 0,
                            )
                        }
                        error={form.errors[PRODUCTO_FIELDS.EXISTENCIA_INICIAL]}
                    />
                </Grid.Col>
            )}

            <Grid.Col span={12}>
                <Checkbox
                    label="Permite modificar precio unitario al facturar"
                    description="Si está activo, el cajero puede cambiar el precio unitario de este producto al momento de facturar."
                    key={form.key(PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO)}
                    {...form.getInputProps(PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO, {
                        type: "checkbox",
                    })}
                />
            </Grid.Col>

            <Grid.Col span={12}>
                <TarifaIvaSelect
                    label="Tarifa IVA"
                    placeholder="Seleccionar tarifa IVA"
                    required
                    value={tarifaIvaCodigo || null}
                    onChange={handleTarifaChange}
                    error={form.errors[PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO]}
                />
            </Grid.Col>

            <Grid.Col span={6}>
                <NumberInput
                    label="Precio de costo"
                    placeholder="0.00"
                    min={0}
                    decimalScale={precioDecimalScale}
                    fixedDecimalScale={precioFixedDecimalScale}
                    prefix="₡ "
                    thousandSeparator=","
                    value={form.values[PRODUCTO_FIELDS.PRECIO_COSTO] ?? ""}
                    onChange={(val) =>
                        form.setFieldValue(
                            PRODUCTO_FIELDS.PRECIO_COSTO,
                            val === ""
                                ? undefined
                                : typeof val === "number"
                                  ? val
                                  : parseFloat(String(val)) || 0,
                        )
                    }
                    error={form.errors[PRODUCTO_FIELDS.PRECIO_COSTO]}
                />
            </Grid.Col>
            <Grid.Col span={6}>
                <NumberInput
                    label="Precio unitario (neto)"
                    placeholder="0.00"
                    required
                    min={0.01}
                    decimalScale={precioDecimalScale}
                    fixedDecimalScale={precioFixedDecimalScale}
                    prefix="₡ "
                    thousandSeparator=","
                    value={form.values[PRODUCTO_FIELDS.PRECIO_UNITARIO] || ""}
                    onChange={handlePrecioUnitarioChange}
                    error={form.errors[PRODUCTO_FIELDS.PRECIO_UNITARIO]}
                />
            </Grid.Col>
            <Grid.Col span={12}>
                <NumberInput
                    label={`Precio venta${porcentaje !== null && porcentaje !== 0 ? ` (+${porcentaje}% IVA)` : ""}`}
                    placeholder="0.00"
                    min={0.01}
                    decimalScale={precioDecimalScale}
                    fixedDecimalScale={precioFixedDecimalScale}
                    prefix="₡ "
                    thousandSeparator=","
                    value={precioVenta}
                    onChange={handlePrecioVentaChange}
                    classNames={PRECIO_VENTA_CLASSES}
                />
            </Grid.Col>
        </>
    );
}
