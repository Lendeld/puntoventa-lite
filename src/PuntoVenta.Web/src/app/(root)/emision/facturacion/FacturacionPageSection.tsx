"use client";

import { VENTA_FIELDS } from "@lib/constants/ventas.constants";
import { useFacturacionPageController } from "@lib/hooks/facturacion/useFacturacionPageController";
import { FacturacionApartadoModal } from "@pages/emision/facturacion/FacturacionApartadoModal";
import { FacturacionCobroModal } from "@pages/emision/facturacion/FacturacionCobroModal";
import { FacturacionCreditoBanner } from "@pages/emision/facturacion/FacturacionCreditoBanner";
import { FacturacionDocumentoCard } from "@pages/emision/facturacion/FacturacionDocumentoCard";
import { FacturacionFooterBar } from "@pages/emision/facturacion/FacturacionFooterBar";
import { FacturacionLineasEditor } from "@pages/emision/facturacion/FacturacionLineasEditor";
import { FacturacionObservacionesCard } from "@pages/emision/facturacion/FacturacionObservacionesCard";
import { FacturacionResultadoDrawer } from "@pages/emision/facturacion/FacturacionResultadoDrawer";
import { Alert, Box, Loader, ScrollAreaAutosize, Stack } from "@mantine/core";
import { IconAlertCircle } from "@tabler/icons-react";
import { Suspense } from "react";

interface Props {
    puedeFacturar: boolean;
}

export default function FacturacionPageSection({ puedeFacturar }: Props) {
    return (
        <Suspense
            fallback={
                <Box className="flex h-page items-center justify-center">
                    <Loader size="sm" />
                </Box>
            }
        >
            <FacturacionPageContent puedeFacturar={puedeFacturar} />
        </Suspense>
    );
}

