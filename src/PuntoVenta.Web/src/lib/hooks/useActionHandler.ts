'use client';

import { AppNotifier } from '@components/ui/AppNotifier';
import { startNavigationProgress } from '@mantine/nprogress';
import { useState } from 'react';
import type { UseFormReturnType } from '@mantine/form';
import type { ActionResult } from '@lib/types/base.types';
import { resolveApiErrorMessage } from '@lib/utils/apiErrors';
import { mapApiErrorsToForm } from '@lib/utils/formErrors';
import { ROUTES } from '@lib/constants/routes.constants';

const BROADCAST_CHANNEL_NAME = 'pv:auth';

interface UseActionHandlerOptions<
    TValues extends Record<string, unknown>,
    TResult extends ActionResult,
> {
    form?: UseFormReturnType<TValues>;
    forbiddenMessage?: string;
    resolveErrorMessage?: (result: TResult) => string;
    onSuccess?: (result: TResult) => void;
    onForbidden?: (result: TResult, errorMessage: string) => void;
    onUnauthorized?: (result: TResult, errorMessage: string) => void;
    onInternalError?: (result: TResult, errorMessage: string) => void;
    keepLoadingOnSuccess?: boolean;
}

type Severity = NonNullable<ActionResult['severity']>;

export function useActionHandler<
    TValues extends Record<string, unknown> = Record<string, unknown>,
    TResult extends ActionResult = ActionResult,
>(options: UseActionHandlerOptions<TValues, TResult> = {}) {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [errorSeverity, setErrorSeverity] = useState<Severity | null>(null);
    const {
        form,
        forbiddenMessage = 'No tienes permiso para realizar esta acción.',
        resolveErrorMessage,
        onSuccess,
        onForbidden,
        onUnauthorized,
        onInternalError,
        keepLoadingOnSuccess = false,
    } = options;

    function getErrorMessage(result: TResult): string {
        return resolveErrorMessage?.(result) ??
            resolveApiErrorMessage(result.errors, {
                fallback:
                    result.status === 403
                        ? forbiddenMessage
                        : 'Error inesperado.',
            });
    }

    function resolveSeverity(status: number, severity?: TResult['severity']): Severity {
        if (severity) return severity;
        return status >= 500 ? 'error' : 'warning';
    }

    function setHandledMessage(message: string, status: number, severity?: TResult['severity']) {
        const effective = resolveSeverity(status, severity);
        setErrorSeverity(effective);
        if (effective === 'warning') {
            AppNotifier.warning({ message });
            return;
        }
        if (effective === 'info') {
            AppNotifier.info({ message });
            return;
        }
        AppNotifier.error({ message });
    }

    function setHandledForbidden(message: string) {
        setErrorSeverity('warning');
        AppNotifier.warning({ message });
    }

    async function execute(action: () => Promise<TResult>): Promise<TResult | null> {
        setLoading(true);
        setError(null);
        setErrorSeverity(null);

        try {
            const result = await action();

            if (result.status >= 400) {
                setLoading(false);
                const errorMessage =
                    result.status >= 500
                        ? 'Error interno del servidor. Intente nuevamente.'
                        : getErrorMessage(result);

                if (result.status === 401 && onUnauthorized) {
                    onUnauthorized(result, errorMessage);
                    return result;
                }
                else if (result.status === 401) {
                    // Un 401 que llega aquí significa que el retry+refresh de apiClient ya falló:
                    // la sesión está muerta. Cerrar de forma limpia y consistente (igual que
                    // usePermisoQuery y TokenValidator) en vez de dejar la UI en estado roto.
                    if (typeof window !== 'undefined') {
                        try {
                            new BroadcastChannel(BROADCAST_CHANNEL_NAME).postMessage({ type: 'logout' });
                        } catch {
                            // BroadcastChannel no soportado: el redirect basta
                        }
                        window.location.href = ROUTES.API_LOGOUT;
                    }
                    return result;
                }

                if (result.status === 403 && onForbidden) {
                    onForbidden(result, errorMessage);
                    return result;
                } else if (result.status === 403) {
                    setHandledForbidden(errorMessage);
                    return result;
                }

                if (result.status === 400 && form && result.errors) {
                    const formKeys = Object.keys(form.values);
                    const matchedAny = Object.keys(result.errors).some((k) =>
                        formKeys.includes(k),
                    );
                    mapApiErrorsToForm(form, result.errors);
                    if (!matchedAny) {
                        setHandledMessage(
                            "Algunos datos no son válidos. Revisa el formulario e intenta de nuevo.",
                            result.status,
                            result.severity,
                        );
                    }
                    return result;
                }

                if (result.status === 409) {
                    if (form && result.errors) mapApiErrorsToForm(form, result.errors);
                    setHandledMessage(errorMessage, result.status, result.severity);
                    return result;
                }

                if (result.status >= 500 && onInternalError) {
                    onInternalError(result, errorMessage);
                    return result;
                }

                setHandledMessage(errorMessage, result.status, result.severity);
                return result;
            }

            if (keepLoadingOnSuccess) {
                startNavigationProgress();
            } else {
                setLoading(false);
            }
            onSuccess?.(result);
            return result;
        } catch {
            const errorMessage = 'Ocurrió un error inesperado. Intente nuevamente.';
            setLoading(false);
            if (onInternalError) {
                onInternalError(null as unknown as TResult, errorMessage);
            } else {
                setHandledMessage(errorMessage, 500);
            }
            return null;
        }
    }

    return { execute, loading, error, errorSeverity, setError };
}
