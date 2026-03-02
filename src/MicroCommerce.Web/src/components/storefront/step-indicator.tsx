"use client";

import { Check } from "lucide-react";

import { cn } from "@/lib/utils";

interface Step {
  label: string;
  description?: string;
}

interface StepIndicatorProps {
  steps: Step[];
  currentStep: number;
}

export function StepIndicator({ steps, currentStep }: StepIndicatorProps) {
  return (
    <>
      {/* Desktop step indicator */}
      <nav
        className="hidden items-center gap-4 sm:flex"
        aria-label="Checkout steps"
      >
        {steps.map((step, index) => {
          const stepNumber = index + 1;
          const isCompleted = stepNumber < currentStep;
          const isActive = stepNumber === currentStep;

          return (
            <div key={step.label} className="flex items-center gap-4">
              {index > 0 && (
                <div
                  className={cn(
                    "h-0.5 w-12 shrink-0 lg:w-20",
                    isCompleted ? "bg-primary" : "bg-border",
                  )}
                  aria-hidden="true"
                />
              )}
              <div className="flex shrink-0 items-center gap-2">
                <div
                  className={cn(
                    "flex size-8 items-center justify-center rounded-full text-sm font-semibold",
                    isCompleted && "bg-primary text-primary-foreground",
                    isActive &&
                      "border-2 border-primary bg-primary text-primary-foreground",
                    !isCompleted &&
                      !isActive &&
                      "bg-muted text-muted-foreground",
                  )}
                  aria-current={isActive ? "step" : undefined}
                >
                  {isCompleted ? (
                    <Check className="size-4" aria-hidden="true" />
                  ) : (
                    stepNumber
                  )}
                </div>
                <span
                  className={cn(
                    "text-sm",
                    isActive && "font-medium text-foreground",
                    isCompleted && "font-medium text-foreground",
                    !isCompleted && !isActive && "text-muted-foreground",
                  )}
                >
                  {step.label}
                </span>
              </div>
            </div>
          );
        })}
      </nav>

      {/* Mobile step indicator */}
      <p className="text-sm text-muted-foreground sm:hidden">
        Step {currentStep} of {steps.length}: {steps[currentStep - 1]?.label}
      </p>
    </>
  );
}
