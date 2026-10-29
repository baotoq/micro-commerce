import Link from "next/link";
import { redirect } from "next/navigation";
import { formatPrice } from "@/lib/catalog";
import { flagReason, getCart } from "@/lib/cart";
import { getAccessToken, signInUrl } from "@/lib/session";
import { CheckoutForm } from "./checkout-form";

const shippingFeePerMerchant = 30_000;

export default async function CheckoutPage() {
  const accessToken = await getAccessToken();
  if (!accessToken) redirect(signInUrl("/checkout"));
  const cart = await getCart(accessToken);
  if (cart.lines.length === 0) redirect("/cart");

  const shippingTotal = new Set(cart.lines.map((line) => line.shopId).filter(Boolean)).size * shippingFeePerMerchant;

  return (
    <main className="mx-auto w-full max-w-3xl px-6 py-10">
      <h1 className="mb-6 text-2xl font-semibold">Checkout</h1>
      <ul aria-label="Checkout lines" className="divide-y border-y">
        {cart.lines.map((line) => (
          <li key={line.variantId} className="flex justify-between gap-4 py-4">
            <div>
              <p className="font-medium">{line.productName ?? "Product no longer exists"}</p>
              {line.optionValues.length > 0 && (
                <p className="text-sm text-muted-foreground">{line.optionValues.join(" / ")}</p>
              )}
              <p className="text-sm text-muted-foreground">Quantity: {line.quantity}</p>
              {line.flags.map((flag) => (
                <p key={flag} className="text-sm text-destructive">
                  {flagReason(flag, line)}
                </p>
              ))}
            </div>
            <p className="text-right tabular-nums">
              {line.lineTotal !== null ? formatPrice(line.lineTotal) : "–"}
            </p>
          </li>
        ))}
      </ul>
      <dl className="mt-5 grid gap-2">
        <div className="flex justify-between">
          <dt>Items total</dt>
          <dd aria-label="Items total" className="tabular-nums">{formatPrice(cart.itemsTotal)}</dd>
        </div>
        <div className="flex justify-between">
          <dt>Shipping Fee</dt>
          <dd aria-label="Shipping Fee" className="tabular-nums">{formatPrice(shippingTotal)}</dd>
        </div>
      </dl>
      <CheckoutForm itemsTotal={cart.itemsTotal} />
      <Link href="/cart" className="mt-4 inline-block text-sm underline">
        Back to Cart
      </Link>
    </main>
  );
}
