import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { Button, buttonVariants } from "@/components/ui/button";
import { getListingBySku } from "@/lib/seller/listings/data";

export async function generateMetadata({
  params,
}: {
  params: Promise<{ sku: string }>;
}): Promise<Metadata> {
  const { sku } = await params;
  return { title: `Preview ${sku} · Micro Commerce` };
}

type Check = { label: string; sub: string; tone: "good" | "warn" };

const CHECKS: Check[] = [
  { label: "Title under 60 chars", sub: "14 / 60", tone: "good" },
  { label: "Description over 100 chars", sub: "208 / 800", tone: "good" },
  { label: "4 photos", sub: "recommend 6+", tone: "warn" },
  { label: "Variants in stock", sub: "5 of 6 active", tone: "good" },
  { label: "Tagged & categorized", sub: "4 tags · Vessels", tone: "good" },
];

const slugify = (s: string) =>
  s
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/(^-|-$)/g, "");

export default async function ListingPreviewPage({
  params,
}: {
  params: Promise<{ sku: string }>;
}) {
  const { sku } = await params;
  const listing = await getListingBySku(sku);
  if (!listing) notFound();

  const slug = slugify(listing.name);

  return (
    <div className="flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-black/[0.06] bg-white px-7 py-4">
        <div className="flex items-center gap-3">
          <Link
            href={`/seller/listings/${sku}/edit`}
            aria-label="Back"
            className="flex h-8 w-8 items-center justify-center rounded-md hover:bg-canvas-parchment"
          >
            ‹
          </Link>
          <div>
            <p className="text-[11px] uppercase tracking-wider text-foreground/60">
              Preview · {listing.name}
            </p>
            <h1 className="mt-0.5 text-[22px] font-semibold leading-none tracking-tight text-foreground">
              How shoppers will see it
            </h1>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <div className="flex gap-1 rounded-lg bg-canvas-parchment p-[3px]">
            <Button
              variant="ghost"
              size="sm"
              className="bg-white text-foreground shadow-sm hover:bg-white"
            >
              Desktop
            </Button>
            <Button
              variant="ghost"
              size="sm"
              className="bg-transparent text-foreground hover:bg-transparent"
            >
              Mobile
            </Button>
          </div>
          <Link
            href={`/seller/listings/${sku}/edit`}
            className={buttonVariants({ variant: "outline" })}
          >
            Back to edit
          </Link>
          <Link href="/seller/listings/published" className={buttonVariants()}>
            Publish now →
          </Link>
        </div>
      </div>

      {/* Body */}
      <div className="flex flex-1 overflow-hidden bg-canvas-parchment">
        {/* Browser preview */}
        <div className="flex flex-1 items-center justify-center p-6">
          <div
            className="overflow-hidden rounded-xl border border-black/[0.06] bg-white shadow-2xl"
            style={{ width: "100%", maxWidth: 720, height: 560 }}
          >
            <div
              className="flex items-center gap-2 border-b border-black/[0.06] px-3"
              style={{ height: 30, background: "#E8E3D8" }}
            >
              {["#FF6058", "#FFBD2E", "#28C941"].map((c) => (
                <span
                  key={c}
                  aria-hidden="true"
                  className="h-[9px] w-[9px] rounded-full"
                  style={{ background: c }}
                />
              ))}
              <div
                className="ml-2 flex flex-1 items-center gap-1 rounded bg-white px-2"
                style={{ height: 18 }}
              >
                <span aria-hidden="true" className="text-[10px]">
                  🔒
                </span>
                <span className="text-[11px] text-foreground/60">
                  alex-studio.micro.shop/{slug}
                </span>
              </div>
            </div>
            <div
              className="grid"
              style={{
                gridTemplateColumns: "1.4fr 1fr",
                height: "calc(100% - 30px)",
              }}
            >
              <div style={{ background: "#e2d5c8" }} />
              <div className="flex flex-col p-7">
                <p className="text-[11px] uppercase tracking-wider text-foreground/60">
                  Alex Studio · {listing.category}
                </p>
                <h2 className="mt-1.5 text-[32px] font-semibold leading-none tracking-tight text-foreground">
                  {listing.name}
                </h2>
                <p className="mt-2.5 text-[22px] font-semibold tabular-nums text-foreground">
                  $95
                </p>
                <p className="mt-1 text-[11px] text-foreground/60">
                  Medium · Persimmon · 4 in stock
                </p>

                <p className="mt-3.5 mb-1.5 text-[11px] font-medium text-foreground">
                  SIZE
                </p>
                <div className="flex gap-1">
                  <span className="rounded-full bg-canvas-parchment px-3 py-1 text-[12px] font-medium text-foreground">
                    Small
                  </span>
                  <span className="rounded-full bg-foreground px-3 py-1 text-[12px] font-medium text-white">
                    Medium
                  </span>
                  <span className="rounded-full bg-canvas-parchment px-3 py-1 text-[12px] font-medium text-foreground/50">
                    Large · out
                  </span>
                </div>

                <p className="mt-3.5 mb-1.5 text-[11px] font-medium text-foreground">
                  GLAZE
                </p>
                <div className="flex gap-2">
                  <span
                    className="h-6 w-6 rounded-full ring-2 ring-foreground"
                    style={{ background: "#c2410c" }}
                  />
                  <span
                    className="h-6 w-6 rounded-full border border-black/[0.1]"
                    style={{ background: "#F0E8D7" }}
                  />
                </div>

                <div className="flex-1" />
                <Button className="mt-3 h-11 w-full rounded-pill">
                  Add to bag · $95
                </Button>
              </div>
            </div>
          </div>
        </div>

        {/* Health rail */}
        <aside className="w-[320px] shrink-0 overflow-auto border-l border-black/[0.06] bg-white p-[22px]">
          <div className="mb-2 flex items-center justify-between">
            <h2 className="text-[13.5px] font-semibold text-foreground">
              Listing health
            </h2>
            <span className="text-[13.5px] font-semibold tabular-nums text-good">
              96
            </span>
          </div>
          <div className="mb-4 h-1.5 overflow-hidden rounded-full bg-black/[0.06]">
            <div className="h-full w-[96%] rounded-full bg-good" />
          </div>
          <div className="flex flex-col gap-3">
            {CHECKS.map((c) => (
              <div key={c.label} className="flex items-start gap-2">
                <span
                  aria-hidden="true"
                  className={`mt-0.5 flex h-[18px] w-[18px] shrink-0 items-center justify-center rounded-full text-white ${c.tone === "good" ? "bg-good" : "bg-warn"}`}
                >
                  <span className="text-[10px] font-bold">
                    {c.tone === "good" ? "✓" : "i"}
                  </span>
                </span>
                <div className="flex-1">
                  <div className="text-[12.5px] font-medium text-foreground">
                    {c.label}
                  </div>
                  <div className="text-[11px] text-foreground/60">{c.sub}</div>
                </div>
              </div>
            ))}
          </div>
        </aside>
      </div>
    </div>
  );
}
