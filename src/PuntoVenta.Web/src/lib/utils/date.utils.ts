import dayjs from "dayjs";
import "dayjs/locale/es";
import utc from "dayjs/plugin/utc.js";
import timezone from "dayjs/plugin/timezone.js";

dayjs.extend(utc);
dayjs.extend(timezone);

const CR_TZ = "America/Costa_Rica";

export type DateFormat = "date" | "time" | "datetime" | "datetime-ampm" | "datetime-full";

const DATE_FORMATS: Record<DateFormat, string> = {
    date: "DD/MM/YYYY",
    time: "hh:mm A",
    datetime: "DD/MM/YYYY hh:mm A",
    "datetime-ampm": "DD/MM/YYYY hh:mm A",
    "datetime-full": "DD/MM/YYYY hh:mm:ss A",
};

export function formatDate(
    value: Date | string | null | undefined,
    format: DateFormat = "datetime-ampm",
    locale = "es",
): string {
    if (!value) return "—";

    const date = dayjs(value).locale(locale);

    if (!date.isValid()) return "—";

    return date.format(DATE_FORMATS[format]);
}

// true si la fecha UTC cae en un día (zona CR) anterior a hoy (zona CR).
// Se usa para detectar arqueos abiertos que debieron cerrarse en un día previo.
export function esDiaAnteriorEnCR(utcIso: string | null | undefined): boolean {
    if (!utcIso) return false;
    const dia = dayjs(utcIso).tz(CR_TZ);
    if (!dia.isValid()) return false;
    return dia.isBefore(dayjs().tz(CR_TZ), "day");
}
