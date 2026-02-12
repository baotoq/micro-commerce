"use client";

import { CheckCircle } from "lucide-react";

export function VerifiedBadge() {
  return (
    <div className="flex items-center gap-1 text-xs text-emerald-600">
      <CheckCircle className="size-3.5" />
      <span>Verified Purchase</span>
    </div>
  );
}
