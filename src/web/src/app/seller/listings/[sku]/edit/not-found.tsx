// Rendered when `getListingBySku()` returns null and the page calls `notFound()`.
// Scoped to `[sku]/edit` so the rest of the seller layout (sidebar, topbar)
// stays mounted — only the page body swaps to this message.

import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";

export default function ListingEditNotFound() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-3 px-7 py-6 text-center">
      <h1 className="text-[24px] font-semibold tracking-tight text-foreground">
        Listing not found
      </h1>
      <p className="text-sm text-muted-foreground">
        The SKU you opened doesn&apos;t match any product in your catalog. It
        may have been deleted, or the URL was mistyped.
      </p>
      <Link
        href="/seller/listings"
        className={buttonVariants({ variant: "outline" })}
      >
        Back to listings
      </Link>
    </div>
  );
}
