"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { generarBackupAction, obtenerVersionSistemaAction, validarBackupAction } from "@lib/actions/backup.actions";
import { esAppEscritorio, getBackupBridge } from "@lib/backup/backup-bridge";
import { BROADCAST_CHANNEL_NAME } from "@lib/constants/auth.constants";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { ROUTES } from "@lib/constants/routes.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import type { VersionSistemaDto } from "@lib/types/backup.types";
import { usePermisoQuery } from "@lib/hooks/usePermisoQuery";
import { obtenerUsuarioActualService } from "@lib/services/auth.service";
import type { UsuarioActualDto } from "@lib/types/auth.types";
import {
    Alert,
    Anchor,
    Badge,
    Box,
    Button,
    Divider,
    Group,
    Loader,
    Modal,
    Paper,
    PinInput,
    Stack,
    Text,
    ThemeIcon,
} from "@mantine/core";
import { modals } from "@mantine/modals";
import {
    IconAlertCircle,
    IconAlertTriangle,
    IconArrowRight,
    IconDatabaseExport,
    IconDatabaseImport,
    IconShieldLock,
} from "@tabler/icons-react";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";

// ---------------------------------------------------------------------------
// Hook: usuario actual
// ---------------------------------------------------------------------------
function useUsuarioActual() {
    return useQuery({
        queryKey: QUERY_KEYS.auth.usuarioActual,
        queryFn: async () => {
            const response = await obtenerUsuarioActualService();
            if (response.errors) throw response.errors;
            return response.data as UsuarioActualDto;
        },
        staleTime: 1000 * 60 * 5,
    });
}

// ---------------------------------------------------------------------------
// Hook: versión de esquema actual de la DB
// ---------------------------------------------------------------------------
function useVersionSistema() {
    return useQuery<VersionSistemaDto | null>({
        queryKey: ["backup", "version-sistema"],
        queryFn: async () => {
            const result = await obtenerVersionSistemaAction();
            return result.data ?? null;
        },
        staleTime: Infinity,
    });
}

// ---------------------------------------------------------------------------
// Modal de confirmación PIN (reutilizable para generar y restaurar)
// ---------------------------------------------------------------------------
interface PinModalProps {
    opened: boolean;
    titulo: string;
    descripcion: string;
    confirmLabel: string;
    onConfirm: (pin: string) => void;
    onClose: () => void;
    loading: boolean;
    /** Error del backend (ej. PIN incorrecto) para mostrar dentro del modal, no como toast. */
    errorExterno?: string | null;
    /** Limpia el error externo cuando el usuario vuelve a escribir el PIN. */
    onClearError?: () => void;
}

