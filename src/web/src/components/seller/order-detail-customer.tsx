import type { OrderCustomer } from "@/lib/seller/types";

export function OrderDetailCustomer({ customer }: { customer: OrderCustomer }) {
  const initials = customer.shortName
    .split(" ")
    .map((p) => p.charAt(0))
    .join("");
  return (
    <div className="rounded-xl border border-black/[0.06] bg-white p-4">
      <div className="mb-3 flex items-center gap-3">
        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-[#e8e3da] text-[13px] font-semibold text-[#1d1d1f]">
          {initials}
        </div>
        <div>
          <div className="text-[13.5px] font-semibold text-[#1d1d1f]">
            {customer.name}
          </div>
          <div className="text-xs text-[#1d1d1f]/50">
            {customer.lifetimeOrdersLabel}
          </div>
        </div>
        <button
          type="button"
          className="ml-auto text-xs font-medium text-[#1d1d1f]/50"
        >
          Profile →
        </button>
      </div>
      <div className="my-3 h-px bg-black/[0.06]" />
      <div className="text-xs text-[#1d1d1f]/50">Ship to</div>
      <div className="mt-0.5 text-sm text-[#1d1d1f]">
        {customer.ship.line1}
        <br />
        {customer.ship.line2}
      </div>
      {customer.billSameAsShip && (
        <div className="mt-3 text-xs text-[#1d1d1f]/50">
          Bill to · same as ship
        </div>
      )}
      <div className="mt-3 text-xs text-[#1d1d1f]/50">Email</div>
      <div className="text-sm text-[#1d1d1f]">{customer.email}</div>
    </div>
  );
}
