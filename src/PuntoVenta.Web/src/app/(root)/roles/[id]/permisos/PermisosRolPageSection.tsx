"use client";

import { ROUTES } from "@/lib/constants/routes.constants";
import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarPermisosRolAction } from "@lib/actions/roles.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import {
    obtenerPaginasPermisosRolService,
    obtenerPermisosRolPorPaginaService,
} from "@lib/services/roles.service";
import type { PermisoPaginaDto, RolDto } from "@lib/types/roles.types";
import {
    Alert,
    Box,
    Button,
    Flex,
    Group,
    Paper,
    ScrollArea,
    Skeleton,
    Stack,
    Tabs,
    Text,
    ThemeIcon,
} from "@mantine/core";
import { modals } from "@mantine/modals";
import {
    IconAlertCircle,
    IconArrowLeft,
    IconDeviceFloppy,
    IconRefresh,
    IconShieldLock,
} from "@tabler/icons-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useEffect, useMemo, useRef, useState } from "react";
import PermisosSection from "@pages/roles/[id]/permisos/PermisosSection";
import ErrorDataAlert from "@/components/ui/ErrorDataAlert";

interface Props {
    id: string;
    initialRol: RolDto;
}

function getAssignedIds(permisos: PermisoPaginaDto[]): string[] {
    return permisos
        .flatMap((permiso) => (permiso.asignado ? [permiso.permisoId] : []))
        .sort();
}

function areSameIds(left: string[], right: string[]) {
    if (left.length !== right.length) return false;

    const a = left.toSorted();
    const b = right.toSorted();

    return a.length === b.length && a.every((value, index) => value === b[index]);
}

