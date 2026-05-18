// web/src/components/seller/analytics-kpi-row.tsx

import { KpiCard } from "@/components/seller/kpi-card";
import type { KpiPoint } from "@/lib/seller/analytics/types";

export function AnalyticsKpiRow({ kpis }: { kpis: KpiPoint[] }) {
  return (
    <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
      {kpis.map((k) => (
        <KpiCard key={k.label} kpi={k} />
      ))}
    </div>
  );
}
