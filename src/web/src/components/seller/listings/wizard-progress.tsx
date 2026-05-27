"use client";

import { cn } from "@/lib/utils";

export type WizardStepDescriptor = {
  number: 1 | 2 | 3;
  label: string;
};

const STEPS: WizardStepDescriptor[] = [
  { number: 1, label: "Basics" },
  { number: 2, label: "Pricing & Inventory" },
  { number: 3, label: "Media & Discovery" },
];

type Props = {
  current: 1 | 2 | 3;
  className?: string;
};

export function WizardProgress({ current, className }: Props) {
  return (
    <ol
      className={cn("flex items-center gap-3", className)}
      aria-label="Wizard progress"
    >
      {STEPS.map((step) => {
        const completed = step.number < current;
        const active = step.number === current;
        return (
          <li
            key={step.number}
            aria-current={active ? "step" : undefined}
            className="flex items-center gap-2"
          >
            <span
              className={cn(
                "size-2 rounded-full",
                completed
                  ? "bg-good"
                  : active
                    ? "bg-foreground"
                    : "bg-black/[0.15]",
              )}
              aria-hidden="true"
            />
            <span
              className={cn(
                "text-[11px] tracking-wide uppercase",
                active
                  ? "font-semibold text-foreground"
                  : "text-muted-foreground",
              )}
            >
              {step.label}
            </span>
          </li>
        );
      })}
    </ol>
  );
}

export const WIZARD_STEPS = STEPS;
