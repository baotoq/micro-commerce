// Browser-facing proxy for the Catalog API's SAS-issuance endpoint. The
// PhotoUploader runs client-side so its fetch is same-origin (Next.js);
// this route forwards the body to the Catalog API which holds the storage
// credentials.
//
// This is a WRITE proxy (the upstream POST /api/products/photo-upload-url sits
// under the Catalog API's seller policy), so it gates on requireSeller() and
// forwards the Keycloak access token. The by-sku/exists and listings proxies
// are reads and stay anonymous.

import { NextResponse } from "next/server";
import { getAccessToken, requireSeller } from "@/lib/auth/token";

function apiBase(): string {
  const url = process.env.API_URL;
  if (!url) {
    throw new Error(
      "API_URL env var is not set. Run the app via the Aspire AppHost.",
    );
  }
  return url.replace(/\/$/, "");
}

export async function POST(req: Request) {
  await requireSeller();
  const token = await getAccessToken();
  const body = await req.text();
  const upstream = await fetch(`${apiBase()}/api/products/photo-upload-url`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body,
    cache: "no-store",
  });
  const text = await upstream.text();
  return new NextResponse(text, {
    status: upstream.status,
    headers: {
      "Content-Type":
        upstream.headers.get("Content-Type") ?? "application/json",
    },
  });
}
