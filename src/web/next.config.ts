import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  // Cache Components (Next 16) — replaces the v15-era `force-dynamic` +
  // `cache: "no-store"` opt-outs with explicit `"use cache"` + `cacheTag`
  // on the loaders. See `src/lib/seller/listings/data.ts`.
  cacheComponents: true,
  // Aspire serves the web app via a per-run host like web-microcommerce.dev.localhost:<port>.
  // Without these, Next 16 blocks HMR/dev resources as cross-origin and the dev runtime
  // never finishes initializing — client components stay unhydrated (e.g., dialog buttons no-op).
  allowedDevOrigins: ["127.0.0.1", "*.dev.localhost"],
};

export default nextConfig;
