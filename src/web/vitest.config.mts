import { resolve } from "node:path";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": resolve(__dirname, "./src"),
      "server-only": resolve(__dirname, "./src/test/server-only-stub.ts"),
      // next-auth's env side-effect does a bare `import "next/server"`; Next
      // ships it as `next/server.js` with no subpath exports map, which the
      // Vitest resolver can't follow on its own. Point it at the file so the
      // real auth config (and its callbacks) can be imported under jsdom.
      "next/server": resolve(__dirname, "./node_modules/next/server.js"),
    },
  },
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: ["./vitest.setup.ts"],
    include: ["src/**/*.{test,spec}.{ts,tsx}"],
    // Inline next-auth (ESM) so Vite transforms it and the `next/server` alias
    // above applies. Otherwise it is externalized and Node's native ESM
    // resolver can't follow Next's extension-less `import "next/server"`.
    server: { deps: { inline: ["next-auth", "@auth/core"] } },
  },
});