function PinConfirmModal({
    opened,
    titulo,
    descripcion,
    confirmLabel,
    onConfirm,
    onClose,
    loading,
    errorExterno,
    onClearError,
}: PinModalProps) {
    const [pin, setPin] = useState("");
    const [pinError, setPinError] = useState<string | null>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    // Error a mostrar: el de validación local tiene prioridad; si no, el del backend.
    const mensajeError = pinError ?? errorExterno ?? null;

    // Al abrir: enfocar la primera casilla del PIN (con delay para ganarle al focus-trap
    // del Modal, que por defecto enfoca el botón de cerrar). Al cerrar: limpiar el PIN
    // (el padre cierra por éxito sin pasar por handleClose).
    useEffect(() => {
        if (opened) {
            const t = setTimeout(() => inputRef.current?.focus(), 120);
            return () => clearTimeout(t);
        }
        setPin("");
        setPinError(null);
    }, [opened]);

    // valor opcional: onComplete (auto al 6º dígito) lo pasa directo para evitar leer
    // el estado `pin` aún sin actualizar (stale closure). El botón llama sin args y usa el estado.
    function handleConfirm(valor?: string) {
        const pinActual = valor ?? pin;
        if (!/^\d{6}$/.test(pinActual)) {
            setPinError("Ingresa los 6 dígitos del PIN.");
            return;
        }
        setPinError(null);
        onConfirm(pinActual);
    }

    function handleClose() {
        setPin("");
        setPinError(null);
        onClose();
    }

    return (
        <Modal
            opened={opened}
            onClose={handleClose}
            title={titulo}
            centered
            closeOnClickOutside={!loading}
            closeOnEscape={!loading}
            withCloseButton={!loading}
        >
            <Stack gap="md">
                <Text size="sm" c="dimmed">
                    {descripcion}
                </Text>

                <Stack gap="xs">
                    <Group justify="center">
                        <PinInput
                            ref={inputRef}
                            length={6}
                            type="number"
                            mask
                            value={pin}
                            onChange={(val) => {
                                setPin(val);
                                setPinError(null);
                                onClearError?.();
                            }}
                            onComplete={(val) => handleConfirm(val)}
                            disabled={loading}
                            error={!!mensajeError}
                        />
                    </Group>
                    {mensajeError && (
                        <Text size="xs" c="red" ta="center">
                            {mensajeError}
                        </Text>
                    )}
                </Stack>

                <Group justify="flex-end">
                    <Button variant="outline" onClick={handleClose} disabled={loading}>
                        Cancelar
                    </Button>
                    <Button loading={loading} onClick={() => handleConfirm()}>
                        {confirmLabel}
                    </Button>
                </Group>
            </Stack>
        </Modal>
    );
}

