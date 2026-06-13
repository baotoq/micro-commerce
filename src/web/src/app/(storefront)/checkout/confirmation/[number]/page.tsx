import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { auth } from "@/lib/auth/config";
import { fetchOrderConfirmation } from "@/lib/catalog/checkout";
import { money } from "@/lib/money";

export default async function ConfirmationPage({
  params,
}: {
  params: Promise<{ number: string }>;
}) {
  const session = await auth();
  if (!session) redirect("/api/auth/signin?callbackUrl=/");

  const { number } = await params;
  const orderNumber = Number(number);
  if (!Number.isInteger(orderNumber)) notFound();

  const order = await fetchOrderConfirmation(orderNumber);
  if (!order) notFound();

  return (
    <div className="mx-auto max-w-xl px-6 py-12 text-center">
      <p className="text-4xl">✓</p>
      <h1 className="font-display mt-3 text-3xl">
        Order <i>#{order.number}</i> placed.
      </h1>
      <p className="mt-2 text-sm text-muted-foreground">
        A confirmation is on its way to {order.customer.email}. Every piece is
        made one at a time — it ships in 3–5 days.
      </p>

      <div className="mt-8 rounded-xl border p-5 text-left">
        {order.lines.map((line) => (
          <div
            key={line.sku}
            className="flex justify-between border-b py-2.5 text-sm last:border-b-0"
          >
            <span>
              {line.productName}
              {line.qty > 1 ? ` ×${line.qty}` : ""}
            </span>
            <span className="tabular-nums">
              {money(line.unitPrice * line.qty)}
            </span>
          </div>
        ))}
        <dl className="mt-3 space-y-1 border-t pt-3 text-sm">
          <div className="flex justify-between">
            <dt className="text-muted-foreground">Subtotal</dt>
            <dd className="tabular-nums">{money(order.summary.subtotal)}</dd>
          </div>
          {order.summary.discountCode && (
            <div className="flex justify-between text-green-700">
              <dt>Promo · {order.summary.discountCode}</dt>
              <dd className="tabular-nums">
                −{money(order.summary.discountAmount)}
              </dd>
            </div>
          )}
          <div className="flex justify-between">
            <dt className="text-muted-foreground">Shipping</dt>
            <dd className="tabular-nums">{money(order.summary.shipping)}</dd>
          </div>
          <div className="flex justify-between">
            <dt className="text-muted-foreground">Tax</dt>
            <dd className="tabular-nums">{money(order.summary.tax)}</dd>
          </div>
          <div className="flex justify-between font-semibold">
            <dt>Paid</dt>
            <dd className="tabular-nums">{money(order.summary.paid)}</dd>
          </div>
        </dl>
      </div>

      <Link
        href="/"
        className="mt-8 inline-block rounded-full bg-foreground px-6 py-3 text-sm font-semibold text-background"
      >
        Keep shopping →
      </Link>
    </div>
  );
}
