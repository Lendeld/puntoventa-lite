import type { UseFormReturnType } from '@mantine/form';
import type { ApiValidationErrors } from '@/lib/types/base.types';

export function mapApiErrorsToForm<T extends Record<string, unknown>>(
    form: UseFormReturnType<T>,
    errors: ApiValidationErrors,
): void {
    form.setErrors(errors);
}
