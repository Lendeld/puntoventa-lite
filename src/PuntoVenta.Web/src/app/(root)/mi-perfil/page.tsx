import type { Metadata } from "next";
import MiPerfilPageSection from "./MiPerfilPageSection";

export const metadata: Metadata = {
    title: "Mi perfil",
};

export default function MiPerfilPage() {
    return <MiPerfilPageSection />;
}
