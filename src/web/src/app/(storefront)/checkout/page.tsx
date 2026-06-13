import { redirect } from "next/navigation";
import { CheckoutForm } from "@/components/storefront/checkout-form";
import { ProductImage } from "@/components/storefront/product-image";
import { auth } from "@/lib/auth/config";
import { fetchProductBySku } from "@/lib/catalog/api";
import { fetchPromotionByCode } from "@/lib/catalog/promotions";
import { money } from "@/lib/money";
import { readCart } from "@/lib/storefront/cart-cookie";
import { computeTotals, type PricedLine } from "@/lib/storefront/totals";

export default async function CheckoutPage() {
  const session = await auth();
  if (!session?.user?.email) {
    redirect("/api/auth/signin?callbackUrl=/checkout");
  }

  const cart = await readCart();
  if (cart.lines.length === 0) redirect("/cart");

  const products = await Promise.all(
    cart.lines.map((l) => fetchProductBySku(l.sku)),
  );
  const priced: PricedLine[] = [];
  cart.lines.forEach((line, i) => {
    const p = products[i];
    if (p && p.status !== "draft") {
      priced.push({
        sku: line.sku,
        name: p.name,
        price: p.price,
        qty: line.qty,
      });
    }
  });
  if (priced.length === 0) redirect("/cart");

  const promo = cart.promoCode
    ? await fetchPromotionByCode(cart.promoCode)
    : null;
  const totals = computeTotals(priced, promo, "standard");

  return (
    <div className="mx-auto max-w-5xl px-6 py-6">
      <div className="grid gap-10 lg:grid-cols-[1fr_360px]">
        <CheckoutForm email={session.user.email} totals={totals} />

        {/* Summary rail (Checkout_Desktop right column) */}
        <aside className="order-first lg:order-last lg:sticky lg:top-4 lg:self-start">
          <div className="rounded-xl border">
            <p className="border-b px-5 py-3.5 text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
              Order summary
            </p>
            <div className="px-5">
              {priced.map((l) => (
                <div
                  key={l.sku}
                  className="flex items-center gap-3 border-b py-3 last:border-b-0"
                >
                  <ProductImage
                    photoUrl={undefined}
                    category=""
                    name={l.name}
                    className="h-12 w-12"
                  />
                  <div className="grow">
                    <p className="text-[13px] font-semibold">{l.name}</p>
                    <p className="text-xs text-muted-foreground">×{l.qty}</p>
                  </div>
                  <span className="text-xs font-semibold tabular-nums">
                    {money(l.price * l.qty)}
                  </span>
                </div>
              ))}
            </div>
            <dl className="space-y-1.5 bg-muted/50 px-5 py-4 text-sm">
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Subtotal</dt>
                <dd className="tabular-nums">{money(totals.subtotal)}</dd>
              </div>
              {totals.discount > 0 && cart.promoCode && (
                <div className="flex justify-between text-green-700">
                  <dt>Promo · {cart.promoCode}</dt>
                  <dd className="tabular-nums">−{money(totals.discount)}</dd>
                </div>
              )}
              {/* Starting estimate at Standard shipping; the form's Review step
                  shows the live total for the chosen delivery method. */}
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Shipping · from</dt>
                <dd className="tabular-nums">{money(totals.shipping)}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Estimated tax</dt>
                <dd className="tabular-nums">{money(totals.tax)}</dd>
              </div>
              <div className="flex justify-between border-t pt-2 font-semibold">
                <dt>Total · from</dt>
                <dd className="tabular-nums text-lg">{money(totals.total)}</dd>
              </div>
            </dl>
          </div>
        </aside>
      </div>
    </div>
  );
}
