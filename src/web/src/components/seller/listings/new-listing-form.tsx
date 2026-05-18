"use client";

import { useRouter } from "next/navigation";
import { useActionState, useEffect } from "react";
import type { ActionResult } from "@/lib/seller/listings/actions";
import { createListingAction } from "@/lib/seller/listings/actions";

function FieldError({ errors }: { errors?: string[] }) {
  if (!errors?.length) return null;
  return (
    <p className="mt-1 text-[11px] text-rose-600" role="alert">
      {errors[0]}
    </p>
  );
}

export function NewListingForm() {
  const router = useRouter();

  const [state, formAction, _pending] = useActionState(
    async (_prev: ActionResult | null, fd: FormData) => createListingAction(fd),
    null,
  );

  useEffect(() => {
    if (state?.ok) {
      router.push("/seller/listings");
    }
  }, [state, router]);

  const fieldErrors = state && !state.ok ? state.fieldErrors : undefined;

  return (
    <form id="new-listing-form" action={formAction} className="contents">
      {/* Global error */}
      {state && !state.ok && !state.fieldErrors && (
        <p className="col-span-2 text-sm text-rose-600 px-1 -mb-2" role="alert">
          {state.error}
        </p>
      )}

      {/* Left column cards — wired */}
      <div className="flex flex-col gap-5">
        {/* Title & description card */}
        <div className="bg-white rounded-lg border border-border p-5">
          <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
            Title &amp; description
          </h4>

          {/* SKU */}
          <div className="mb-2.5">
            <label
              htmlFor="sku"
              className="text-[11px] text-muted-foreground block mb-1"
            >
              SKU
            </label>
            <input
              id="sku"
              name="sku"
              type="text"
              placeholder="e.g. MC-VS-001"
              className="w-full flex items-center rounded-md px-3.5 text-[15px] font-semibold text-foreground placeholder:font-normal placeholder:text-muted-foreground focus:outline-none"
              style={{
                height: 44,
                border: "1.5px solid var(--border)",
              }}
            />
            <FieldError errors={fieldErrors?.sku} />
          </div>

          {/* Name */}
          <div className="mb-2.5">
            <label
              htmlFor="name"
              className="text-[11px] text-muted-foreground block mb-1"
            >
              Name
            </label>
            <input
              id="name"
              name="name"
              type="text"
              placeholder="Persimmon vase"
              className="w-full flex items-center rounded-md px-3.5 text-[15px] font-semibold text-foreground placeholder:font-normal placeholder:text-muted-foreground focus:outline-none"
              style={{
                height: 44,
                border: "1.5px solid var(--foreground)",
              }}
            />
            <FieldError errors={fieldErrors?.name} />
          </div>

          {/* Description — static mockup */}
          <div
            className="rounded-md border border-black/[0.1] p-3.5"
            style={{ minHeight: 110 }}
          >
            <p className="text-sm leading-relaxed text-foreground">
              Hand-thrown stoneware with a soft persimmon glaze. Each piece is a
              little different — the rim has a slight asymmetry I happen to
              like. Best for short-stem flowers or
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

        {/* Photos card — static mockup */}
        <div className="bg-white rounded-lg border border-border p-5">
          <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
            Photos · 3 of 6
          </h4>
          <div className="grid grid-cols-3 gap-2">
            <div className="rounded-md bg-[#e2d5c8]" style={{ height: 120 }} />
            <div className="rounded-md bg-[#e3d4ce]" style={{ height: 120 }} />
            <div className="rounded-md bg-[#efe8d9]" style={{ height: 120 }} />
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
            <div
              className="rounded-md bg-canvas-parchment flex items-center justify-center text-muted-foreground"
              style={{ height: 120 }}
            >
              <span className="text-[11px]">+</span>
            </div>
          </div>
        </div>
      </div>

      {/* Right column cards — wired */}
      <div className="flex flex-col gap-5">
        {/* Status card */}
        <div className="bg-white rounded-lg border border-border p-5">
          <label
            htmlFor="status"
            className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider block mb-2"
          >
            Status
          </label>
          <select
            id="status"
            name="status"
            defaultValue="draft"
            className="w-full rounded-md border border-border px-3 text-sm text-foreground bg-white"
            style={{ height: 38 }}
          >
            <option value="active">Active</option>
            <option value="draft">Draft</option>
            <option value="low">Low</option>
            <option value="out">Out</option>
          </select>
          <FieldError errors={fieldErrors?.status} />
        </div>

        {/* Pricing card */}
        <div className="bg-white rounded-lg border border-border p-5">
          <h4 className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider mb-3">
            Pricing
          </h4>
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label
                htmlFor="price"
                className="text-[11px] text-muted-foreground mb-1 block"
              >
                Price
              </label>
              <div
                className="flex items-center gap-1.5 rounded-md border border-border px-3"
                style={{ height: 44 }}
              >
                <span className="text-muted-foreground tabular-nums text-sm">
                  $
                </span>
                <input
                  id="price"
                  name="price"
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="0.00"
                  className="flex-1 text-[16px] font-semibold tabular-nums text-foreground bg-transparent outline-none"
                />
              </div>
              <FieldError errors={fieldErrors?.price} />
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
          <div className="mb-3">
            <label
              htmlFor="inventory"
              className="text-[11px] text-muted-foreground block mb-1"
            >
              Total in stock
            </label>
            <div
              className="flex items-center justify-between px-3 py-2 bg-canvas-parchment rounded-lg"
              style={{ height: 44 }}
            >
              <input
                id="inventory"
                name="inventory"
                type="number"
                min="0"
                step="1"
                placeholder="0"
                className="flex-1 text-[16px] font-semibold tabular-nums text-foreground bg-transparent outline-none"
              />
            </div>
            <FieldError errors={fieldErrors?.inventory} />
          </div>
          <div className="flex items-center gap-2">
            <div className="w-8 h-4 rounded-full bg-good flex items-center justify-end pr-0.5">
              <div className="w-3 h-3 rounded-full bg-white shadow-sm" />
            </div>
            <span className="text-sm text-foreground">Allow pre-orders</span>
          </div>
        </div>

        {/* Shipping — static mockup */}
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
          <div>
            <label
              htmlFor="category"
              className="text-[11px] text-muted-foreground block mb-1"
            >
              Category
            </label>
            <input
              id="category"
              name="category"
              type="text"
              placeholder="Ceramics"
              className="w-full flex items-center rounded-md border border-border px-3 text-sm text-foreground focus:outline-none"
              style={{ height: 38 }}
            />
            <FieldError errors={fieldErrors?.category} />
          </div>
          {/* Tags — static mockup */}
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

        {/* Listing health — static mockup */}
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
            3 photos · clear title · price set · description over 100 chars. Add
            1 more photo to reach 100.
          </p>
        </div>
      </div>
    </form>
  );
}
