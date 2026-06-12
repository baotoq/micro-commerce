// Test-only escape hatch. Playwright fixtures mutate the catalog directly
// via the Aspire-injected `services__catalog-api__http__0` URL, bypassing
// the Next.js Server Actions that normally call `revalidateTag`. Without a
// way to bust the `"use cache" + cacheTag("listings")` loaders, count-tied
// specs (pagination, listings index, delete-cache) see stale values.
//
// Only enabled in non-production environments. Returns 404 in prod so a
// build-time scan doesn't reveal a debug surface.

import { revalidateTag } from "next/cache";
import { NextResponse } from "next/server";

const ALLOWED_TAGS = new Set(["listings", "promotions"]);

export async function POST(req: Request) {
  if (process.env.NODE_ENV === "production") {
    return new NextResponse(null, { status: 404 });
  }
  let tag: string | undefined;
  try {
    const body = (await req.json()) as { tag?: string };
    tag = body.tag;
  } catch {
    return NextResponse.json({ error: "invalid json body" }, { status: 400 });
  }
  if (!tag || !ALLOWED_TAGS.has(tag)) {
    return NextResponse.json(
      { error: `tag must be one of: ${[...ALLOWED_TAGS].join(", ")}` },
      { status: 400 },
    );
  }
  // `{ expire: 0 }` expires the tag entry immediately so the NEXT request is a
  // blocking cache miss that fetches fresh data. The empty-object form (`{}`)
  // behaves like `profile="max"` (stale-while-revalidate), which would let the
  // e2e see stale rows right after a direct catalog write (e.g. a just-created
  // promo). `updateTag` (read-your-own-writes) is not usable here — it only
  // works in Server Actions, and this is a Route Handler.
  revalidateTag(tag, { expire: 0 });
  return new NextResponse(null, { status: 204 });
}