function FacturacionPageContent({ puedeFacturar }: Props) {
    const controller = useFacturacionPageController();
    const {
        form,
        state,
        dispatch,
        vendedores,
        aplicaVendedores,
        cajas,
        aplicaCajas,
        pagos,
        monedaCodigo,
        monedaEquivalente,
        equivalenteTotal,
        totales,
        lineasEditor,
        cobro,
        facturacionViewportRef,
        loadingGuardar,
        loadingEmitir,
        error,
        canEmit,
        showOpcionesCobro,
        emitirLabel,
        esApartado,
        getFieldError,
        handleHeaderChange,
        handleConfirmarCrearApartado,
        handleConfirmarCobro,
        closeApartadoModal,
        closeResultadoDrawer,
        confirmarNuevaFactura,
        confirmarGuardarProforma,
        confirmarCrearApartado,
        handleEmitir,
    } = controller;

    return (
        <>
            <ScrollAreaAutosize
                className="h-page"
                offsetScrollbars
                viewportRef={facturacionViewportRef}
            >
                <Stack gap="lg" pb={150}>
                    <FacturacionCobroModal
                        opened={cobro.cobroModalOpen}
                        modo={cobro.cobroModo}
                        total={totales.total}
                        pagado={totales.pagado}
                        saldo={totales.saldo}
                        monedaCodigo={monedaCodigo}
                        equivalenteTotal={equivalenteTotal}
                        monedaEquivalente={monedaEquivalente}
                        pagos={pagos}
                        pagosError={getFieldError(VENTA_FIELDS.PAGOS)}
                        loading={loadingEmitir}
                        onClose={cobro.closeCobroModal}
                        onAgregarPago={cobro.addPagoVacio}
                        onUpdatePago={cobro.updatePago}
                        onRemovePago={cobro.removePago}
                        getFieldError={getFieldError}
                        onConfirm={handleConfirmarCobro}
                    />
                    <FacturacionResultadoDrawer
                        opened={Boolean(state.ultimoDocumentoEmitido)}
                        documento={state.ultimoDocumentoEmitido}
                        vuelto={state.ultimoVueltoEmitido}
                        onClose={closeResultadoDrawer}
                    />
                    <FacturacionApartadoModal
                        opened={state.apartadoModalOpen}
                        fechaVencimiento={state.apartadoFechaVencimiento}
                        minDate={
                            form.values[VENTA_FIELDS.FECHA_DOCUMENTO]
                                ? String(form.values[VENTA_FIELDS.FECHA_DOCUMENTO])
                                : undefined
                        }
                        error={state.apartadoFechaError}
                        loading={loadingGuardar}
                        onClose={closeApartadoModal}
                        onFechaChange={(apartadoFechaVencimiento) =>
                            dispatch({
                                type: "patch",
                                value: { apartadoFechaVencimiento, apartadoFechaError: null },
                            })
                        }
                        onConfirm={handleConfirmarCrearApartado}
                    />

                    {error && (
                        <Alert color="red" variant="light" icon={<IconAlertCircle size={16} />}>
                            {error}
                        </Alert>
                    )}
                    <FacturacionDocumentoCard
                        values={form.values as Record<string, unknown>}
                        errors={form.errors}
                        disabled={false}
                        clienteSeleccionado={state.clienteSeleccionado}
                        aplicaVendedores={aplicaVendedores}
                        vendedores={vendedores}
                        aplicaCajas={aplicaCajas}
                        cajas={cajas}
                        onClienteChange={(value) => form.setFieldValue(VENTA_FIELDS.CLIENTE_ID, value)}
                        onClienteSeleccionadoChange={(clienteSeleccionado) =>
                            dispatch({ type: "patch", value: { clienteSeleccionado } })
                        }
                        onFieldChange={handleHeaderChange}
                    />
                    <FacturacionCreditoBanner
                        clienteId={String(form.values[VENTA_FIELDS.CLIENTE_ID] ?? "") || null}
                        condicionVentaCodigo={String(form.values[VENTA_FIELDS.CONDICION_VENTA_CODIGO] ?? "")}
                        plazoActual={(form.values[VENTA_FIELDS.PLAZO_CREDITO_DIAS] as number | null) ?? null}
                        onSugerirPlazo={(dias) => form.setFieldValue(VENTA_FIELDS.PLAZO_CREDITO_DIAS, dias)}
                    />
                    <FacturacionLineasEditor
                        lineas={lineasEditor.lineas}
                        monedaCodigo={monedaCodigo}
                        disabled={false}
                        lineasError={getFieldError(VENTA_FIELDS.LINEAS)}
                        selectedProductoId={lineasEditor.selectedProductoId}
                        onSelectedProductoIdChange={lineasEditor.setSelectedProductoId}
                        onSelectedProductoChange={lineasEditor.setSelectedProducto}
                        onProductoSeleccionado={lineasEditor.addLinea}
                        onUpdateLinea={lineasEditor.updateLinea}
                        onRemoveLinea={lineasEditor.removeLinea}
                        getFieldError={getFieldError}
                    />
                    <FacturacionObservacionesCard
                        values={form.values as Record<string, unknown>}
                        errors={form.errors}
                        disabled={false}
                        onFieldChange={handleHeaderChange}
                    />
                </Stack>
            </ScrollAreaAutosize>

            <FacturacionFooterBar
                documento={state.documento}
                totales={totales}
                monedaCodigo={monedaCodigo}
                monedaEquivalente={monedaEquivalente}
                equivalenteTotal={equivalenteTotal}
                emitirLabel={emitirLabel}
                estado={{
                    puedeFacturar,
                    canEmit,
                    loadingEmitir,
                    loadingGuardar,
                    showOpcionesCobro,
                    disableCrearApartado: Boolean(state.documentoId),
                    disableGuardarProforma: esApartado,
                }}
                onNuevaFactura={confirmarNuevaFactura}
                onCrearApartado={confirmarCrearApartado}
                onGuardarProforma={confirmarGuardarProforma}
                onEmitir={handleEmitir}
                onCobroDetallado={cobro.abrirCobroDetallado}
            />
        </>
    );
}
