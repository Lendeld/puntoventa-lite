"use client";

import type { CajaListadoItemDto } from "@lib/types/cajas.types";
import type { ClienteListaDto } from "@lib/types/clientes.types";
import type { VendedorActivoDto } from "@lib/types/vendedores.types";
import { FacturacionCondicionesCard } from "@pages/emision/facturacion/FacturacionCondicionesCard";
import { FacturacionVentaCard } from "@pages/emision/facturacion/FacturacionVentaCard";
import { Grid } from "@mantine/core";
import type { ReactNode } from "react";

export interface FacturacionDocumentoCardProps {
    values: Record<string, unknown>;
    errors: Record<string, ReactNode>;
    disabled: boolean;
    clienteSeleccionado: ClienteListaDto | null;
    aplicaVendedores: boolean;
    vendedores: VendedorActivoDto[];
    aplicaCajas: boolean;
    cajas: CajaListadoItemDto[];
    onClienteChange: (value: string) => void;
    onClienteSeleccionadoChange: (cliente: ClienteListaDto | null) => void;
    onFieldChange: (field: string, value: unknown) => void;
    showFechaVencimiento?: boolean;
    fechaVencimientoLabel?: string;
}

export function FacturacionDocumentoCard(props: FacturacionDocumentoCardProps) {
    return (
        <Grid gap="lg" align="stretch">
            <Grid.Col span={{ base: 12, xl: 8 }}>
                <FacturacionVentaCard {...props} />
            </Grid.Col>
            <Grid.Col span={{ base: 12, xl: 4 }}>
                <FacturacionCondicionesCard {...props} />
            </Grid.Col>
        </Grid>
    );
}
