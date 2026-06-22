"use client";

import { AnimatePresence, m } from "framer-motion";
import { usePathname } from "next/navigation";
import { pageEnter } from "@ui/motion/MotionConfig";

interface Props {
    children: React.ReactNode;
}

export function PageTransition({ children }: Props) {
    const pathname = usePathname();

    return (
        <AnimatePresence mode="wait" initial={false}>
            <m.div
                key={pathname}
                variants={pageEnter}
                initial="initial"
                animate="enter"
                exit="exit"
                className="min-h-full"
            >
                {children}
            </m.div>
        </AnimatePresence>
    );
}
