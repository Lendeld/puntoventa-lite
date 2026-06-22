import PermisosChecks from "@/app/(root)/roles/[id]/permisos/PermisosChecks";
import { PermisosRolPorPaginaDto } from "@/lib/types/roles.types";
import {
    Alert,
    Badge,
    Flex,
    Group,
    ScrollArea,
    Skeleton,
    Stack,
    Text,
} from "@mantine/core";
import { IconAlertCircle } from "@tabler/icons-react";
import { Dispatch, SetStateAction } from "react";

interface Props {
    activeTab: string;
    paginaPermisos?: PermisosRolPorPaginaDto;
    selectedPermisos: string[];
    totalPermisos: number;
    setSelectedPermisos: Dispatch<SetStateAction<string[]>>;
    loadingPermisos: boolean;
    isPermisosError: boolean;
    isDirty: boolean;
    isPrincipal: boolean;
    saving: boolean;
}

export default function PermisosSection({
    paginaPermisos,
    selectedPermisos,
    totalPermisos,
    setSelectedPermisos,
    loadingPermisos,
    isPermisosError,
    isDirty,
    isPrincipal,
    saving,
}: Props) {
    return (
        <Flex className="flex-1 min-w-0 flex-col bg-theme-surface">
            <Group
                justify="space-between"
                className="w-full border-b border-theme p-4"
            >
                <Stack gap={2} className="min-w-0 flex-1">
                    <Group gap="xs">
                        <Text fw={700} size="lg">
                            {paginaPermisos?.nombre ?? "Cargando permisos..."}
                        </Text>
                        <Badge variant="light" color="accentPV">
                            {selectedPermisos.length}/{totalPermisos}
                        </Badge>
                        {isDirty && !isPrincipal && (
                            <Badge variant="light" color="yellow">
                                Sin guardar
                            </Badge>
                        )}
                    </Group>
                    <Text size="sm" c="dimmed">
                        Selecciona permisos que aplican para esta página.
                    </Text>
                </Stack>
            </Group>

            <ScrollArea className="flex-1 min-h-0 min-w-0" scrollbarSize={6}>
                <Stack gap="md" p="lg" className="min-w-0">
                    {loadingPermisos && (
                        <Skeleton className="h-[calc(100dvh-360px)]" />
                    )}

                    {!loadingPermisos && isPermisosError && (
                        <Alert
                            icon={<IconAlertCircle size={16} />}
                            color="red"
                            variant="light"
                        >
                            Error al cargar permisos de la página.
                        </Alert>
                    )}

                    {!loadingPermisos && !isPermisosError && (
                        <PermisosChecks
                            permisos={paginaPermisos?.permisos || []}
                            selectedPermisos={selectedPermisos}
                            setSelectedPermisos={setSelectedPermisos}
                            saving={saving}
                        />
                    )}
                </Stack>
            </ScrollArea>
        </Flex>
    );
}
