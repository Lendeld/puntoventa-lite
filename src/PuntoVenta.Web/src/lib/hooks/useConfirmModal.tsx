import { modals, ModalsProviderProps } from "@mantine/modals";
import { ReactNode } from "react";

export interface ConfirmModalProps {
    title: ReactNode;
    message: ReactNode;
    labels: ModalsProviderProps["labels"];
    onConfirm: () => void;
    onCancel?: () => void;
    variant?: "danger" | "default";
    overlay?: boolean;
    closeOnClickOutside?: boolean;
}

const useConfirmModal = ({
    title,
    message,
    labels,
    onConfirm,
    onCancel,
    variant = "default",
    overlay = false,
    closeOnClickOutside,
}: ConfirmModalProps) => {
    return () =>
        modals.openConfirmModal({
            title: title,
            centered: true,
            closeOnClickOutside: closeOnClickOutside,
            overlayProps: overlay ? { blur: 3, opacity: 1 } : undefined,
            children: message,
            labels: labels,
            confirmProps: {
                color: variant === "danger" ? "red" : "accentPV",
            },
            cancelProps: {
                variant: "outline",
            },
            onConfirm: () => onConfirm(),
            onCancel: () => {
                if (onCancel) onCancel();
            },
        });
};

export default useConfirmModal;
