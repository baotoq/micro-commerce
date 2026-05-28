// Browser-facing proxy for the Catalog API's SKU-uniqueness probe. The
// create wizard fires this from a debounce in step-basics.tsx to drive the
// inline "That SKU is taken" hint while the user is still typing. Returns
// 204 if the SKU exists, 404 if available — matching the upstream contract.

import { NextResponse } from "next/server";

function apiBase(): string {
  const url = process.env.API_URL;
  if (!url) {
    throw new Error(
      "API_URL env var is not set. Run the app via the Aspire AppHost.",
    );
  }
  return url.replace(/\/$/, "");
}

export async function GET(
  _req: Request,
  { params }: { params: Promise<{ sku: string }> },
) {
  const { sku } = await params;
  const upstream = await fetch(
    `${apiBase()}/api/products/by-sku/${encodeURIComponent(sku)}/exists`,
    { method: "GET", cache: "no-store" },
  );
  return new NextResponse(null, { status: upstream.status });
}
