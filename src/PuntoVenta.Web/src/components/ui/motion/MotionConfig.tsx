import type { Variants } from "framer-motion";

export const easeSoft = [0.2, 0.8, 0.2, 1] as const;

const durations = {
    fast: 0.14,
    med: 0.22,
    slow: 0.36,
} as const;

export const pageEnter: Variants = {
    initial: { opacity: 0, y: 4 },
    enter: {
        opacity: 1,
        y: 0,
        transition: { duration: durations.med, ease: easeSoft },
    },
    exit: {
        opacity: 0,
        transition: { duration: durations.fast, ease: easeSoft },
    },
};

export const sidebarReveal: Variants = {
    initial: { opacity: 0, x: -6 },
    enter: {
        opacity: 1,
        x: 0,
        transition: { duration: durations.med, ease: easeSoft },
    },
    exit: {
        opacity: 0,
        x: -6,
        transition: { duration: durations.fast, ease: easeSoft },
    },
};

export const expandCollapse: Variants = {
    initial: { height: 0, opacity: 0 },
    enter: {
        height: "auto",
        opacity: 1,
        transition: { duration: durations.med, ease: easeSoft },
    },
    exit: {
        height: 0,
        opacity: 0,
        transition: { duration: durations.fast, ease: easeSoft },
    },
};
