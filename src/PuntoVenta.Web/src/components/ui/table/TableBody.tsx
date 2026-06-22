import { ReactNode } from "react";

interface Props {
    children: ReactNode;
}

export function TableBody({ children }: Props) {
    return (
        <div className="bg-theme-surface overflow-auto flex-1">{children}</div>
    );
}
