import { Alert, Center } from "@mantine/core";
import { IconAlertCircle } from "@tabler/icons-react";

interface Props {
    message?: string;
    height?: string;
}

export default function ErrorDataAlert({ message, height }: Props) {
    return (
        <Center className={`${height} w-full rounded-md`}>
            <Alert
                icon={<IconAlertCircle size={16} />}
                color="red"
                variant="light"
            >
                {message || "Ocurrió un error al cargar los datos."}
            </Alert>
        </Center>
    );
}
