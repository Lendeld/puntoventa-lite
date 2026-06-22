export const metadata = {
    title: "Detalle Usuario",
};

interface Props {
    params: Promise<{
        id: string;
    }>;
}

export default async function DetalleUsuarioPage({ params }: Props) {
    const { id } = await params;
    return <div>DetalleUsuarioPage - ID: {id}</div>;
}
