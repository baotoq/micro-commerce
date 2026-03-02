"use client";

import { CheckCircle } from "lucide-react";
import { Badge } from "@/components/ui/badge";

export function VerifiedBadge() {
  return (
    <Badge
      variant="secondary"
      className="border-success/20 bg-success-bg text-success-foreground gap-1 text-xs font-medium"
    >
      <CheckCircle className="size-3" />
      Verified Purchase
    </Badge>
  );
}
