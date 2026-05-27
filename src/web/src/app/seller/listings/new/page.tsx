import { Suspense } from "react";
import { NewListingForm } from "@/components/seller/listings/new-listing-form";
import { SellerTopbar } from "@/components/seller/shell/seller-topbar";

export default function NewListingPage() {
  return (
    <div className="flex flex-col min-h-screen">
      <SellerTopbar title="New listing" subtitle="Listings · Drafts · 1 of 1" />

      <div className="flex-1 overflow-auto p-7">
        {/*
          The wizard reads `?step=` via useSearchParams, which requires a
          Suspense boundary under Next 16 to avoid bailing out of static
          rendering for the surrounding tree.
        */}
        <Suspense fallback={null}>
          <NewListingForm />
        </Suspense>
      </div>
    </div>
  );
}
