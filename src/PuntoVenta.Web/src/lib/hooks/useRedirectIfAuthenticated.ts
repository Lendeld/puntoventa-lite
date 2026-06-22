import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { obtenerRutaPostAutenticacionAction } from "@lib/actions/auth.actions";

export function useRedirectIfAuthenticated() {
    const router = useRouter();

    useEffect(() => {
        async function redirigirSiAutenticado() {
            const ruta = await obtenerRutaPostAutenticacionAction();
            // Redirect depende del resultado de una server action async + se
            // re-evalúa en restauración bfcache (pageshow); no puede ser un
            // redirect() server-side.
            // react-doctor-disable-next-line react-doctor/nextjs-no-client-side-redirect
            if (ruta && ruta !== "/login") router.replace(ruta);
        }

        redirigirSiAutenticado();

        function handlePageShow(e: PageTransitionEvent) {
            if (e.persisted) redirigirSiAutenticado();
        }

        window.addEventListener("pageshow", handlePageShow);
        return () => window.removeEventListener("pageshow", handlePageShow);
    }, [router]);
}
