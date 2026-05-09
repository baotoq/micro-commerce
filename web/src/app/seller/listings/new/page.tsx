import { SellerTopbar } from "@/components/seller/seller-topbar";
import { Button } from "@/components/ui/button";

export default function NewListingPage() {
  return (
    <div className="flex flex-col min-h-screen">
      <SellerTopbar
        title="New listing"
        subtitle="Listings · Drafts · 1 of 1"
        actions={
          <>
            <Button variant="outline">Save draft</Button>
            <Button>&#10003; Publish</Button>
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
            <div className="bg-white rounded-xl border border-black/[0.06] p-5">
              <h4 className="text-[13.5px] font-semibold text-[#1d1d1f] mb-3">
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
                  className="rounded-md border-[1.5px] border-dashed border-black/20 bg-[#f5f5f7] flex flex-col items-center justify-center gap-1.5"
                  style={{ height: 120 }}
                >
                  <div className="w-[60%] h-1.5 bg-black/[0.06] rounded-full overflow-hidden">
                    <div className="h-full bg-[#1d1d1f] rounded-full w-[72%]" />
                  </div>
                  <span className="text-[10px] text-[#1d1d1f]/50">
                    vase-04.heic · 1.4mb
                  </span>
                </div>
                {/* Upload cell */}
                <div
                  className="rounded-md border-[1.5px] border-dashed border-black/20 bg-[#f5f5f7] flex flex-col items-center justify-center gap-1.5 text-[#1d1d1f]/40"
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
                  <span className="text-[11px] text-[#1d1d1f]/50">
                    Drag to add
                  </span>
                </div>
                {/* Plus cell */}
                <div
                  className="rounded-md bg-[#f5f5f7] flex items-center justify-center text-[#1d1d1f]/30"
                  style={{ height: 120 }}
                >
                  <span className="text-[11px]">+</span>
                </div>
              </div>
            </div>

            {/* Title & description card */}
            <div className="bg-white rounded-xl border border-black/[0.06] p-5">
              <h4 className="text-[13.5px] font-semibold text-[#1d1d1f] mb-3">
                Title &amp; description
              </h4>
              {/* Title input */}
              <div
                className="flex items-center rounded-md px-3.5 mb-2.5"
                style={{
                  height: 44,
                  border: "1.5px solid #1d1d1f",
                }}
              >
                <span className="text-[15px] font-semibold text-[#1d1d1f]">
                  Persimmon vase
                </span>
              </div>
              {/* Description textarea */}
              <div
                className="rounded-md border border-black/[0.1] p-3.5"
                style={{ minHeight: 110 }}
              >
                <p className="text-sm leading-relaxed text-[#1d1d1f]">
                  Hand-thrown stoneware with a soft persimmon glaze. Each piece
                  is a little different — the rim has a slight asymmetry I
                  happen to like. Best for short-stem flowers or
                </p>
                <span className="text-[11px] text-[#1d1d1f]/40">|</span>
              </div>
              <div className="flex items-center justify-between mt-2">
                <span className="text-[11px] text-[#1d1d1f]/50">
                  Markdown OK
                </span>
                <span className="text-[11px] text-[#1d1d1f]/50 tabular-nums">
                  142 / 800
                </span>
              </div>
            </div>
          </div>

          {/* Right column */}
          <div className="flex flex-col gap-5">
            {/* Price & stock card */}
            <div className="bg-white rounded-xl border border-black/[0.06] p-5">
              <h4 className="text-[13.5px] font-semibold text-[#1d1d1f] mb-3">
                Price &amp; stock
              </h4>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <p className="text-[11px] text-[#1d1d1f]/50 mb-1">Price</p>
                  <div
                    className="flex items-center gap-1.5 rounded-md border border-black/[0.1] px-3"
                    style={{ height: 44 }}
                  >
                    <span className="text-[#1d1d1f]/50 tabular-nums text-sm">
                      $
                    </span>
                    <span className="text-[16px] font-semibold tabular-nums text-[#1d1d1f]">
                      86.00
                    </span>
                  </div>
                </div>
                <div>
                  <p className="text-[11px] text-[#1d1d1f]/50 mb-1">Stock</p>
                  <div
                    className="flex items-center rounded-md border border-black/[0.1] px-3"
                    style={{ height: 44 }}
                  >
                    <span className="text-[16px] font-semibold tabular-nums text-[#1d1d1f]">
                      12
                    </span>
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-2 mt-3.5 px-2.5 py-2 bg-[#f5f5f7] rounded-md">
                <span className="w-2 h-2 rounded-full bg-emerald-500 shrink-0" />
                <span className="text-sm text-[#1d1d1f]">
                  Suggested: $78–$94 based on 6 similar shops
                </span>
              </div>
            </div>

            {/* Category & tags card */}
            <div className="bg-white rounded-xl border border-black/[0.06] p-5">
              <h4 className="text-[13.5px] font-semibold text-[#1d1d1f] mb-3">
                Category &amp; tags
              </h4>
              <p className="text-[11px] text-[#1d1d1f]/50">Category</p>
              <div
                className="flex items-center justify-between rounded-md border border-black/[0.1] px-3 mt-1"
                style={{ height: 38 }}
              >
                <span className="text-sm text-[#1d1d1f]">
                  Ceramics · <span className="text-[#1d1d1f]/50">Vessels</span>
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
              <p className="text-[11px] text-[#1d1d1f]/50 mt-3">Tags</p>
              <div className="flex flex-wrap gap-1.5 mt-1.5">
                {["hand-thrown", "stoneware", "persimmon", "small batch"].map(
                  (tag) => (
                    <span
                      key={tag}
                      className="text-[11px] px-2 py-1 rounded-full bg-[#f0f0f0] text-[#1d1d1f]"
                    >
                      {tag} <span className="opacity-50 ml-1">&times;</span>
                    </span>
                  ),
                )}
                <span className="text-[11px] px-2 py-1 rounded-full text-[#1d1d1f]/40">
                  + add
                </span>
              </div>
            </div>

            {/* Listing health card */}
            <div className="bg-[#f5f5f7] rounded-xl p-5">
              <div className="flex items-center justify-between mb-2">
                <span className="text-[13.5px] font-semibold text-[#1d1d1f]">
                  Listing health
                </span>
                <span className="text-[13.5px] font-semibold tabular-nums text-emerald-600">
                  92
                </span>
              </div>
              <div className="h-1.5 bg-black/[0.06] rounded-full mb-2.5 overflow-hidden">
                <div className="h-full bg-emerald-500 rounded-full w-[92%]" />
              </div>
              <p className="text-[11px] text-[#1d1d1f]/50">
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
