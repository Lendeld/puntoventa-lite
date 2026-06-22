"use client";

import { useEffect } from "react";
import { usePathname } from "next/navigation";
import { completeNavigationProgress } from "@mantine/nprogress";

export function RouterProgressSync() {
    const pathname = usePathname();

    useEffect(() => {
        completeNavigationProgress();
    }, [pathname]);

    return null;
}