// ---------------------------------------------------------------------------
// Sección principal
// ---------------------------------------------------------------------------
export default function RespaldoPageSection() {
    const router = useRouter();
    const { data: usuario, isLoading: loadingUsuario } = useUsuarioActual();
    const { data: tienePermiso, isLoading: loadingPermiso } = usePermisoQuery(
        PERMISOS.BACKUP_ADMINISTRAR,
    );
    const { data: versionSistema } = useVersionSistema();

    const esDesktop = esAppEscritorio();

    // Estado de modals PIN
    const [modalGenerar, setModalGenerar] = useState(false);
    const [modalRestaurar, setModalRestaurar] = useState(false);
    const [rutaRestaurar, setRutaRestaurar] = useState<string | null>(null);

    // Error del backend (PIN incorrecto, etc.) a mostrar dentro del modal PIN abierto.
    const [errorPin, setErrorPin] = useState<string | null>(null);

    // Loading states
    const [generandoBackup, setGenerandoBackup] = useState(false);
    const [restaurandoBackup, setRestaurandoBackup] = useState(false);

    // Ruta de destino de generar backup (ref para evitar cierre de stale en async)
    const rutaDestinoRef = useRef<string | null>(null);

    // ---------------------------------------------------------------------------
    // Flujo: generar backup
    // ---------------------------------------------------------------------------
    async function handleIniciarGenerar() {
        if (!esDesktop) return;
        const bridge = getBackupBridge();
        if (!bridge) return;

        const ruta = await bridge.elegirDestino();
        if (!ruta) return; // cancelado

        rutaDestinoRef.current = ruta;
        setErrorPin(null);
        setModalGenerar(true);
    }

    async function handleConfirmarGenerar(pin: string) {
        const rutaDestino = rutaDestinoRef.current;
        if (!rutaDestino) return;

        setGenerandoBackup(true);
        try {
            const result: Awaited<ReturnType<typeof generarBackupAction>> = await generarBackupAction(pin, rutaDestino);
            if (result.status >= 400) {
                const errors = result.errors ?? {};
                const msg =
                    errors["Usuario_Pin"] ??
                    errors["general"] ??
                    Object.values(errors)[0] ??
                    "Error al generar el respaldo.";
                // El error (PIN incorrecto, etc.) se muestra dentro del modal, no como toast.
                setErrorPin(msg);
                return;
            }
            setModalGenerar(false);
            rutaDestinoRef.current = null;
            AppNotifier.success({
                message: `Respaldo generado exitosamente en: ${rutaDestino}`,
            });
        } finally {
            setGenerandoBackup(false);
        }
    }

    // ---------------------------------------------------------------------------
    // Flujo: restaurar backup
    // ---------------------------------------------------------------------------
    async function handleIniciarRestaurar() {
        if (!esDesktop) return;
        const bridge = getBackupBridge();
        if (!bridge) return;

        const ruta = await bridge.elegirOrigen();
        if (!ruta) return; // cancelado

        setRutaRestaurar(ruta);
        setErrorPin(null);
        setModalRestaurar(true);
    }

    async function handleConfirmarRestaurar(pin: string) {
        if (!rutaRestaurar) return;
        const bridge = getBackupBridge();
        if (!bridge) return;

        setRestaurandoBackup(true);
        try {
            // 1. Validar el backup con el backend (PIN + versión de esquema)
            const validacion: Awaited<ReturnType<typeof validarBackupAction>> = await validarBackupAction(rutaRestaurar, pin);

            if (validacion.status >= 400) {
                const errors = validacion.errors ?? {};
                const msg =
                    errors["Usuario_Pin"] ??
                    errors["general"] ??
                    Object.values(errors)[0] ??
                    "Error al validar el respaldo.";
                // El error (PIN incorrecto, etc.) se muestra dentro del modal, no como toast.
                setErrorPin(msg);
                setRestaurandoBackup(false);
                return;
            }

            const validacionData = validacion.data;
            if (!validacionData) {
                AppNotifier.error({ message: "No se recibió respuesta de validación." });
                setRestaurandoBackup(false);
                return;
            }

            // 2. Si versión incompatible, mostrar detalle y abortar
            if (!validacionData.esCompatible) {
                setModalRestaurar(false);
                setRutaRestaurar(null);
                setRestaurandoBackup(false);
                AppNotifier.error({
                    message: `El respaldo no es compatible con la versión actual. Versión del respaldo: ${validacionData.versionBackup}. Versión de la app: ${validacionData.versionApp}.`,
                });
                return;
            }

            // 3. Confirmar la operación destructiva
            setModalRestaurar(false);
            setRestaurandoBackup(false);

            const rutaParaSwap = rutaRestaurar;
            const tokenParaSwap = validacionData.tokenRestauracion;
            setRutaRestaurar(null);

            modals.openConfirmModal({
                title: "Confirmar restauración",
                centered: true,
                closeOnClickOutside: false,
                overlayProps: { blur: 3 },
                children: (
                    <Stack gap="sm">
                        <Alert
                            icon={<IconAlertTriangle size={16} />}
                            color="red"
                            variant="light"
                        >
                            <Text size="sm" fw={700}>
                                Esta acción reemplazará TODA la base de datos actual con el respaldo
                                seleccionado y cerrará tu sesión.
                            </Text>
                        </Alert>
                        <Text size="sm" c="dimmed">
                            Los datos actuales serán reemplazados. Asegúrate de tener un respaldo
                            reciente antes de continuar. Esta acción no se puede deshacer.
                        </Text>
                    </Stack>
                ),
                labels: { confirm: "Restaurar y cerrar sesión", cancel: "Cancelar" },
                confirmProps: { color: "red" },
                cancelProps: { variant: "outline" },
                onConfirm: () => void ejecutarSwap(rutaParaSwap, tokenParaSwap),
            });
        } catch {
            AppNotifier.error({ message: "Ocurrió un error inesperado." });
            setRestaurandoBackup(false);
        }
    }

    async function ejecutarSwap(ruta: string, token: string) {
        const bridge = getBackupBridge();
        if (!bridge) return;

        setRestaurandoBackup(true);
        try {
            const resultado = await bridge.restaurar(ruta, token);
            if (!resultado.ok) {
                AppNotifier.error({
                    message: resultado.error ?? "Error al restaurar el respaldo.",
                });
                // Si hubo rollback, el API se relanzó (posible puerto nuevo) y Next quedó
                // inconsistente: relanzar la app completa para volver a un estado limpio.
                if (resultado.requiereReinicio) {
                    setTimeout(() => bridge.reiniciarApp(), 1500);
                } else {
                    setRestaurandoBackup(false);
                }
                return;
            }

            // Swap exitoso. Limpiar sesión del renderer antes de reiniciar.
            try {
                new BroadcastChannel(BROADCAST_CHANNEL_NAME).postMessage({ type: "logout" });
            } catch { /* BroadcastChannel no soportado */ }

            if (resultado.requiereReinicio) {
                // En build empaquetado el API arrancó en un puerto efímero nuevo;
                // Next sigue apuntando al puerto anterior (recibido en BASE_URL_API al fork).
                // Relanzar la app garantiza que el nuevo ciclo API + Next arranque
                // con puertos consistentes desde el inicio. El relaunch lo orquesta
                // Electron main con app.relaunch() + app.exit(0).
                AppNotifier.success({ message: "Restauración exitosa. La aplicación se reiniciará." });
                // Pequeño delay para que la notificación sea visible antes del cierre
                setTimeout(() => bridge.reiniciarApp(), 1500);
            } else {
                // Fallback (dev con puerto fijo): navegar a login en el proceso actual
                router.replace(ROUTES.LOGIN);
            }
        } catch {
            AppNotifier.error({ message: "Ocurrió un error inesperado al restaurar." });
            setRestaurandoBackup(false);
        }
    }

    // ---------------------------------------------------------------------------
    // Render
    // ---------------------------------------------------------------------------
    if (loadingPermiso || loadingUsuario) {
        return (
            <Stack align="center" py="xl">
                <Loader color="accentPV" />
            </Stack>
        );
    }

    if (!tienePermiso) {
        return (
            <Alert icon={<IconAlertCircle size={16} />} color="orange" variant="light">
                No tienes permiso para acceder a esta sección.
            </Alert>
        );
    }

    // Solo disponible en la app de escritorio
    if (!esDesktop) {
        return (
            <Stack gap="lg">
                <Alert icon={<IconAlertCircle size={16} />} color="blue" variant="light">
                    <Text fw={600}>Solo disponible en la app de escritorio</Text>
                    <Text size="sm" mt={4}>
                        La función de respaldo y restauración solo está disponible en la app de
                        escritorio de Punto Venta Lite.
                    </Text>
                </Alert>
            </Stack>
        );
    }

    // Bloquear si el usuario no tiene PIN configurado
    if (usuario && !usuario.tienePin) {
        return (
            <Stack gap="lg">
                <Alert
                    icon={<IconShieldLock size={16} />}
                    color="orange"
                    variant="light"
                    title="PIN de seguridad requerido"
                >
                    <Stack gap="xs">
                        <Text size="sm">
                            Para usar el respaldo y restauración debes configurar primero tu PIN de
                            seguridad. El PIN protege acciones sensibles como esta.
                        </Text>
                        <Anchor component={Link} href={`${ROUTES.MI_PERFIL}?tab=pin`} size="sm" fw={600}>
                            <Group component="span" gap={4} align="center" wrap="nowrap">
                                Configurar PIN ahora
                                <IconArrowRight size={12} />
                            </Group>
                        </Anchor>
                    </Stack>
                </Alert>
            </Stack>
        );
    }

    return (
        <Stack gap="lg">
            {/* Tarjeta: Generar respaldo */}
            <Paper withBorder radius="lg" className="bg-theme-surface overflow-hidden">
                <Group
                    align="flex-start"
                    gap="md"
                    className="w-full border-b border-theme p-5"
                    wrap="nowrap"
                >
                    <ThemeIcon size="lg" radius="md" color="accentPV" variant="light">
                        <IconDatabaseExport size={20} />
                    </ThemeIcon>
                    <Stack gap={2}>
                        <Group gap="xs" align="center">
                            <Text fw={700}>Generar respaldo</Text>
                            {versionSistema && (
                                <Badge size="xs" variant="outline" color="gray">
                                    Versión {versionSistema}
                                </Badge>
                            )}
                        </Group>
                        <Text size="sm" c="dimmed">
                            Exporta una copia consistente de la base de datos local. El archivo
                            generado puede usarse para restaurar el sistema ante cualquier problema.
                        </Text>
                    </Stack>
                </Group>
                <Stack gap="md" p="lg">
                    <Box>
                        <Text size="sm" c="dimmed" mb="md">
                            Se generará un archivo <Badge component="span" size="sm" variant="light">.db</Badge> con
                            una copia segura de todos los datos actuales. Se te pedirá confirmar con
                            tu PIN de seguridad.
                        </Text>
                        <Button
                            leftSection={<IconDatabaseExport size={16} />}
                            onClick={() => void handleIniciarGenerar()}
                            loading={generandoBackup}
                        >
                            Generar respaldo
                        </Button>
                    </Box>
                </Stack>
            </Paper>

            <Divider label="Restauración" labelPosition="center" />

            {/* Tarjeta: Restaurar respaldo */}
            <Paper withBorder radius="lg" className="bg-theme-surface overflow-hidden">
                <Group
                    align="flex-start"
                    gap="md"
                    className="w-full border-b border-theme p-5"
                    wrap="nowrap"
                >
                    <ThemeIcon size="lg" radius="md" color="red" variant="light">
                        <IconDatabaseImport size={20} />
                    </ThemeIcon>
                    <Stack gap={2}>
                        <Text fw={700}>Restaurar respaldo</Text>
                        <Text size="sm" c="dimmed">
                            Importa un respaldo previo. Esta operación reemplaza la base de datos
                            actual y cierra tu sesión.
                        </Text>
                    </Stack>
                </Group>
                <Stack gap="md" p="lg">
                    <Alert
                        icon={<IconAlertTriangle size={16} />}
                        color="red"
                        variant="light"
                    >
                        <Text size="sm" fw={700}>
                            Advertencia: la restauración es irreversible.
                        </Text>
                        <Text size="sm" mt={4}>
                            Todos los datos actuales serán reemplazados por los del respaldo. Solo
                            son compatibles respaldos de la misma versión del sistema.
                        </Text>
                    </Alert>
                    <Box>
                        <Button
                            leftSection={<IconDatabaseImport size={16} />}
                            color="red"
                            variant="outline"
                            onClick={() => void handleIniciarRestaurar()}
                            loading={restaurandoBackup}
                        >
                            Restaurar respaldo
                        </Button>
                    </Box>
                </Stack>
            </Paper>

            {/* Modal PIN — generar */}
            <PinConfirmModal
                opened={modalGenerar}
                titulo="Confirmar respaldo"
                descripcion="Ingresa tu PIN de seguridad para confirmar la generación del respaldo."
                confirmLabel="Generar respaldo"
                onConfirm={(pin) => void handleConfirmarGenerar(pin)}
                onClose={() => {
                    setModalGenerar(false);
                    setErrorPin(null);
                    rutaDestinoRef.current = null;
                }}
                loading={generandoBackup}
                errorExterno={errorPin}
                onClearError={() => setErrorPin(null)}
            />

            {/* Modal PIN — restaurar */}
            <PinConfirmModal
                opened={modalRestaurar}
                titulo="Confirmar restauración"
                descripcion="Ingresa tu PIN de seguridad para validar y restaurar el respaldo."
                confirmLabel="Validar y continuar"
                onConfirm={(pin) => void handleConfirmarRestaurar(pin)}
                onClose={() => {
                    setModalRestaurar(false);
                    setErrorPin(null);
                    setRutaRestaurar(null);
                }}
                loading={restaurandoBackup}
                errorExterno={errorPin}
                onClearError={() => setErrorPin(null)}
            />
        </Stack>
    );
}
