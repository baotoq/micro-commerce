import { Bell } from "lucide-react";
import type React from "react";
import { SignOut } from "@/components/seller/shell/sign-out";
import { BRAND } from "@/lib/seller/brand";

export function SellerTopbar({
  title,
  subtitle,
  actions,
  accountEmail,
}: {
  title: string;
  subtitle?: string;
  actions?: React.ReactNode;
  /** Signed-in seller email surfaced next to the avatar, when available. */
  accountEmail?: string;
}) {
  const initial = BRAND.owner.charAt(0);
  return (
    <div className="flex items-center justify-between border-b border-black/[0.06] bg-white px-7 py-5">
      <div>
        <h1 className="text-3xl font-semibold tracking-tight text-foreground">
          {title}
        </h1>
        {subtitle && (
          <p className="mt-1 text-xs uppercase tracking-wider text-foreground/60">
            {subtitle}
          </p>
        )}
      </div>
      <div className="flex items-center gap-2">
        {actions}
        <button
          type="button"
          aria-label="Notifications"
          className="inline-flex size-9 items-center justify-center rounded-full text-foreground/70 hover:bg-black/[0.04] hover:text-foreground"
        >
          <Bell className="size-4" aria-hidden />
        </button>
        <div
          role="img"
          aria-label={`Signed in as ${BRAND.owner}`}
          className="flex size-8 items-center justify-center rounded-full bg-surface-avatar-warm text-sm font-semibold text-foreground"
        >
          {initial}
        </div>
        {accountEmail && (
          <span className="max-w-40 truncate text-xs text-foreground/60">
            {accountEmail}
          </span>
        )}
        <SignOut />
      </div>
    </div>
  );
}
