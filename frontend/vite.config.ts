/// <reference types="vitest" />
import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import path from "path";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },

  server: {
    proxy: {
      "/api": {
        target: "http://localhost:5073", // <-- your backend URL
        changeOrigin: true,
        secure: false,
        cookieDomainRewrite: {
          "*": "",
        },
      },
    },
  },

  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: "./tests/setupTests.ts",
    include: ["tests/**/*.{test,spec}.{ts,tsx}"],

    coverage: {
      provider: "v8", // uses @vitest/coverage-v8
      reporter: ["text", "html", "lcov"],
      reportsDirectory: "./coverage",
      include: ["src/**/*.{ts,tsx}"],

      thresholds: {
        lines: 80,
        functions: 80,
        branches: 70,
        statements: 80,
      },
      // if you ever want *per-file* thresholds instead, use:
      // thresholds: {
      //   perFile: true,
      //   lines: 80,
      //   functions: 80,
      //   branches: 70,
      //   statements: 80,
      // },
    },
  },
});
