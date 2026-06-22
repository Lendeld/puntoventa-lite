import { PermisoPaginaDto } from "@lib/types/roles.types";
import { Grid, Group, Switch, Text } from "@mantine/core";

interface Props {
    permisos: PermisoPaginaDto[];
    selectedPermisos: string[];
    setSelectedPermisos: React.Dispatch<React.SetStateAction<string[]>>;
    saving: boolean;
}

export default function PermisosChecks({
    permisos,
    selectedPermisos,
    setSelectedPermisos,
    saving,
}: Props) {
    function handleTogglePermiso(permisoId: string) {
        setSelectedPermisos((current) =>
            current.includes(permisoId)
                ? current.filter((idActual) => idActual !== permisoId)
                : [...current, permisoId],
        );
    }

    return (
        <Grid gap="xl">
            {permisos.map((permiso) => (
                <Grid.Col span={6} key={permiso.permisoId}>
                    <Group
                        key={permiso.permisoId}
                        align="center"
                        justify="space-between"
                        className="w-full h-full"
                        wrap="nowrap"
                    >
                        <Text fw={600}>{permiso.descripcion}</Text>
                        <Switch
                            className="shrink-0"
                            checked={selectedPermisos.includes(
                                permiso.permisoId,
                            )}
                            onChange={() =>
                                handleTogglePermiso(permiso.permisoId)
                            }
                            disabled={saving}
                            aria-label={permiso.descripcion}
                        />
                    </Group>
                </Grid.Col>
            ))}
        </Grid>
    );
}
