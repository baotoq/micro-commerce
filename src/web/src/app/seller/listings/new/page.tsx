import { Check } from "lucide-react";
import { NewListingForm } from "@/components/seller/listings/new-listing-form";
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
            <button
              type="submit"
              form="new-listing-form"
              className={buttonVariants()}
            >
              <Check size={14} /> Publish
            </button>
          </>
        }
      />

      <div className="flex-1 overflow-auto p-7">
        <div
          className="grid gap-5"
          style={{ gridTemplateColumns: "1.4fr 1fr" }}
        >
          <NewListingForm />
        </div>
      </div>
    </div>
  );
}
