"use client";

import { useRouter } from "next/navigation";
import { ShopForm } from "@/components/shop-form";
import { openMerchant } from "@/lib/accounts";

export function OpenShopForm() {
  const router = useRouter();

  return (
    <ShopForm
      submitLabel="Open Shop"
      onSubmit={async (input) => {
        await openMerchant(input);
        router.push("/merchant");
        // Re-render the header, which now links to the Merchant Centre.
        router.refresh();
      }}
    />
  );
}
