"use client";

import { useRouter } from "next/navigation";
import { useActionState } from "react";
import { Button } from "@/components/ui/button";
import type { ActionResult } from "@/lib/seller/listings/actions";
import { updateListingAction } from "@/lib/seller/listings/actions";
import type { Listing } from "@/lib/seller/listings/types";

type Props = { listing: Listing };

const STATUS_OPTIONS = [
  { value: "active", label: "Active" },
  { value: "low", label: "Low" },
  { value: "out", label: "Out" },
  { value: "draft", label: "Draft" },
] as const;

export function EditListingForm({ listing }: Props) {
  const router = useRouter();
  const action = updateListingAction.bind(null, listing.sku);

  const [state, formAction, isPending] = useActionState<
    ActionResult | null,
    FormData
  >(async (_prev, formData) => {
    const result = await action(formData);
    if (result.ok) {
      router.refresh();
    }
    return result;
  }, null);

  return (
    <form action={formAction} className="flex flex-col gap-5">
      {/* Top-level feedback */}
      {state && !state.ok && (
        <div
          role="alert"
          className="rounded-md border border-bad/30 bg-bad/10 px-4 py-3 text-sm text-bad"
        >
          {state.error}
        </div>
      )}
      {state?.ok && (
        <output className="block rounded-md border border-good/30 bg-good/10 px-4 py-3 text-sm text-good">
          Saved successfully.
        </output>
      )}

      {/* Details card */}
      <div className="rounded-xl border border-black/[0.06] bg-white p-5">
        <h3 className="mb-4 text-[13.5px] font-semibold text-foreground">
          Details
        </h3>
        <div className="flex flex-col gap-4">
          {/* SKU — read-only */}
          <div>
            <label
              htmlFor="sku"
              className="mb-1 block text-[11px] font-medium uppercase tracking-wider text-foreground/60"
            >
              SKU
            </label>
            <input
              id="sku"
              name="sku"
              type="text"
              defaultValue={listing.sku}
              readOnly
              className="w-full rounded-md border border-black/[0.1] bg-canvas-parchment px-3 py-2 text-sm font-mono text-foreground/60"
            />
          </div>

          {/* Name */}
          <div>
            <label
              htmlFor="name"
              className="mb-1 block text-[11px] font-medium uppercase tracking-wider text-foreground/60"
            >
              Name
            </label>
            <input
              id="name"
              name="name"
              type="text"
              defaultValue={listing.name}
              required
              className="w-full rounded-md border border-black/[0.1] px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-foreground/30"
            />
            {!state?.ok && state?.fieldErrors?.name && (
              <p className="mt-1 text-[11px] text-bad">
                {state.fieldErrors.name[0]}
              </p>
            )}
          </div>

          {/* Category */}
          <div>
            <label
              htmlFor="category"
              className="mb-1 block text-[11px] font-medium uppercase tracking-wider text-foreground/60"
            >
              Category
            </label>
            <input
              id="category"
              name="category"
              type="text"
              defaultValue={listing.category}
              required
              className="w-full rounded-md border border-black/[0.1] px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-foreground/30"
            />
            {!state?.ok && state?.fieldErrors?.category && (
              <p className="mt-1 text-[11px] text-bad">
                {state.fieldErrors.category[0]}
              </p>
            )}
          </div>

          {/* Price + Inventory */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label
                htmlFor="price"
                className="mb-1 block text-[11px] font-medium uppercase tracking-wider text-foreground/60"
              >
                Price
              </label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-foreground/50">
                  $
                </span>
                <input
                  id="price"
                  name="price"
                  type="number"
                  step="0.01"
                  min="0"
                  defaultValue={listing.price}
                  required
                  className="w-full rounded-md border border-black/[0.1] py-2 pl-7 pr-3 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-foreground/30"
                />
              </div>
              {!state?.ok && state?.fieldErrors?.price && (
                <p className="mt-1 text-[11px] text-bad">
                  {state.fieldErrors.price[0]}
                </p>
              )}
            </div>

            <div>
              <label
                htmlFor="inventory"
                className="mb-1 block text-[11px] font-medium uppercase tracking-wider text-foreground/60"
              >
                Inventory
              </label>
              <input
                id="inventory"
                name="inventory"
                type="number"
                min="0"
                step="1"
                defaultValue={listing.inventory}
                required
                className="w-full rounded-md border border-black/[0.1] px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-foreground/30"
              />
              {!state?.ok && state?.fieldErrors?.inventory && (
                <p className="mt-1 text-[11px] text-bad">
                  {state.fieldErrors.inventory[0]}
                </p>
              )}
            </div>
          </div>

          {/* Status */}
          <div>
            <label
              htmlFor="status"
              className="mb-1 block text-[11px] font-medium uppercase tracking-wider text-foreground/60"
            >
              Status
            </label>
            <select
              id="status"
              name="status"
              defaultValue={listing.status}
              className="w-full rounded-md border border-black/[0.1] bg-white px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-foreground/30"
            >
              {STATUS_OPTIONS.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
            {!state?.ok && state?.fieldErrors?.status && (
              <p className="mt-1 text-[11px] text-bad">
                {state.fieldErrors.status[0]}
              </p>
            )}
          </div>
        </div>
      </div>

      {/* Actions */}
      <div className="flex justify-end">
        <Button type="submit" disabled={isPending}>
          {isPending ? "Saving…" : "Save changes"}
        </Button>
      </div>
    </form>
  );
}
