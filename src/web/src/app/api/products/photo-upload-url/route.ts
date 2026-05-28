// Browser-facing proxy for the Catalog API's SAS-issuance endpoint. The
// PhotoUploader runs client-side so its fetch is same-origin (Next.js);
// this route forwards the body to the Catalog API which holds the storage
// credentials.

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

export async function POST(req: Request) {
  const body = await req.text();
  const upstream = await fetch(`${apiBase()}/api/products/photo-upload-url`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
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
