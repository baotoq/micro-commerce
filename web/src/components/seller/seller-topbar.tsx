import type React from "react";

export function SellerTopbar({
  title,
  subtitle,
  actions,
}: {
  title: string;
  subtitle?: string;
  actions?: React.ReactNode;
}) {
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
      {actions && <div className="flex items-center gap-2">{actions}</div>}
    </div>
  );
}
