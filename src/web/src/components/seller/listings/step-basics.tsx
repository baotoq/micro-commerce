"use client";

import { useEffect, useRef } from "react";
import { type Control, type UseFormSetError, useWatch } from "react-hook-form";
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import type {
  ProductFormInput,
  ProductFormOutput,
} from "@/lib/seller/listings/schema";
import { cn } from "@/lib/utils";

const TEXT_INPUT_STYLE = {
  height: 44,
  border: "1.5px solid var(--border)",
} as const;

const TEXT_INPUT_CLASS =
  "rounded-md px-3.5 text-[15px] font-semibold text-foreground placeholder:font-normal placeholder:text-muted-foreground border-0 focus-visible:ring-0";

const FIELD_LABEL_CLASS = "text-[11px] text-muted-foreground font-normal";

const DESCRIPTION_MAX = 2000;
const DESCRIPTION_WARN_AT = 1800;
const SKU_FETCH_DEBOUNCE_MS = 350;
const SKU_REGEX = /^[A-Z0-9-]{1,64}$/;

// The wizard's useForm uses the 3-generic form (TInput, TContext, TOutput) so
// price/inventory/weight type as strings on input but numbers after resolver
// transform. Steps mirror those generics so Control accepts the parent's form.
type Props = {
  control: Control<ProductFormInput, unknown, ProductFormOutput>;
  setError: UseFormSetError<ProductFormInput>;
  apiBase: string;
};

export function StepBasics({ control, setError, apiBase }: Props) {
  const sku = useWatch({ control, name: "sku" });
  const description = useWatch({ control, name: "description" }) ?? "";

  useSkuUniqueness({
    sku,
    apiBase,
    onTaken: () =>
      setError("sku", { type: "manual", message: "That SKU is taken" }),
  });

  const descLen = description.length;
  const counterClass =
    descLen >= DESCRIPTION_WARN_AT ? "text-warn" : "text-muted-foreground";

  return (
    <div className="flex flex-col gap-5">
      <div className="bg-white rounded-lg border border-border p-5">
        <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
          Title &amp; description
        </h4>

        <FormField
          control={control}
          name="sku"
          render={({ field }) => (
            <FormItem className="mb-2.5 gap-1">
              <FormLabel className={FIELD_LABEL_CLASS}>SKU</FormLabel>
              <FormControl>
                <Input
                  type="text"
                  placeholder="e.g. MC-VS-001"
                  className={TEXT_INPUT_CLASS}
                  style={TEXT_INPUT_STYLE}
                  autoComplete="off"
                  {...field}
                />
              </FormControl>
              <FormMessage className="mt-1 text-[11px]" />
            </FormItem>
          )}
        />

        <FormField
          control={control}
          name="name"
          render={({ field }) => (
            <FormItem className="mb-2.5 gap-1">
              <FormLabel className={FIELD_LABEL_CLASS}>Name</FormLabel>
              <FormControl>
                <Input
                  type="text"
                  placeholder="Persimmon vase"
                  className={TEXT_INPUT_CLASS}
                  style={{
                    height: 44,
                    border: "1.5px solid var(--foreground)",
                  }}
                  autoComplete="off"
                  {...field}
                />
              </FormControl>
              <FormMessage className="mt-1 text-[11px]" />
            </FormItem>
          )}
        />

        <FormField
          control={control}
          name="description"
          render={({ field }) => (
            <FormItem className="gap-1">
              <FormLabel className={FIELD_LABEL_CLASS}>Description</FormLabel>
              <FormControl>
                <textarea
                  placeholder="Hand-thrown stoneware with a soft persimmon glaze..."
                  className="min-h-[110px] w-full rounded-md border border-black/[0.1] p-3.5 text-sm leading-relaxed text-foreground placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-foreground/30"
                  maxLength={DESCRIPTION_MAX}
                  {...field}
                  value={field.value ?? ""}
                />
              </FormControl>
              <div className="flex items-center justify-between mt-1">
                <FormMessage className="text-[11px]" />
                <span
                  className={cn(
                    "text-[11px] tabular-nums ml-auto",
                    counterClass,
                  )}
                  aria-live="polite"
                >
                  {descLen} / {DESCRIPTION_MAX}
                </span>
              </div>
            </FormItem>
          )}
        />
      </div>

      <div className="bg-white rounded-lg border border-border p-5">
        <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
          Category
        </h4>
        <FormField
          control={control}
          name="category"
          render={({ field }) => (
            <FormItem className="gap-1">
              <FormLabel className={FIELD_LABEL_CLASS}>Category</FormLabel>
              <FormControl>
                <Input
                  type="text"
                  placeholder="Ceramics"
                  className="rounded-md border border-border px-3 text-sm text-foreground focus-visible:ring-0"
                  style={{ height: 38 }}
                  autoComplete="off"
                  {...field}
                />
              </FormControl>
              <FormMessage className="mt-1 text-[11px]" />
            </FormItem>
          )}
        />
      </div>
    </div>
  );
}

function useSkuUniqueness({
  sku,
  apiBase,
  onTaken,
}: {
  sku: string;
  apiBase: string;
  onTaken: () => void;
}) {
  // Latest fetch wins — older inflight requests are ignored via a ref counter.
  const seqRef = useRef(0);
  // Hold onTaken in a ref so the debounce effect doesn't restart on every
  // parent re-render (the callback identity changes each pass).
  const onTakenRef = useRef(onTaken);
  useEffect(() => {
    onTakenRef.current = onTaken;
  }, [onTaken]);

  useEffect(() => {
    const normalised = (sku ?? "").trim().toUpperCase();
    if (!normalised || !SKU_REGEX.test(normalised)) return;
    const seq = ++seqRef.current;

    const timer = setTimeout(() => {
      void (async () => {
        try {
          const res = await fetch(
            `${apiBase}/api/products/by-sku/${encodeURIComponent(normalised)}/exists`,
            { method: "GET" },
          );
          if (seq !== seqRef.current) return;
          if (res.status === 204) {
            onTakenRef.current();
          }
          // 404 → available; anything else → unknown, do nothing (PRD §5).
        } catch {
          // Network failure — treat as unknown (don't block).
        }
      })();
    }, SKU_FETCH_DEBOUNCE_MS);

    return () => clearTimeout(timer);
  }, [sku, apiBase]);
}
