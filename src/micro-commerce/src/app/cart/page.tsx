import Link from "next/link";
import { redirect } from "next/navigation";
import { formatPrice } from "@/lib/catalog";
import { flagReason, getCart } from "@/lib/cart";
import { getAccessToken, signInUrl } from "@/lib/session";
import { CartLineControls } from "./cart-line-controls";

export default async function CartPage() {
  const accessToken = await getAccessToken();
  if (!accessToken) redirect(signInUrl("/cart"));
  const cart = await getCart(accessToken);

  return (
    <main className="mx-auto w-full max-w-3xl px-6 py-10">
      <h1 className="mb-6 text-2xl font-semibold">Cart</h1>
      {cart.lines.length === 0 ? (
        <p className="text-muted-foreground">
          Your Cart is empty.{" "}
          <Link href="/" className="underline">
            Browse Products
          </Link>
        </p>
      ) : (
        <>
          <ul aria-label="Cart lines" className="divide-y border-y">
            {cart.lines.map((line) => (
              <li key={line.variantId} className="flex flex-wrap items-start justify-between gap-4 py-4">
                <div className="min-w-0">
                  {line.productId ? (
                    <Link href={`/products/${line.productId}`} className="font-medium hover:underline">
                      {line.productName}
                    </Link>
                  ) : (
                    <span className="font-medium text-muted-foreground">Product no longer exists</span>
                  )}
                  {line.optionValues.length > 0 && (
                    <p className="text-sm text-muted-foreground">{line.optionValues.join(" / ")}</p>
                  )}
                  {line.shopName && <p className="text-sm">Sold by {line.shopName}</p>}
                  {line.unitPrice !== null && (
                    <p className="text-sm tabular-nums">{formatPrice(line.unitPrice)} each</p>
                  )}
                  {line.flags.map((flag) => (
                    <p key={flag} className="text-sm text-destructive">
                      {flagReason(flag, line)}
                    </p>
                  ))}
                </div>
                <div className="flex items-start gap-4">
                  <CartLineControls line={line} />
                  <p className="w-28 text-right tabular-nums">
                    {line.lineTotal !== null ? formatPrice(line.lineTotal) : "–"}
                  </p>
                </div>
              </li>
            ))}
          </ul>
          <p className="mt-4 flex justify-between text-lg font-semibold">
            <span>Items total</span>
            <span className="tabular-nums" aria-label="Items total">
              {formatPrice(cart.itemsTotal)}
            </span>
          </p>
        </>
      )}
    </main>
  );
}
