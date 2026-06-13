import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
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
  // Product photoUrls flow through next/image (storefront ProductImage). Allow-list
  // ONLY the known hosts — the seeded placeholder host plus the Azure Blob hosts the
  // seller photo-uploader writes (Azurite locally, *.blob.core.windows.net in prod).
  // Do NOT wildcard the host: an open remotePatterns turns the image optimizer into an
  // SSRF/abuse proxy for any seller-supplied URL.
  images: {
    remotePatterns: [
      { protocol: "https", hostname: "placehold.co" },
      { protocol: "https", hostname: "**.blob.core.windows.net" },
      { protocol: "http", hostname: "127.0.0.1", port: "10000" },
      { protocol: "http", hostname: "azurite.micro-commerce.k8s.orb.local" },
    ],
  },
};

export default nextConfig;
