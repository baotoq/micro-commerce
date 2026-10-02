import { redirect } from "next/navigation";
import { getCurrentAccount, signInUrl } from "@/lib/session";
import { OpenShopForm } from "./open-shop-form";

export default async function OpenShopPage() {
  const me = await getCurrentAccount();
  if (!me) redirect(signInUrl("/open-shop"));
  if (me.merchant) redirect("/merchant");

  return (
    <main className="mx-auto w-full max-w-xl px-6 py-16">
      <h1 className="mb-2 text-2xl font-semibold">Open your Shop</h1>
      <p className="mb-8 text-muted-foreground">
        Start selling on the Marketplace right away; no approval needed. Your Pickup Address is where your
        parcels ship from.
      </p>
      <OpenShopForm />
    </main>
  );
}
