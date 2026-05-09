// web/src/components/seller/top-products.tsx
import type { TopProduct } from "@/lib/seller/types";
import { money } from "@/lib/money";

export function TopProducts({ products }: { products: TopProduct[] }) {
  const max = Math.max(...products.map((p) => p.revenue), 1);
  return (
    <div className="rounded-lg border border-black/[0.06] bg-white p-5">
      <h2 className="text-base font-semibold tracking-tight">Top products</h2>
      <ul className="mt-4 space-y-3">
        {products.map((p) => {
          const pct = (p.revenue / max) * 100;
          return (
            <li key={p.sku}>
              <div className="flex items-baseline justify-between text-sm">
                <span>{p.name}</span>
                <span className="tabular-nums text-[#1d1d1f]/70">
                  {p.units} · {money(p.revenue)}
                </span>
              </div>
              <div className="mt-1 h-1.5 w-full rounded-full bg-black/[0.05]">
                <div className="h-1.5 rounded-full bg-[#1d1d1f]" style={{ width: `${pct}%` }} aria-hidden />
              </div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
