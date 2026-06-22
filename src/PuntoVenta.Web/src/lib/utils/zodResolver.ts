import type { ZodType } from 'zod';

export function zodResolver<T extends Record<string, unknown>>(schema: ZodType<T>) {
  return (values: T): Record<string, string | null> => {
    const result = schema.safeParse(values);
    if (result.success) return {};

    const errors: Record<string, string | null> = {};
    result.error.issues.forEach((issue) => {
      const key = issue.path.join('.');
      if (key && !errors[key]) {
        errors[key] = issue.message;
      }
    });
    return errors;
  };
}
