import type { PromoCode } from "@/lib/seller/types";

function ShareIcon() {
  return (
    <svg
      width="11"
      height="11"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <title>share</title>
      <path d="M14 9V5l6 6-6 6v-4H3V9h11z" />
    </svg>
  );
}

function EditIcon() {
  return (
    <svg
      width="11"
      height="11"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <title>edit</title>
      <path d="M14.7 4.3a1 1 0 0 1 1.4 1.4l-9 9L3 16l1.3-4.1 9.4-9.6z" />
    </svg>
  );
}

function ChevRIcon() {
  return (
    <svg
      width="12"
      height="12"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <title>chevron right</title>
      <path d="M7 4l6 6-6 6" />
    </svg>
  );
}

function formatRevenue(n: number): string {
  return `$${n.toLocaleString("en-US")}`;
}

export function PromosTable({ promos }: { promos: PromoCode[] }) {
  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="border-b border-black/[0.06]">
          <th className="px-4 py-3 text-left text-xs font-medium text-[#1d1d1f]/50">
            Code
          </th>
          <th className="px-4 py-3 text-left text-xs font-medium text-[#1d1d1f]/50">
            What it does
          </th>
          <th className="px-4 py-3 text-right text-xs font-medium text-[#1d1d1f]/50">
            Redemptions
          </th>
          <th className="px-4 py-3 text-right text-xs font-medium text-[#1d1d1f]/50">
            Driven revenue
          </th>
          <th className="px-4 py-3 text-left text-xs font-medium text-[#1d1d1f]/50">
            Window
          </th>
          <th className="px-4 py-3 text-left text-xs font-medium text-[#1d1d1f]/50">
            Status
          </th>
          <th className="px-4 py-3" />
        </tr>
      </thead>
      <tbody>
        {promos.map((promo) => (
          <tr
            key={promo.code}
            data-highlight={promo.highlight ? "true" : undefined}
            style={{
              background: promo.highlight
                ? "rgba(0,102,204,0.04)"
                : "transparent",
            }}
            className="border-b border-black/[0.04] last:border-0"
          >
            <td className="px-4 py-3">
              <div className="flex items-center gap-2">
                <span className="rounded-md bg-[#f5f5f7] px-2 py-1 font-mono text-[12px] font-semibold text-[#1d1d1f]">
                  {promo.code}
                </span>
                <button
                  type="button"
                  className="flex h-6 w-6 items-center justify-center rounded text-[#1d1d1f]/40 hover:bg-black/[0.04] hover:text-[#1d1d1f]"
                  aria-label={`Share ${promo.code}`}
                >
                  <ShareIcon />
                </button>
              </div>
            </td>
            <td className="px-4 py-3">
              <div className="text-[12.5px] font-semibold text-[#1d1d1f]">
                {promo.what}
              </div>
            </td>
            <td className="px-4 py-3 text-right tabular-nums">
              {promo.redemptions}
            </td>
            <td
              className="px-4 py-3 text-right tabular-nums font-semibold"
              style={{
                color:
                  promo.drivenRevenue > 0
                    ? "var(--foreground, #1d1d1f)"
                    : "#1d1d1f70",
              }}
            >
              {promo.drivenRevenue > 0
                ? formatRevenue(promo.drivenRevenue)
                : "—"}
            </td>
            <td className="px-4 py-3 text-xs text-[#1d1d1f]/50">
              {promo.window}
            </td>
            <td className="px-4 py-3">
              {promo.tone === "good" ? (
                <span className="flex items-center gap-1.5 rounded-full bg-emerald-50 px-2.5 py-1 text-[11px] font-semibold text-emerald-700">
                  <span className="inline-block h-1.5 w-1.5 rounded-full bg-emerald-500" />
                  {promo.status}
                </span>
              ) : (
                <span className="flex items-center gap-1.5 rounded-full bg-black/[0.04] px-2.5 py-1 text-[11px] font-semibold text-[#1d1d1f]/70">
                  <span className="inline-block h-1.5 w-1.5 rounded-full bg-[#1d1d1f]/30" />
                  {promo.status}
                </span>
              )}
            </td>
            <td className="px-4 py-3">
              <div className="flex gap-1">
                <button
                  type="button"
                  className="flex h-[26px] w-[26px] items-center justify-center rounded text-[#1d1d1f]/40 hover:bg-black/[0.04] hover:text-[#1d1d1f]"
                  aria-label={`Edit ${promo.code}`}
                >
                  <EditIcon />
                </button>
                <button
                  type="button"
                  className="flex h-[26px] w-[26px] items-center justify-center rounded text-[#1d1d1f]/40 hover:bg-black/[0.04] hover:text-[#1d1d1f]"
                  aria-label={`View ${promo.code}`}
                >
                  <ChevRIcon />
                </button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
