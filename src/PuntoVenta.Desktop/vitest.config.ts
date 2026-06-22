import { defineConfig } from "vitest/config";

export default defineConfig({
    test: {
        // Entorno Node puro — sin Electron, sin DOM
        environment: "node",
        include: ["tests/**/*.test.ts"],
    },
});
