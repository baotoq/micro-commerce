import Link from "next/link";
import { redirect } from "next/navigation";
import { formatPrice } from "@/lib/catalog";
import { getCheckouts } from "@/lib/checkout";
import { getAccessToken, signInUrl } from "@/lib/session";

export default async function OrdersPage() {
  const accessToken = await getAccessToken();
  if (!accessToken) redirect(signInUrl("/orders"));
  const checkouts = await getCheckouts(accessToken);

  return (
    <main className="mx-auto w-full max-w-3xl px-6 py-10">
      <h1 className="mb-6 text-2xl font-semibold">Orders</h1>
      {checkouts.items.length === 0 ? (
        <p className="text-muted-foreground">
          You have no Checkouts yet. <Link href="/" className="underline">Browse Products</Link>
        </p>
      ) : (
        <ul aria-label="Checkouts" className="divide-y border-y">
          {checkouts.items.map((checkout) => (
            <li key={checkout.id} className="grid gap-2 py-4">
              <div className="flex flex-wrap justify-between gap-2">
                <h2 className="font-medium">
                  {checkout.lines.map((line) => line.productName).join(", ")}
                </h2>
                <p>{checkout.status === "AwaitingPayment" ? "Awaiting Payment" : "Expired"}</p>
              </div>
              <p className="text-sm text-muted-foreground">
                Expires {new Date(checkout.expiresAt).toLocaleString()}
              </p>
              <p className="text-sm tabular-nums">Total: {formatPrice(checkout.total)}</p>
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
