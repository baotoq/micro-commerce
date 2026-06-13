import Link from "next/link";
import { CartLineRow } from "@/components/storefront/cart-line-row";
import { PromoForm } from "@/components/storefront/promo-form";
import { fetchProductBySku } from "@/lib/catalog/api";
import { fetchPromotionByCode } from "@/lib/catalog/promotions";
import { money } from "@/lib/money";
import { readCart } from "@/lib/storefront/cart-cookie";
import { computeTotals, type PricedLine } from "@/lib/storefront/totals";

export default async function CartPage() {
  const cart = await readCart();

  if (cart.lines.length === 0) {
    return (
      <div className="mx-auto flex max-w-6xl flex-col items-center px-6 py-24 text-center">
        <h1 className="font-display text-3xl">
          Your <i>bag</i> is empty.
        </h1>
        <Link
          href="/"
          className="mt-6 rounded-full bg-foreground px-5 py-2.5 text-xs font-semibold text-background"
        >
          Shop the collection →
        </Link>
      </div>
    );
  }

  const products = await Promise.all(
    cart.lines.map((l) => fetchProductBySku(l.sku)),
  );
  const rows = cart.lines
    .map((line, i) => ({ line, product: products[i] }))
    .filter((r) => r.product && r.product.status !== "draft");
  const stale = cart.lines.length - rows.length;

  const priced: PricedLine[] = rows.map((r) => ({
    sku: r.line.sku,
    name: r.product!.name,
    price: r.product!.price,
    qty: r.line.qty,
  }));
  const promo = cart.promoCode
    ? await fetchPromotionByCode(cart.promoCode)
    : null;
  const totals = computeTotals(priced, promo, "standard");

  return (
    <div className="mx-auto max-w-6xl px-6 py-6">
      <h1 className="font-display text-3xl">
        Your <i>bag</i>
      </h1>
      {stale > 0 && (
        <p className="mt-2 rounded-lg bg-amber-50 px-3 py-2 text-xs text-amber-800">
          {stale} item{stale > 1 ? "s are" : " is"} no longer available and was
          removed from view.
        </p>
      )}

      <div className="mt-4 grid gap-8 lg:grid-cols-[1fr_360px]">
        <div>
          {rows.map((r) => (
            <CartLineRow
              key={r.line.sku}
              product={r.product!}
              qty={r.line.qty}
            />
          ))}
        </div>

        <aside className="lg:sticky lg:top-4 lg:self-start">
          <div className="rounded-xl border p-5">
            <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
              Order summary
            </p>
            <dl className="mt-3 space-y-1.5 text-sm">
              <div className="flex justify-between">
                <dt className="text-muted-foreground">
                  Subtotal · {rows.length} items
                </dt>
                <dd className="tabular-nums">{money(totals.subtotal)}</dd>
              </div>
              {totals.discount > 0 && cart.promoCode && (
                <div className="flex justify-between text-green-700">
                  <dt>Promo · {cart.promoCode}</dt>
                  <dd className="tabular-nums">−{money(totals.discount)}</dd>
                </div>
              )}
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Shipping · from</dt>
                <dd className="tabular-nums">{money(totals.shipping)}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Estimated tax</dt>
                <dd className="tabular-nums">{money(totals.tax)}</dd>
              </div>
            </dl>
            <div className="my-4 border-t" />
            <div className="flex items-baseline justify-between">
              <span className="text-sm font-semibold">Total</span>
              <span className="text-2xl font-semibold tabular-nums">
                {money(totals.total)}
              </span>
            </div>
            <p className="mt-1 text-[11px] text-muted-foreground">
              USD · final shipping chosen at checkout
            </p>
            <Link
              href="/checkout"
              className="mt-4 block rounded-full bg-foreground py-3 text-center text-sm font-semibold text-background"
            >
              Checkout →
            </Link>
          </div>
          <div className="mt-3">
            <PromoForm appliedCode={cart.promoCode} />
          </div>
        </aside>
      </div>
    </div>
  );
}
