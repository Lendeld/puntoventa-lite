// Recharts loguea "The width(0) and height(0) of chart should be greater than 0"
// durante el doble-mount de React StrictMode (dev). Los charts renderizan bien y
// el warning NO aparece en build de producción. Filtramos SOLO ese mensaje para
// no ensuciar la consola; cualquier otro warn/error pasa intacto.
if (typeof window !== "undefined" && process.env.NODE_ENV !== "production") {
    // Fragmento estable del mensaje (sirva como sea que recharts pase los args).
    const RECHARTS_NOISE = "of chart should be greater than 0";

    const contieneRuido = (args: unknown[]) =>
        args.some((a) => typeof a === "string" && a.includes(RECHARTS_NOISE));

    const filtrar =
        (original: (...args: unknown[]) => void) =>
        (...args: unknown[]) => {
            if (contieneRuido(args)) return;
            original(...args);
        };

    console.warn = filtrar(console.warn.bind(console));
    console.error = filtrar(console.error.bind(console));
}

export {};
