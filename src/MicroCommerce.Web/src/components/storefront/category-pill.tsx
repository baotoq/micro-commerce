"use client";

import type { LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

interface CategoryPillProps {
  label: string;
  icon?: LucideIcon;
  active?: boolean;
  onClick?: () => void;
}

export function CategoryPill({
  label,
  icon: Icon,
  active,
  onClick,
}: CategoryPillProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        "inline-flex items-center gap-2 rounded-full px-5 py-2.5 text-[13px] font-medium transition-colors",
        active
          ? "bg-primary text-primary-foreground"
          : "bg-muted text-foreground hover:bg-muted/80",
      )}
    >
      {Icon && <Icon className="size-4" />}
      {label}
    </button>
  );
}
