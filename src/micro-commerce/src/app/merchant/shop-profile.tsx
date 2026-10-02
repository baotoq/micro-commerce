"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { ShopForm } from "@/components/shop-form";
import { Button } from "@/components/ui/button";
import { type Merchant, updateShop } from "@/lib/accounts";

export function ShopProfile({ merchant }: { merchant: Merchant }) {
  const router = useRouter();
  const [isEditing, setIsEditing] = useState(false);

  if (isEditing) {
    return (
      <section>
        <h1 className="mb-6 text-2xl font-semibold">Edit Shop profile</h1>
        <ShopForm
          showDescription
          defaultValues={{ ...merchant, description: merchant.description ?? "" }}
          submitLabel="Save"
          onSubmit={async (input) => {
            await updateShop(input);
            setIsEditing(false);
            // Re-render the server components that show the Shop profile.
            router.refresh();
          }}
          onCancel={() => setIsEditing(false)}
        />
      </section>
    );
  }

  const { street, ward, district, province } = merchant.pickupAddress;

  return (
    <section>
      <div className="mb-6 flex items-center justify-between gap-4">
        <h1 className="text-2xl font-semibold">{merchant.shopName}</h1>
        <Button variant="outline" onClick={() => setIsEditing(true)}>
          Edit
        </Button>
      </div>
      <dl className="grid gap-4 sm:grid-cols-[10rem_1fr]">
        <dt className="text-muted-foreground">Description</dt>
        <dd className="whitespace-pre-line">{merchant.description ?? "No description yet."}</dd>
        <dt className="text-muted-foreground">Pickup Address</dt>
        <dd>{[street, ward, district, province].join(", ")}</dd>
      </dl>
    </section>
  );
}
