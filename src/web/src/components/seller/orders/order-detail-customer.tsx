import type { OrderCustomer } from "@/lib/seller/orders/types";

export function OrderDetailCustomer({ customer }: { customer: OrderCustomer }) {
  const initials = customer.shortName
    .split(" ")
    .map((p) => p.charAt(0))
    .join("");
  return (
    <div className="rounded-xl border border-black/[0.06] bg-white p-4">
      <div className="mb-3 flex items-center gap-3">
        <div className="flex size-10 shrink-0 items-center justify-center rounded-full bg-surface-avatar-warm text-[13px] font-semibold text-foreground">
          {initials}
        </div>
        <div>
          <div className="text-[13.5px] font-semibold text-foreground">
            {customer.name}
          </div>
          <div className="text-xs text-foreground/50">
            {customer.lifetimeOrdersLabel}
          </div>
        </div>
        <button
          type="button"
          className="ml-auto text-xs font-medium text-foreground/50"
        >
          Profile →
        </button>
      </div>
      <div className="my-3 h-px bg-black/[0.06]" />
      <div className="text-xs text-foreground/50">Ship to</div>
      <div className="mt-0.5 text-sm text-foreground">
        {customer.ship.line1}
        <br />
        {customer.ship.line2}
      </div>
      {customer.billSameAsShip && (
        <div className="mt-3 text-xs text-foreground/50">
          Bill to · same as ship
        </div>
      )}
      <div className="mt-3 text-xs text-foreground/50">Email</div>
      <div className="text-sm text-foreground">{customer.email}</div>
    </div>
  );
}
