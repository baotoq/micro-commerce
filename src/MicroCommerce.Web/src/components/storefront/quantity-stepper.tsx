"use client";

import { Minus, Plus } from "lucide-react";
import { cn } from "@/lib/utils";

interface QuantityStepperProps {
  value: number;
  onIncrement: () => void;
  onDecrement: () => void;
  min?: number;
  max?: number;
  disabled?: boolean;
  className?: string;
}

export function QuantityStepper({
  value,
  onIncrement,
  onDecrement,
  min = 1,
  max = 99,
  disabled = false,
  className,
}: QuantityStepperProps) {
  return (
    <div
      className={cn(
        "inline-flex h-9 items-center rounded-md border border-border",
        className,
      )}
    >
      <button
        type="button"
        onClick={onDecrement}
        disabled={value <= min || disabled}
        className="flex h-full w-9 items-center justify-center text-foreground transition-colors hover:bg-muted disabled:opacity-50"
        aria-label="Decrease quantity"
      >
        <Minus className="size-3.5" />
      </button>
      <span className="flex h-full w-12 items-center justify-center border-x border-border text-sm font-medium tabular-nums">
        {value}
      </span>
      <button
        type="button"
        onClick={onIncrement}
        disabled={value >= max || disabled}
        className="flex h-full w-9 items-center justify-center text-foreground transition-colors hover:bg-muted disabled:opacity-50"
        aria-label="Increase quantity"
      >
        <Plus className="size-3.5" />
      </button>
    </div>
  );
}
