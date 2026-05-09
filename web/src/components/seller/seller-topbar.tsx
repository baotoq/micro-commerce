import { Bell } from "lucide-react";
import type React from "react";
import { BRAND } from "@/lib/seller/data";

export function SellerTopbar({
  title,
  subtitle,
  actions,
}: {
  title: string;
  subtitle?: string;
  actions?: React.ReactNode;
}) {
  const initial = BRAND.owner.charAt(0);
  return (
    <div className="flex items-center justify-between border-b border-black/[0.06] bg-white px-7 py-5">
      <div>
        {subtitle && (
          <p className="text-xs uppercase tracking-wider text-[#1d1d1f]/60">
            {subtitle}
          </p>
        )}
        <h1 className="mt-1 text-3xl font-semibold tracking-tight text-[#1d1d1f]">
          {title}
        </h1>
      </div>
      <div className="flex items-center gap-2">
        {actions}
        <button
          type="button"
          aria-label="Notifications"
          className="inline-flex size-9 items-center justify-center rounded-full text-[#1d1d1f]/70 hover:bg-black/[0.04] hover:text-[#1d1d1f]"
        >
          <Bell className="size-4" aria-hidden />
        </button>
        <div
          role="img"
          aria-label={`Signed in as ${BRAND.owner}`}
          className="flex size-8 items-center justify-center rounded-full bg-[#e8e3da] text-sm font-semibold text-[#1d1d1f]"
        >
          {initial}
        </div>
      </div>
    </div>
  );
}
