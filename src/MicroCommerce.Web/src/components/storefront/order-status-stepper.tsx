"use client";

import { Check, X } from "lucide-react";

const STEPS = ["Submitted", "Paid", "Confirmed", "Shipped", "Delivered"] as const;

function mapToCustomerStep(status: string): {
  activeIndex: number;
  isFailed: boolean;
} {
  switch (status) {
    case "Submitted":
    case "StockReserved":
      return { activeIndex: 0, isFailed: false };
    case "Paid":
      return { activeIndex: 1, isFailed: false };
    case "Confirmed":
      return { activeIndex: 2, isFailed: false };
    case "Shipped":
      return { activeIndex: 3, isFailed: false };
    case "Delivered":
      return { activeIndex: 4, isFailed: false };
    case "Failed":
    case "Cancelled":
      return { activeIndex: 0, isFailed: true };
    default:
      return { activeIndex: 0, isFailed: false };
  }
}

interface OrderStatusStepperProps {
  status: string;
  failureReason?: string | null;
}

export function OrderStatusStepper({
  status,
  failureReason,
}: OrderStatusStepperProps) {
  const { activeIndex, isFailed } = mapToCustomerStep(status);

  return (
    <div className="w-full">
      <div className="flex items-center justify-between">
        {STEPS.map((step, index) => {
          const isCompleted = !isFailed && index < activeIndex;
          const isCurrent = index === activeIndex;
          const isCurrentFailed = isCurrent && isFailed;
          const isLast = index === STEPS.length - 1;

          return (
            <div key={step} className="flex flex-1 items-center">
              {/* Step circle + label */}
              <div className="flex flex-col items-center">
                <div
                  className={`flex size-8 shrink-0 items-center justify-center rounded-full border-2 transition-colors sm:size-10 ${
                    isCompleted
                      ? "border-green-500 bg-green-500 text-white"
                      : isCurrentFailed
                        ? "border-red-500 bg-red-500 text-white"
                        : isCurrent
                          ? "animate-pulse border-blue-500 bg-blue-500 text-white"
                          : "border-zinc-200 bg-white text-zinc-400"
                  }`}
                >
                  {isCompleted ? (
                    <Check className="size-4 sm:size-5" />
                  ) : isCurrentFailed ? (
                    <X className="size-4 sm:size-5" />
                  ) : (
                    <span className="text-xs font-medium sm:text-sm">
                      {index + 1}
                    </span>
                  )}
                </div>
                <span
                  className={`mt-2 text-center text-[10px] font-medium sm:text-xs ${
                    isCompleted
                      ? "text-green-600"
                      : isCurrentFailed
                        ? "text-red-600"
                        : isCurrent
                          ? "text-blue-600"
                          : "text-zinc-400"
                  }`}
                >
                  {step}
                </span>
              </div>

              {/* Connecting line */}
              {!isLast && (
                <div
                  className={`mx-1 h-0.5 flex-1 sm:mx-2 ${
                    isCompleted ? "bg-green-500" : "bg-zinc-200"
                  }`}
                />
              )}
            </div>
          );
        })}
      </div>

      {/* Failure reason */}
      {isFailed && failureReason && (
        <p className="mt-4 rounded-lg bg-red-50 px-4 py-2 text-center text-sm text-red-600">
          {failureReason}
        </p>
      )}
    </div>
  );
}