// La página coordina tabs remotos, selección local y guardado atómico de permisos.
// react-doctor-disable-next-line react-doctor/no-giant-component
export default function PermisosRolPageSection({ id, initialRol }: Props) {
    const queryClient = useQueryClient();
    const hydratedTabRef = useRef<string | null>(null);
    const [tabSeleccionada, setTabSeleccionada] = useState<string | null>(null);
    const [selectedPermisos, setSelectedPermisos] = useState<string[]>([]);

    const {
        data: rol,
        isLoading: loadingRol,
        isError: isRolError,
    } = useQuery({
        queryKey: QUERY_KEYS.roles.detalle(id),
        initialData: initialRol,
        queryFn: async () => initialRol,
    });

    const {
        data: paginas,
        isLoading: loadingPaginas,
        isError: isPaginasError,
        refetch: refetchPaginas,
    } = useQuery({
        queryKey: QUERY_KEYS.roles.paginasPermisos(id),
        queryFn: async () => {
            const res = await obtenerPaginasPermisosRolService(id);
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });

    // Tab efectiva derivada: la selección del usuario si sigue siendo válida,
    // si no el primer tab disponible. Evita un efecto de auto-corrección.
    const activeTab =
        tabSeleccionada &&
        paginas?.some((pagina) => pagina.paginaId === tabSeleccionada)
            ? tabSeleccionada
            : (paginas?.[0]?.paginaId ?? null);

    const {
        data: paginaPermisos,
        isLoading: loadingPermisos,
        isFetching: fetchingPermisos,
        isError: isPermisosError,
        refetch: refetchPermisos,
    } = useQuery({
        queryKey: QUERY_KEYS.roles.permisosPagina(id, activeTab ?? ""),
        enabled: !!activeTab,
        queryFn: async () => {
            const res = await obtenerPermisosRolPorPaginaService(
                id,
                activeTab!,
            );
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });

    const permisosAsignados = useMemo(
        () => (paginaPermisos ? getAssignedIds(paginaPermisos.permisos) : []),
        [paginaPermisos],
    );

    useEffect(() => {
        if (!paginaPermisos || !activeTab) return;

        const hydrationKey = `${activeTab}:${permisosAsignados.join(",")}`;
        if (hydratedTabRef.current === hydrationKey) return;

        // Hidrata la selección editable desde la data del server una vez por tab
        // (ref-guard). selectedPermisos luego lo edita el usuario; no es derivable.
        // react-doctor-disable-next-line react-doctor/no-derived-state
        setSelectedPermisos(permisosAsignados);
        hydratedTabRef.current = hydrationKey;
    }, [activeTab, paginaPermisos, permisosAsignados]);

    const isPrincipal = rol.isPrincipal;
    const isDirty = !areSameIds(selectedPermisos, permisosAsignados);

    const {
        execute,
        loading: saving,
        error,
        setError,
    } = useActionHandler({
        onSuccess: async () => {
            if (!activeTab) return;

            hydratedTabRef.current = null;
            await queryClient.invalidateQueries({
                queryKey: QUERY_KEYS.roles.permisosPagina(id, activeTab),
            });

            AppNotifier.success({
                message: "Permisos actualizados exitosamente.",
            });
        },
    });

    function applyTabChange(nextTab: string) {
        hydratedTabRef.current = null;
        setError(null);
        setTabSeleccionada(nextTab);
    }

    function handleTabChange(nextTab: string | null) {
        if (!nextTab || nextTab === activeTab) return;

        if (!isDirty) {
            applyTabChange(nextTab);
            return;
        }

        modals.openConfirmModal({
            title: "Cambios sin guardar",
            centered: true,
            overlayProps: { blur: 3, opacity: 1 },
            children: (
                <Text size="sm">
                    Hay cambios sin guardar en esta página. Si continúas, se
                    perderán.
                </Text>
            ),
            labels: {
                confirm: "Cambiar página",
                cancel: "Seguir editando",
            },
            confirmProps: {
                color: "accentPV",
            },
            onConfirm: () => applyTabChange(nextTab),
        });
    }

    async function handleSave() {
        if (!activeTab || isPrincipal) return;

        await execute(() =>
            actualizarPermisosRolAction(id, activeTab, selectedPermisos),
        );
    }

    async function handleRefresh() {
        setError(null);
        await refetchPaginas();

        if (activeTab) {
            hydratedTabRef.current = null;
            await refetchPermisos();
        }
    }

    const totalPermisos = paginaPermisos?.permisos.length ?? 0;

    return (
        <Stack gap="md" className="h-page pt-1 px-1">
            <Group justify="space-between" align="flex-start">
                <Button
                    component={Link}
                    href={ROUTES.ROLES}
                    variant="outline"
                    leftSection={<IconArrowLeft size={16} />}
                >
                    Volver a roles
                </Button>
                <Group>
                    <Button
                        variant="light"
                        leftSection={<IconRefresh size={16} />}
                        onClick={() => void handleRefresh()}
                        loading={loadingPaginas || fetchingPermisos}
                    >
                        Recargar
                    </Button>
                    <Button
                        leftSection={<IconDeviceFloppy size={16} />}
                        onClick={() => void handleSave()}
                        loading={saving}
                        disabled={
                            !activeTab ||
                            !isDirty ||
                            isPrincipal ||
                            loadingPermisos
                        }
                    >
                        Guardar cambios
                    </Button>
                </Group>
            </Group>
            <Group gap="sm">
                <ThemeIcon
                    size="lg"
                    radius="md"
                    color="accentPV"
                    variant="light"
                >
                    <IconShieldLock size={18} />
                </ThemeIcon>
                <Box>
                    <Text size="sm" c="dimmed">
                        {loadingRol
                            ? "Cargando rol..."
                            : rol
                              ? `${rol.nombre}${rol.descripcion ? ` · ${rol.descripcion}` : ""}`
                              : "No fue posible cargar el rol."}
                    </Text>
                </Box>
            </Group>

            {isRolError && (
                <ErrorDataAlert
                    message="Error al cargar el rol."
                    height="h-[calc(100dvh-252px)]"
                />
            )}
            {error && (
                <Alert
                    icon={<IconAlertCircle size={16} />}
                    color="red"
                    variant="light"
                >
                    {error}
                </Alert>
            )}

            {!loadingPaginas && isPaginasError && (
                <ErrorDataAlert
                    message="Error al cargar las páginas."
                    height="h-[calc(100dvh-252px)]"
                />
            )}

            <Paper
                withBorder
                radius="lg"
                className="flex-1 min-h-0 overflow-hidden border-theme bg-theme-surface"
            >
                <Tabs
                    orientation="vertical"
                    value={activeTab}
                    onChange={handleTabChange}
                    className="h-full overflow-hidden"
                >
                    <Flex className="h-full min-h-0 w-full">
                        <ScrollArea
                            className="min-h-0 w-72 shrink-0 border-r border-theme bg-theme-surface"
                            scrollbarSize={6}
                        >
                            <Tabs.List>
                                {loadingPaginas && (
                                    <Skeleton className="h-[calc(100dvh-252px)]" />
                                )}

                                {!loadingPaginas &&
                                    paginas?.map((pagina) => (
                                        <Tabs.Tab
                                            key={pagina.paginaId}
                                            value={pagina.paginaId}
                                        >
                                            {pagina.nombre}
                                        </Tabs.Tab>
                                    ))}
                            </Tabs.List>
                        </ScrollArea>

                        {!loadingPaginas &&
                            !paginas?.length &&
                            !isPaginasError && (
                                <Group className="flex flex-1 items-center justify-center p-8">
                                    <Stack gap={6} align="center">
                                        <ThemeIcon
                                            size={52}
                                            radius="xl"
                                            color="gray"
                                            variant="light"
                                        >
                                            <IconShieldLock size={24} />
                                        </ThemeIcon>
                                        <Text fw={600}>
                                            No hay páginas configuradas
                                        </Text>
                                        <Text size="sm" c="dimmed">
                                            No existen páginas activas con
                                            permisos para este módulo.
                                        </Text>
                                    </Stack>
                                </Group>
                            )}
                        {activeTab && paginas?.length ? (
                            <PermisosSection
                                activeTab={activeTab}
                                selectedPermisos={selectedPermisos}
                                isDirty={isDirty}
                                isPrincipal={isPrincipal}
                                loadingPermisos={
                                    loadingPermisos || fetchingPermisos
                                }
                                isPermisosError={isPermisosError}
                                setSelectedPermisos={setSelectedPermisos}
                                totalPermisos={totalPermisos}
                                saving={saving}
                                paginaPermisos={paginaPermisos}
                            />
                        ) : null}
                    </Flex>
                </Tabs>
            </Paper>
        </Stack>
    );
}
