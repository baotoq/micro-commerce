// web/src/components/seller/range-tabs.tsx
"use client";

import { useState } from "react";
import { cn } from "@/lib/utils";

export function RangeTabs({ options, defaultValue }: { options: string[]; defaultValue: string }) {
  const [active, setActive] = useState(defaultValue);
  return (
    <div className="inline-flex rounded-full border border-black/[0.08] p-0.5">
      {options.map((opt) => (
        <button
          key={opt}
          type="button"
          onClick={() => setActive(opt)}
          aria-pressed={active === opt}
          className={cn(
            "rounded-full px-3 py-1 text-xs",
            active === opt ? "bg-[#1d1d1f] text-white" : "text-[#1d1d1f]/70",
          )}
        >
          {opt}
        </button>
      ))}
    </div>
  );
}
