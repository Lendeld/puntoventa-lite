import { Metadata } from "next";
import { AcercaPageSection } from "@pages/acerca/AcercaPageSection";
import { obtenerAcercaService } from "@lib/services/acerca.service";

export const metadata: Metadata = {
    title: "Acerca de",
};

// La info de versiones depende del binario desplegado en el momento. Sin esto,
// Next cachea el server fetch y la página muestra la versión anterior despues
// de un deploy nuevo.
export const dynamic = "force-dynamic";

export default async function AcercaPage() {
    const { data } = await obtenerAcercaService();
    return <AcercaPageSection data={data ?? null} />;
}
