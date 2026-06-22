"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { getBridge, esAppEscritorio } from "@lib/printing/electron-bridge";
import type { PerfilImpresoraTicketDto } from "@lib/types/impresion.types";
import type { ConfigImpresionLocal, ImpresoraInfo } from "@lib/types/impresion-bridge";
import {
    Alert,
    Badge,
    Button,
    Group,
    Loader,
    NumberInput,
    Paper,
    Select,
    Skeleton,
    Stack,
    Switch,
    Text,
    TextInput,
} from "@mantine/core";
import {
    IconAlertCircle,
    IconPrinterOff,
    IconPrinter,
    IconPlugConnected,
} from "@tabler/icons-react";
import { useEffect, useState } from "react";

interface Props {
    perfiles: PerfilImpresoraTicketDto[] | undefined;
    perfilesLoading: boolean;
    perfilesError: boolean;
}

const TCP_REGEX = /^tcp:\/\/.+:\d{1,5}$/;

function validarDesignacion(value: string): string | null {
    if (!value) return null;
    if (TCP_REGEX.test(value)) return null;
    return 'Formato inválido. Usa tcp://host:puerto (por ej. tcp://192.168.1.50:9100).';
}

export function EstaComputadoraSection({
    perfiles,
    perfilesLoading,
    perfilesError,
}: Props) {
    // La detección del bridge depende de `window.pulpoImpresion`, que no existe
    // en SSR. Evaluarla en render causa hydration mismatch (server: sin bridge;
    // cliente desktop: con bridge). Se difiere a después de montar: `null` =
    // aún sin determinar → placeholder neutro idéntico en server y primer paint.
    const [hayBridge, setHayBridge] = useState<boolean | null>(null);
    useEffect(() => {
        setHayBridge(esAppEscritorio());
    }, []);

    // -----------------------------------------------------------------------
    // State
    // -----------------------------------------------------------------------
    const [impresoras, setImpresoras] = useState<ImpresoraInfo[]>([]);
    const [loadingImpresoras, setLoadingImpresoras] = useState(false);

    const [impresora, setImpresora] = useState<string>("");
    const [designacionManual, setDesignacionManual] = useState<string>("");
    const [designacionError, setDesignacionError] = useState<string | null>(null);
    const [perfilClave, setPerfilClave] = useState<string | null>(null);
    const [abrirGaveta, setAbrirGaveta] = useState(false);
    const [copias, setCopias] = useState<number>(1);

    // Config persistida al cargar / último guardado, para detectar cambios sin guardar.
    const [baseline, setBaseline] = useState<ConfigImpresionLocal | null>(null);

    const [guardando, setGuardando] = useState(false);
    const [loadingPrueba, setLoadingPrueba] = useState(false);
    const [loadingGaveta, setLoadingGaveta] = useState(false);

    // -----------------------------------------------------------------------
    // Load existing config + printers on mount
    // -----------------------------------------------------------------------
    useEffect(() => {
        if (!hayBridge) return;
        const bridge = getBridge();
        if (!bridge) return;

        void (async () => {
            setLoadingImpresoras(true);
            try {
                const [lista, cfg] = await Promise.all([
                    bridge.listarImpresoras(),
                    bridge.obtenerConfig(),
                ]);
                setImpresoras(lista);
                if (cfg) {
                    setImpresora(cfg.impresora ?? "");
                    setPerfilClave(cfg.perfilClave ?? null);
                    setAbrirGaveta(cfg.abrirGavetaAlCobrar);
                    setCopias(Math.min(Math.max(cfg.copias, 1), 3));
                }
                setBaseline({
                    impresora: cfg?.impresora ?? null,
                    perfilClave: cfg?.perfilClave ?? null,
                    abrirGavetaAlCobrar: cfg?.abrirGavetaAlCobrar ?? false,
                    copias: cfg ? Math.min(Math.max(cfg.copias, 1), 3) : 1,
                });
            } finally {
                setLoadingImpresoras(false);
            }
        })();
    }, [hayBridge]);

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------
    function resolverImpresora(): string {
        return designacionManual.trim() || impresora;
    }

    function resolverPerfil(): PerfilImpresoraTicketDto | null {
        return perfiles?.find((p) => p.clave === perfilClave) ?? null;
    }

    function buildConfig(): ConfigImpresionLocal {
        return {
            impresora: resolverImpresora() || null,
            perfilClave,
            abrirGavetaAlCobrar: abrirGaveta,
            copias,
        };
    }

    // -----------------------------------------------------------------------
    // Handlers
    // -----------------------------------------------------------------------
    async function handleGuardar() {
        const desError = validarDesignacion(designacionManual);
        setDesignacionError(desError);
        if (desError) return;

        const bridge = getBridge();
        if (!bridge) return;

        setGuardando(true);
        try {
            const cfg = buildConfig();
            await bridge.guardarConfig(cfg);
            setBaseline(cfg);
            AppNotifier.success({ message: "Configuración de impresora guardada." });
        } catch {
            AppNotifier.error({ message: "No se pudo guardar la configuración." });
        } finally {
            setGuardando(false);
        }
    }

    async function handleImprimirPrueba() {
        const bridge = getBridge();
        if (!bridge) return;
        const perfil = resolverPerfil();
        const impresoraElegida = resolverImpresora();

        if (!impresoraElegida) {
            AppNotifier.warning({ message: "Seleccioná o ingresá una impresora primero." });
            return;
        }
        if (!perfil) {
            AppNotifier.warning({ message: "Seleccioná un perfil de impresora primero." });
            return;
        }

        setLoadingPrueba(true);
        try {
            const res = await bridge.imprimirPrueba({ impresora: impresoraElegida, perfil });
            if (res.ok) {
                AppNotifier.success({ message: "Impresión de prueba enviada." });
            } else {
                AppNotifier.error({ message: res.error ?? "Error al imprimir prueba." });
            }
        } finally {
            setLoadingPrueba(false);
        }
    }

    async function handleProbarGaveta() {
        const bridge = getBridge();
        if (!bridge) return;
        const perfil = resolverPerfil();
        const impresoraElegida = resolverImpresora();

        if (!impresoraElegida) {
            AppNotifier.warning({ message: "Seleccioná o ingresá una impresora primero." });
            return;
        }
        if (!perfil) {
            AppNotifier.warning({ message: "Seleccioná un perfil de impresora primero." });
            return;
        }

        setLoadingGaveta(true);
        try {
            const res = await bridge.abrirGaveta({ impresora: impresoraElegida, perfil });
            if (res.ok) {
                AppNotifier.success({ message: "Señal de gaveta enviada." });
            } else {
                AppNotifier.error({ message: res.error ?? "Error al abrir gaveta." });
            }
        } finally {
            setLoadingGaveta(false);
        }
    }

    // -----------------------------------------------------------------------
    // Placeholder neutro hasta determinar el bridge (evita hydration mismatch)
    // -----------------------------------------------------------------------
    if (hayBridge === null) {
        return (
            <Paper className="bg-theme-surface">
                <Stack p="lg" gap="sm">
                    <Group gap="xs">
                        <IconPrinter size={20} />
                        <Text fw={700} size="lg">
                            Esta computadora
                        </Text>
                    </Group>
                    <Skeleton h={36} />
                    <Skeleton h={36} />
                </Stack>
            </Paper>
        );
    }

    // -----------------------------------------------------------------------
    // No-bridge fallback
    // -----------------------------------------------------------------------
    if (!hayBridge) {
        return (
            <Paper className="bg-theme-surface">
                <Stack p="lg" gap="sm">
                    <Group gap="xs">
                        <IconPrinterOff size={20} />
                        <Text fw={700} size="lg">
                            Esta computadora
                        </Text>
                    </Group>
                    <Alert
                        icon={<IconAlertCircle size={16} />}
                        color="blue"
                        variant="light"
                    >
                        La configuración de impresora solo está disponible en la
                        app de escritorio.
                    </Alert>
                </Stack>
            </Paper>
        );
    }

    // -----------------------------------------------------------------------
    // Bridge UI
    // -----------------------------------------------------------------------
    const impresorasData = [
        ...impresoras.map((i) => ({
            value: i.nombre,
            label: `${i.nombre}${i.esDefault ? " (predeterminada)" : ""} [${i.origen}]`,
        })),
    ];

    const perfilesData =
        perfiles?.map((p) => ({
            value: p.clave,
            label: `${p.nombre} (${p.anchoMm} mm)`,
        })) ?? [];

    const dirty =
        baseline !== null &&
        JSON.stringify(buildConfig()) !== JSON.stringify(baseline);

    return (
        <Paper className="bg-theme-surface">
            <Stack p="lg" gap="md">
                <Group gap="xs">
                    <IconPrinter size={20} />
                    <Text fw={700} size="lg">
                        Esta computadora
                    </Text>
                    {dirty && (
                        <Badge variant="light" color="yellow">
                            Sin guardar
                        </Badge>
                    )}
                </Group>

                {loadingImpresoras ? (
                    <Stack gap="sm">
                        <Skeleton h={36} />
                        <Skeleton h={36} />
                    </Stack>
                ) : (
                    <Stack gap="md">
                        {/* Printer selector */}
                        <Select
                            label="Impresora"
                            description="Impresoras detectadas en esta computadora."
                            placeholder="Seleccionar impresora"
                            data={impresorasData}
                            value={impresora || null}
                            onChange={(v) => setImpresora(v ?? "")}
                            clearable
                        />

                        {/* Manual designation */}
                        <TextInput
                            label="Designación manual (opcional)"
                            description="Si la impresora no aparece en la lista, ingresá la dirección TCP directamente."
                            placeholder="tcp://192.168.1.50:9100"
                            value={designacionManual}
                            onChange={(e) => {
                                setDesignacionManual(e.currentTarget.value);
                                setDesignacionError(null);
                            }}
                            error={designacionError}
                        />

                        {/* Profile selector */}
                        {perfilesLoading && <Loader size="xs" />}
                        {!perfilesLoading && perfilesError && (
                            <Alert icon={<IconAlertCircle size={14} />} color="red" variant="light">
                                No se pudieron cargar los perfiles de impresora.
                            </Alert>
                        )}
                        {!perfilesLoading && !perfilesError && (
                            <Select
                                label="Perfil de impresora"
                                description="Define el ancho del papel y otras características del hardware."
                                placeholder="Seleccionar perfil"
                                data={perfilesData}
                                value={perfilClave}
                                onChange={setPerfilClave}
                                clearable
                            />
                        )}

                        {/* Drawer + copies */}
                        <Switch
                            label="Abrir gaveta al cobrar"
                            description="Envía la señal de apertura a la gaveta de efectivo después de imprimir."
                            checked={abrirGaveta}
                            onChange={(e) => setAbrirGaveta(e.currentTarget.checked)}
                        />

                        <NumberInput
                            label="Copias"
                            description="Cantidad de copias a imprimir por documento (1-3)."
                            value={copias}
                            min={1}
                            max={3}
                            allowDecimal={false}
                            onChange={(v) =>
                                setCopias(
                                    typeof v === "number"
                                        ? Math.min(Math.max(Math.trunc(v), 1), 3)
                                        : 1,
                                )
                            }
                            w={120}
                        />

                        {/* Action buttons */}
                        <Group gap="sm" wrap="wrap">
                            <Button
                                leftSection={<IconPlugConnected size={16} />}
                                loading={loadingPrueba}
                                variant="light"
                                onClick={() => void handleImprimirPrueba()}
                            >
                                Imprimir prueba
                            </Button>
                            <Button
                                leftSection={<IconPlugConnected size={16} />}
                                loading={loadingGaveta}
                                variant="light"
                                onClick={() => void handleProbarGaveta()}
                            >
                                Probar gaveta
                            </Button>
                            <Button
                                loading={guardando}
                                disabled={!dirty}
                                onClick={() => void handleGuardar()}
                            >
                                Guardar configuración
                            </Button>
                        </Group>
                    </Stack>
                )}
            </Stack>
        </Paper>
    );
}
