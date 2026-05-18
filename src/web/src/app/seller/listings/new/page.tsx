import { Check } from "lucide-react";
import Link from "next/link";
import { SellerTopbar } from "@/components/seller/shell/seller-topbar";
import { Button, buttonVariants } from "@/components/ui/button";

export default function NewListingPage() {
  return (
    <div className="flex flex-col min-h-screen">
      <SellerTopbar
        title="New listing"
        subtitle="Listings · Drafts · 1 of 1"
        actions={
          <>
            <Button variant="outline">Save draft</Button>
            <Link href="/seller/listings" className={buttonVariants()}>
              <Check size={14} /> Publish
            </Link>
          </>
        }
      />

      <div className="flex-1 overflow-auto p-7">
        <div
          className="grid gap-5"
          style={{ gridTemplateColumns: "1.4fr 1fr" }}
        >
          {/* Left column */}
          <div className="flex flex-col gap-5">
            {/* Photos card */}
            <div className="bg-white rounded-lg border border-border p-5">
              <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
                Photos · 3 of 6
              </h4>
              <div className="grid grid-cols-3 gap-2">
                {/* Filled photo swatches */}
                <div
                  className="rounded-md bg-[#e2d5c8]"
                  style={{ height: 120 }}
                />
                <div
                  className="rounded-md bg-[#e3d4ce]"
                  style={{ height: 120 }}
                />
                <div
                  className="rounded-md bg-[#efe8d9]"
                  style={{ height: 120 }}
                />
                {/* Uploading placeholder */}
                <div
                  className="rounded-md border-[1.5px] border-dashed border-black/20 bg-canvas-parchment flex flex-col items-center justify-center gap-1.5"
                  style={{ height: 120 }}
                >
                  <div className="w-[60%] h-1.5 bg-black/[0.06] rounded-full overflow-hidden">
                    <div className="h-full bg-foreground rounded-full w-[72%]" />
                  </div>
                  <span className="text-[10px] text-muted-foreground">
                    vase-04.heic · 1.4mb
                  </span>
                </div>
                {/* Upload cell */}
                <div
                  className="rounded-md border-[1.5px] border-dashed border-black/20 bg-canvas-parchment flex flex-col items-center justify-center gap-1.5 text-muted-foreground"
                  style={{ height: 120 }}
                >
                  <svg
                    aria-hidden="true"
                    width="18"
                    height="18"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="1.8"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  >
                    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                    <polyline points="17 8 12 3 7 8" />
                    <line x1="12" y1="3" x2="12" y2="15" />
                  </svg>
                  <span className="text-[11px] text-muted-foreground">
                    Drag to add
                  </span>
                </div>
                {/* Plus cell */}
                <div
                  className="rounded-md bg-canvas-parchment flex items-center justify-center text-muted-foreground"
                  style={{ height: 120 }}
                >
                  <span className="text-[11px]">+</span>
                </div>
              </div>
            </div>

            {/* Title & description card */}
            <div className="bg-white rounded-lg border border-border p-5">
              <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
                Title &amp; description
              </h4>
              {/* Title input */}
              <div
                className="flex items-center rounded-md px-3.5 mb-2.5"
                style={{
                  height: 44,
                  border: "1.5px solid var(--foreground)",
                }}
              >
                <span className="text-[15px] font-semibold text-foreground">
                  Persimmon vase
                </span>
              </div>
              {/* Description textarea */}
              <div
                className="rounded-md border border-black/[0.1] p-3.5"
                style={{ minHeight: 110 }}
              >
                <p className="text-sm leading-relaxed text-foreground">
                  Hand-thrown stoneware with a soft persimmon glaze. Each piece
                  is a little different — the rim has a slight asymmetry I
                  happen to like. Best for short-stem flowers or
                </p>
                <span className="text-[11px] text-muted-foreground">|</span>
              </div>
              <div className="flex items-center justify-between mt-2">
                <span className="text-[11px] text-muted-foreground">
                  Markdown OK
                </span>
                <span className="text-[11px] text-muted-foreground tabular-nums">
                  142 / 800
                </span>
              </div>
            </div>
          </div>

          {/* Right column */}
          <div className="flex flex-col gap-5">
            {/* Status toggle card */}
            <div className="bg-white rounded-lg border border-border p-5">
              <h4 className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider mb-2">
                Status
              </h4>
              <div className="flex gap-1 bg-canvas-parchment p-[3px] rounded-lg">
                {["Active", "Draft", "Archived"].map((s, i) => (
                  <button
                    key={s}
                    type="button"
                    className={
                      i === 1
                        ? "flex-1 text-[12px] font-medium py-1.5 rounded-md bg-white text-foreground shadow-sm"
                        : "flex-1 text-[12px] font-medium py-1.5 rounded-md bg-transparent text-muted-foreground"
                    }
                  >
                    {s}
                  </button>
                ))}
              </div>
            </div>

            {/* Price & stock card */}
            <div className="bg-white rounded-lg border border-border p-5">
              <h4 className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider mb-3">
                Pricing
              </h4>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <p className="text-[11px] text-muted-foreground mb-1">
                    Price
                  </p>
                  <div
                    className="flex items-center gap-1.5 rounded-md border border-border px-3"
                    style={{ height: 44 }}
                  >
                    <span className="text-muted-foreground tabular-nums text-sm">
                      $
                    </span>
                    <span className="text-[16px] font-semibold tabular-nums text-foreground">
                      86.00
                    </span>
                  </div>
                </div>
                <div>
                  <p className="text-[11px] text-muted-foreground mb-1">
                    Compare-at
                  </p>
                  <div
                    className="flex items-center gap-1.5 rounded-md border border-border px-3"
                    style={{ height: 44 }}
                  >
                    <span className="text-muted-foreground tabular-nums text-sm">
                      $
                    </span>
                    <span className="text-[16px] tabular-nums text-muted-foreground">
                      —
                    </span>
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-2 mt-3.5 px-2.5 py-2 bg-canvas-parchment rounded-md">
                <span className="w-2 h-2 rounded-full bg-good shrink-0" />
                <span className="text-sm text-foreground">
                  Suggested: $78–$94 based on 6 similar shops
                </span>
              </div>
            </div>

            {/* Inventory card */}
            <div className="bg-white rounded-lg border border-border p-5">
              <h4 className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider mb-3">
                Inventory
              </h4>
              <div className="flex items-center justify-between px-3 py-2 bg-canvas-parchment rounded-lg mb-3">
                <span className="text-sm text-foreground">Total in stock</span>
                <span className="text-[16px] font-semibold tabular-nums text-foreground">
                  12
                </span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-8 h-4 rounded-full bg-good flex items-center justify-end pr-0.5">
                  <div className="w-3 h-3 rounded-full bg-white shadow-sm" />
                </div>
                <span className="text-sm text-foreground">
                  Allow pre-orders
                </span>
              </div>
            </div>

            {/* Shipping card */}
            <div className="bg-white rounded-lg border border-border p-5">
              <h4 className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider mb-3">
                Shipping
              </h4>
              <div className="flex flex-col gap-1.5 text-sm text-muted-foreground">
                <div className="flex justify-between">
                  <span>Weight</span>
                  <span className="text-foreground tabular-nums">1.4 kg</span>
                </div>
                <div className="flex justify-between">
                  <span>Origin</span>
                  <span className="text-foreground">Oakland, CA</span>
                </div>
                <div className="flex justify-between">
                  <span>Class</span>
                  <span className="text-foreground">Fragile · standard</span>
                </div>
              </div>
            </div>

            {/* Category & tags card */}
            <div className="bg-white rounded-lg border border-border p-5">
              <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
                Category &amp; tags
              </h4>
              <p className="text-[11px] text-muted-foreground">Category</p>
              <div
                className="flex items-center justify-between rounded-md border border-border px-3 mt-1"
                style={{ height: 38 }}
              >
                <span className="text-sm text-foreground">
                  Ceramics ·{" "}
                  <span className="text-muted-foreground">Vessels</span>
                </span>
                <svg
                  aria-hidden="true"
                  width="11"
                  height="11"
                  viewBox="0 0 12 12"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <polyline points="2,4 6,8 10,4" />
                </svg>
              </div>
              <p className="text-[11px] text-muted-foreground mt-3">Tags</p>
              <div className="flex flex-wrap gap-1.5 mt-1.5">
                {["hand-thrown", "stoneware", "persimmon", "small batch"].map(
                  (tag) => (
                    <span
                      key={tag}
                      className="text-[11px] px-2 py-1 rounded-full bg-muted text-foreground"
                    >
                      {tag} <span className="opacity-50 ml-1">&times;</span>
                    </span>
                  ),
                )}
                <span className="text-[11px] px-2 py-1 rounded-full text-muted-foreground">
                  + add
                </span>
              </div>
            </div>

            {/* Listing health card */}
            <div className="bg-canvas-parchment rounded-lg p-5">
              <div className="flex items-center justify-between mb-2">
                <span className="text-[13.5px] font-semibold text-foreground">
                  Listing health
                </span>
                <span className="text-[13.5px] font-semibold tabular-nums text-good">
                  92
                </span>
              </div>
              <div className="h-1.5 bg-black/[0.06] rounded-full mb-2.5 overflow-hidden">
                <div className="h-full bg-good rounded-full w-[92%]" />
              </div>
              <p className="text-[11px] text-muted-foreground">
                3 photos · clear title · price set · description over 100 chars.
                Add 1 more photo to reach 100.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
