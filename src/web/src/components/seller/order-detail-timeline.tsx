import { Check, Info, MessageSquare, Package, Truck } from "lucide-react";
import type { OrderTimelineEvent } from "@/lib/seller/orders/types";

const ICON_MAP: Record<string, React.ReactNode> = {
  check: <Check size={12} strokeWidth={2.4} />,
  box: <Package size={12} />,
  truck: <Truck size={12} />,
  chat: <MessageSquare size={12} />,
  info: <Info size={12} />,
};

export function OrderDetailTimeline({
  timeline,
}: {
  timeline: OrderTimelineEvent[];
}) {
  return (
    <div className="rounded-xl border border-black/[0.06] bg-white p-[18px]">
      <div className="mb-3.5 text-[15px] font-semibold text-foreground">
        Timeline
      </div>
      <div className="flex flex-col">
        {timeline.map((e, i) => (
          <div
            key={e.title}
            className="flex items-start gap-3"
            style={{ paddingBottom: i < timeline.length - 1 ? 14 : 0 }}
          >
            <div className="flex w-6 shrink-0 flex-col items-center">
              <span
                className={`flex items-center justify-center rounded-full ${
                  e.tone === "warn"
                    ? "bg-canvas-parchment text-orange-700"
                    : e.on
                      ? "bg-foreground text-white"
                      : "bg-canvas-parchment text-foreground/50"
                }`}
                style={{ width: 24, height: 24 }}
              >
                {ICON_MAP[e.icon]}
              </span>
              {i < timeline.length - 1 && (
                <span
                  className="bg-black/[0.08]"
                  style={{ width: 1.5, flex: 1, minHeight: 18 }}
                />
              )}
            </div>
            <div className="flex-1 pt-0.5">
              <div className="flex justify-between">
                <span className="text-[13px] font-semibold text-foreground">
                  {e.title}
                </span>
                <span className="text-xs text-muted-foreground">{e.when}</span>
              </div>
              <div className="mt-0.5 text-xs text-muted-foreground">
                {e.sub}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
