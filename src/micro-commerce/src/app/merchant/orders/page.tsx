import { formatPrice } from "@/lib/catalog";
import { getMerchantOrders } from "@/lib/checkout";
import { requireMerchantSession } from "@/lib/session";

export default async function MerchantOrdersPage() {
  const { accessToken } = await requireMerchantSession();
  const orders = await getMerchantOrders(accessToken);

  return (
    <section>
      <h1 className="mb-6 text-2xl font-semibold">Orders</h1>
      {orders.items.length === 0 ? (
        <p className="text-muted-foreground">No orders to ship yet.</p>
      ) : (
        <ul aria-label="Orders" className="divide-y border-y">
          {orders.items.map((order) => (
            <li key={order.id} className="grid gap-3 py-5">
              <div className="flex flex-wrap justify-between gap-2">
                <h2 className="font-medium">Order {order.id.slice(0, 8)}</h2>
                <p>To Ship</p>
              </div>
              <p className="text-sm text-muted-foreground">
                Placed {new Date(order.createdAt).toLocaleString()}
              </p>
              <div className="text-sm">
                <p className="font-medium">{order.deliveryAddress.recipientName}</p>
                <p className="text-muted-foreground">
                  {order.deliveryAddress.street}, {order.deliveryAddress.ward}, {order.deliveryAddress.province}
                </p>
              </div>
              <ul className="text-sm">
                {order.lines.map((line) => (
                  <li key={line.variantId} className="flex justify-between gap-4 py-1">
                    <span>{line.productName} · {line.optionValues.join(" / ")} · Qty {line.quantity}</span>
                    <span className="shrink-0 tabular-nums">{formatPrice(line.unitPrice * line.quantity)}</span>
                  </li>
                ))}
              </ul>
              <p className="text-sm tabular-nums">
                Items: {formatPrice(order.subtotal)} · Shipping: {formatPrice(order.shippingFee)}
              </p>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
