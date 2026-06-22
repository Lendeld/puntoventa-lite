import { ReactNode } from "react";

interface Props {
    children: ReactNode;
}

export function TableFooter({ children }: Props) {
    return (
        <div className="p-4 border-t border-theme bg-theme-surface">
            {children}
        </div>
    );
}
