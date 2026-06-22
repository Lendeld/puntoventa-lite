export interface ResultadoBackup {
    ok: boolean;
    error?: string;
    /** Presente en restore exitoso: indica que la app debe reiniciarse para
     *  que el nuevo puerto efímero del API sea consistente con Next. */
    requiereReinicio?: boolean;
}

export interface PulpoBackup {
    /** Abre el diálogo nativo "Guardar como" y devuelve la ruta elegida o null si se canceló. */
    elegirDestino(): Promise<string | null>;
    /** Abre el diálogo nativo "Abrir" y devuelve la ruta del archivo .db elegido o null si se canceló. */
    elegirOrigen(): Promise<string | null>;
    /**
     * Orquesta el swap de la DB viva con el backup:
     * consume el token de capacidad contra el backend (autorización), para el API
     * child, hace backup defensivo, copia el archivo, rearranca.
     * Devuelve { ok: true, requiereReinicio: true } si el swap fue exitoso.
     * El renderer debe llamar reiniciarApp() al recibir requiereReinicio para
     * relanzar la app completa (API + Next + puertos frescos).
     * @param token Token de un solo uso devuelto por validarBackupAction (tokenRestauracion).
     */
    restaurar(rutaOrigen: string, token: string): Promise<ResultadoBackup>;
    /**
     * Relanza la app completa (app.relaunch() + app.exit(0)).
     * Debe llamarse tras un restore exitoso para que el API arranque en un
     * puerto nuevo y Next reciba BASE_URL_API actualizado.
     */
    reiniciarApp(): void;
}

declare global {
    interface Window {
        pulpoBackup?: PulpoBackup;
    }
}
