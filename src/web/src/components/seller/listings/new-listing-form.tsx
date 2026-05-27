"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState, useTransition } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { Form } from "@/components/ui/form";
import { createListingAction } from "@/lib/seller/listings/actions";
import { computeListingHealth } from "@/lib/seller/listings/health";
import {
  type ProductFormInput,
  type ProductFormOutput,
  productInputSchema,
  validateStep,
  type WizardStep,
} from "@/lib/seller/listings/schema";
import { cn } from "@/lib/utils";
import { StepBasics } from "./step-basics";
import { StepMedia } from "./step-media";
import { StepPricing } from "./step-pricing";
import { WizardProgress } from "./wizard-progress";

const STEP_FIELDS: Record<WizardStep, readonly string[]> = {
  1: ["sku", "name", "category", "description"],
  2: ["price", "inventory", "status", "weight", "origin"],
  3: ["photoUrls", "tags"],
};

function parseStepParam(value: string | null): WizardStep {
  if (value === "2") return 2;
  if (value === "3") return 3;
  return 1;
}

function healthBand(score: number): "bad" | "warn" | "good" {
  if (score >= 85) return "good";
  if (score >= 60) return "warn";
  return "bad";
}

export function NewListingForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isSubmitting, startTransition] = useTransition();

  // Step is component state seeded from the URL so a refresh preserves
  // position. router.replace mirrors changes back to the URL for
  // shareability — but the source of truth is React state so the wizard
  // re-renders synchronously on Next/Back clicks without depending on a
  // search-params re-subscription.
  const [step, setStep] = useState<WizardStep>(() =>
    parseStepParam(searchParams.get("step")),
  );
  // biome-ignore lint/correctness/useExhaustiveDependencies: step is read inside but excluded — re-running on step changes would loop with goToStep's setState.
  useEffect(() => {
    const urlStep = parseStepParam(searchParams.get("step"));
    if (urlStep !== step) setStep(urlStep);
  }, [searchParams]);

  const form = useForm<ProductFormInput, unknown, ProductFormOutput>({
    resolver: zodResolver(productInputSchema),
    mode: "onChange",
    defaultValues: {
      sku: "",
      name: "",
      category: "",
      description: "",
      price: "",
      inventory: "",
      status: "draft",
      weight: "",
      origin: "",
      tags: [],
      photoUrls: [],
    },
  });

  // Single subscription powering both the listing-health card and the per-step
  // Next gate. useWatch's subscription is registered through RHF's internal
  // store so the React Compiler doesn't memo over a stale slice.
  const watched = useWatch({ control: form.control });

  const stepOk = validateStep(step, watched as Record<string, unknown>);

  const health = computeListingHealth({
    name: watched.name,
    description: watched.description,
    category: watched.category,
    price: watched.price,
    inventory: watched.inventory,
    weight: watched.weight,
    origin: watched.origin,
    tags: watched.tags,
    photoUrls: watched.photoUrls,
  });
  const band = healthBand(health.score);

  function goToStep(next: WizardStep) {
    setStep(next);
    const params = new URLSearchParams(searchParams.toString());
    params.set("step", String(next));
    router.replace(`?${params.toString()}`);
  }

  function onSubmit(values: ProductFormOutput) {
    setSubmitError(null);
    startTransition(async () => {
      const fd = new FormData();
      for (const [key, value] of Object.entries(values)) {
        if (Array.isArray(value)) {
          for (const item of value) fd.append(key, String(item));
        } else if (value != null) {
          fd.append(key, String(value));
        }
      }
      const result = await createListingAction(fd);
      if (result.ok) {
        router.push("/seller/listings");
        return;
      }
      if (result.fieldErrors) {
        for (const [name, messages] of Object.entries(result.fieldErrors)) {
          if (messages?.length) {
            form.setError(name as keyof ProductFormInput, {
              message: messages[0],
            });
          }
        }
        const firstField = Object.keys(result.fieldErrors)[0];
        const targetStep = stepForField(firstField);
        if (targetStep && targetStep !== step) goToStep(targetStep);
        return;
      }
      setSubmitError(result.error);
    });
  }

  // Same-origin: rewrite in next.config.ts proxies /api/* to the catalog API.
  const apiBase = "";

  return (
    <Form {...form}>
      <form
        id="new-listing-form"
        onSubmit={form.handleSubmit(onSubmit)}
        className="flex flex-col gap-5"
        aria-busy={isSubmitting}
      >
        <WizardProgress current={step} />

        {submitError && (
          <p className="-mb-2 px-1 text-sm text-bad" role="alert">
            {submitError}
          </p>
        )}

        <div
          className="grid gap-5"
          style={{ gridTemplateColumns: "1.4fr 1fr" }}
        >
          <div>
            {step === 1 && (
              <StepBasics
                control={form.control}
                setError={form.setError}
                apiBase={apiBase}
              />
            )}
            {step === 2 && <StepPricing control={form.control} />}
            {step === 3 && <StepMedia control={form.control} />}
          </div>

          <aside className="flex flex-col gap-5">
            <div className="bg-canvas-parchment rounded-lg p-5">
              <div className="flex items-center justify-between mb-2">
                <span className="text-[13.5px] font-semibold text-foreground">
                  Listing health
                </span>
                <span
                  data-testid="listing-health-score"
                  className={cn(
                    "text-[13.5px] font-semibold tabular-nums",
                    band === "good"
                      ? "text-good"
                      : band === "warn"
                        ? "text-warn"
                        : "text-bad",
                  )}
                >
                  {health.score}
                </span>
              </div>
              <div className="h-1.5 bg-black/[0.06] rounded-full mb-2.5 overflow-hidden">
                <div
                  className={cn(
                    "h-full rounded-full",
                    band === "good"
                      ? "bg-good"
                      : band === "warn"
                        ? "bg-warn"
                        : "bg-bad",
                  )}
                  style={{ width: `${health.score}%` }}
                />
              </div>
              {health.reasons.length > 0 ? (
                <ul className="text-[11px] text-muted-foreground space-y-0.5">
                  {health.reasons.map((reason) => (
                    <li key={reason}>· {reason}</li>
                  ))}
                </ul>
              ) : (
                <p className="text-[11px] text-good">
                  All set — your listing scores 100.
                </p>
              )}
            </div>
          </aside>
        </div>

        <div className="flex items-center justify-between gap-3 pt-2">
          <Button
            type="button"
            variant="outline"
            onClick={() =>
              step > 1 ? goToStep((step - 1) as WizardStep) : undefined
            }
            disabled={step === 1}
          >
            Back
          </Button>

          <div className="flex items-center gap-2">
            {step < 3 && (
              <Button
                type="button"
                onClick={() => goToStep((step + 1) as WizardStep)}
                disabled={!stepOk.ok}
              >
                Next
              </Button>
            )}
            {step === 3 && (
              <Button type="submit" disabled={isSubmitting || !stepOk.ok}>
                {isSubmitting ? "Publishing…" : "Publish"}
              </Button>
            )}
          </div>
        </div>
      </form>
    </Form>
  );
}

function stepForField(field: string): WizardStep | null {
  if (STEP_FIELDS[1].includes(field)) return 1;
  if (STEP_FIELDS[2].includes(field)) return 2;
  if (STEP_FIELDS[3].includes(field)) return 3;
  return null;
}
