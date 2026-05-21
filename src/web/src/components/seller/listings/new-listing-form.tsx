"use client";

// React Compiler memoizes react-hook-form's proxy-backed `formState`, which
// stops field-level error subscriptions from firing — required errors never
// rendered until this file was opted out. See components/ui/form.tsx for the
// matching opt-out on the FormMessage side.
"use no memo";

import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { useForm } from "react-hook-form";
import type { z } from "zod";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { createListingAction } from "@/lib/seller/listings/actions";
import { productInputSchema } from "@/lib/seller/listings/schema";

// react-hook-form holds raw input values (strings from the DOM), so we validate
// against the schema's *input* type. The schema transforms price/inventory
// from strings → numbers, so the resolver's transformed output type differs
// from the field-value type — useForm needs both generics.
type FormValues = z.input<typeof productInputSchema>;
type FormOutput = z.output<typeof productInputSchema>;

const STATUS_OPTIONS = [
  { value: "active", label: "Active" },
  { value: "draft", label: "Draft" },
  { value: "low", label: "Low" },
  { value: "out", label: "Out" },
] as const;

const TEXT_INPUT_STYLE = {
  height: 44,
  border: "1.5px solid var(--border)",
} as const;

const NAME_INPUT_STYLE = {
  height: 44,
  border: "1.5px solid var(--foreground)",
} as const;

const TEXT_INPUT_CLASS =
  "rounded-md px-3.5 text-[15px] font-semibold text-foreground placeholder:font-normal placeholder:text-muted-foreground border-0 focus-visible:ring-0";

const NUMBER_INPUT_CLASS =
  "h-auto border-0 bg-transparent p-0 text-[16px] font-semibold tabular-nums text-foreground focus-visible:ring-0";

const SMALL_INPUT_CLASS =
  "rounded-md border border-border px-3 text-sm text-foreground focus-visible:ring-0";

const FIELD_LABEL_CLASS = "text-[11px] text-muted-foreground font-normal";

const SECTION_LABEL_CLASS =
  "text-[11px] font-semibold uppercase tracking-wider text-muted-foreground";

export function NewListingForm() {
  const router = useRouter();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isSubmitting, startTransition] = useTransition();

  const form = useForm<FormValues, unknown, FormOutput>({
    resolver: zodResolver(productInputSchema),
    // Validate on change so errors track the field's *dirty* state — a bare
    // focus+blur on an untouched field shouldn't flash "required"; only an
    // actual edit (typing, then clearing) should.
    mode: "onChange",
    defaultValues: {
      sku: "",
      name: "",
      category: "",
      price: "",
      inventory: "",
      status: "draft",
    },
  });

  function onSubmit(values: FormOutput) {
    setSubmitError(null);
    startTransition(async () => {
      const fd = new FormData();
      for (const [key, value] of Object.entries(values)) {
        fd.append(key, value == null ? "" : String(value));
      }
      const result = await createListingAction(fd);
      if (result.ok) {
        router.push("/seller/listings");
        return;
      }
      // Server-side field errors only fire when the action's Zod parse rejects
      // something the client schema accepted — surface them on the matching field.
      if (result.fieldErrors) {
        for (const [name, messages] of Object.entries(result.fieldErrors)) {
          if (messages?.length) {
            form.setError(name as keyof FormValues, { message: messages[0] });
          }
        }
        return;
      }
      setSubmitError(result.error);
    });
  }

  return (
    <Form {...form}>
      <form
        id="new-listing-form"
        onSubmit={form.handleSubmit(onSubmit)}
        className="contents"
        aria-busy={isSubmitting}
      >
        {submitError && (
          <p className="col-span-2 -mb-2 px-1 text-sm text-bad" role="alert">
            {submitError}
          </p>
        )}

        {/* Left column cards — wired */}
        <div className="flex flex-col gap-5">
          {/* Title & description card */}
          <div className="bg-white rounded-lg border border-border p-5">
            <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
              Title &amp; description
            </h4>

            <FormField
              control={form.control}
              name="sku"
              render={({ field }) => (
                <FormItem className="mb-2.5 space-y-1">
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
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem className="mb-2.5 space-y-1">
                  <FormLabel className={FIELD_LABEL_CLASS}>Name</FormLabel>
                  <FormControl>
                    <Input
                      type="text"
                      placeholder="Persimmon vase"
                      className={TEXT_INPUT_CLASS}
                      style={NAME_INPUT_STYLE}
                      autoComplete="off"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage className="mt-1 text-[11px]" />
                </FormItem>
              )}
            />

            {/* Description — static mockup */}
            <div
              className="rounded-md border border-black/[0.1] p-3.5"
              style={{ minHeight: 110 }}
            >
              <p className="text-sm leading-relaxed text-foreground">
                Hand-thrown stoneware with a soft persimmon glaze. Each piece is
                a little different — the rim has a slight asymmetry I happen to
                like. Best for short-stem flowers or
              </p>
              <span className="text-[11px] text-muted-foreground">|</span>
            </div>
            <div className="flex items-center justify-between mt-2">
              <span className="text-[11px] text-muted-foreground">
                Markdown OK
              </span>
              <span className="text-[11px] text-muted-foreground tabular-nums">
                142 / 800
              </span>
            </div>
          </div>

          {/* Photos card — static mockup */}
          <div className="bg-white rounded-lg border border-border p-5">
            <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
              Photos · 3 of 6
            </h4>
            <div className="grid grid-cols-3 gap-2">
              <div
                className="rounded-md bg-[#e2d5c8]"
                style={{ height: 120 }}
              />
              <div
                className="rounded-md bg-[#e3d4ce]"
                style={{ height: 120 }}
              />
              <div
                className="rounded-md bg-[#efe8d9]"
                style={{ height: 120 }}
              />
              <div
                className="rounded-md border-[1.5px] border-dashed border-black/20 bg-canvas-parchment flex flex-col items-center justify-center gap-1.5"
                style={{ height: 120 }}
              >
                <div className="w-[60%] h-1.5 bg-black/[0.06] rounded-full overflow-hidden">
                  <div className="h-full bg-foreground rounded-full w-[72%]" />
                </div>
                <span className="text-[10px] text-muted-foreground">
                  vase-04.heic · 1.4mb
                </span>
              </div>
              <div
                className="rounded-md border-[1.5px] border-dashed border-black/20 bg-canvas-parchment flex flex-col items-center justify-center gap-1.5 text-muted-foreground"
                style={{ height: 120 }}
              >
                <svg
                  aria-hidden="true"
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                  <polyline points="17 8 12 3 7 8" />
                  <line x1="12" y1="3" x2="12" y2="15" />
                </svg>
                <span className="text-[11px] text-muted-foreground">
                  Drag to add
                </span>
              </div>
              <div
                className="rounded-md bg-canvas-parchment flex items-center justify-center text-muted-foreground"
                style={{ height: 120 }}
              >
                <span className="text-[11px]">+</span>
              </div>
            </div>
          </div>
        </div>

        {/* Right column cards — wired */}
        <div className="flex flex-col gap-5">
          {/* Status card */}
          <div className="bg-white rounded-lg border border-border p-5">
            <FormField
              control={form.control}
              name="status"
              render={({ field }) => (
                <FormItem className="space-y-2">
                  <FormLabel className={SECTION_LABEL_CLASS}>Status</FormLabel>
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    disabled={field.disabled}
                  >
                    <FormControl>
                      <SelectTrigger
                        className="w-full rounded-md border border-border bg-white px-3 text-sm"
                        style={{ height: 38 }}
                      >
                        <SelectValue placeholder="Select status">
                          {(value) =>
                            STATUS_OPTIONS.find((o) => o.value === value)
                              ?.label ?? "Select status"
                          }
                        </SelectValue>
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {STATUS_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>
                          {opt.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage className="mt-1 text-[11px]" />
                </FormItem>
              )}
            />
          </div>

          {/* Pricing card */}
          <div className="bg-white rounded-lg border border-border p-5">
            <h4 className={`${SECTION_LABEL_CLASS} mb-3`}>Pricing</h4>
            <div className="grid grid-cols-2 gap-2">
              <FormField
                control={form.control}
                name="price"
                render={({ field }) => (
                  <FormItem className="space-y-1">
                    <FormLabel className={FIELD_LABEL_CLASS}>Price</FormLabel>
                    <div
                      className="flex items-center gap-1.5 rounded-md border border-border px-3"
                      style={{ height: 44 }}
                    >
                      <span className="text-muted-foreground tabular-nums text-sm">
                        $
                      </span>
                      <FormControl>
                        <Input
                          type="number"
                          min="0"
                          step="0.01"
                          placeholder="0.00"
                          className={NUMBER_INPUT_CLASS}
                          {...field}
                          value={field.value == null ? "" : String(field.value)}
                        />
                      </FormControl>
                    </div>
                    <FormMessage className="mt-1 text-[11px]" />
                  </FormItem>
                )}
              />

              <div className="space-y-1">
                <Label className={FIELD_LABEL_CLASS}>Compare-at</Label>
                <div
                  className="flex items-center gap-1.5 rounded-md border border-border px-3"
                  style={{ height: 44 }}
                >
                  <span className="text-muted-foreground tabular-nums text-sm">
                    $
                  </span>
                  <span className="text-[16px] tabular-nums text-muted-foreground">
                    —
                  </span>
                </div>
              </div>
            </div>
            <div className="flex items-center gap-2 mt-3.5 px-2.5 py-2 bg-canvas-parchment rounded-md">
              <span className="w-2 h-2 rounded-full bg-good shrink-0" />
              <span className="text-sm text-foreground">
                Suggested: $78–$94 based on 6 similar shops
              </span>
            </div>
          </div>

          {/* Inventory card */}
          <div className="bg-white rounded-lg border border-border p-5">
            <h4 className={`${SECTION_LABEL_CLASS} mb-3`}>Inventory</h4>
            <FormField
              control={form.control}
              name="inventory"
              render={({ field }) => (
                <FormItem className="mb-3 space-y-1">
                  <FormLabel className={FIELD_LABEL_CLASS}>
                    Total in stock
                  </FormLabel>
                  <div
                    className="flex items-center justify-between rounded-lg bg-canvas-parchment px-3 py-2"
                    style={{ height: 44 }}
                  >
                    <FormControl>
                      <Input
                        type="number"
                        min="0"
                        step="1"
                        placeholder="0"
                        className={NUMBER_INPUT_CLASS}
                        {...field}
                        value={field.value == null ? "" : String(field.value)}
                      />
                    </FormControl>
                  </div>
                  <FormMessage className="mt-1 text-[11px]" />
                </FormItem>
              )}
            />
            <div className="flex items-center gap-2">
              <div className="w-8 h-4 rounded-full bg-good flex items-center justify-end pr-0.5">
                <div className="w-3 h-3 rounded-full bg-white shadow-sm" />
              </div>
              <span className="text-sm text-foreground">Allow pre-orders</span>
            </div>
          </div>

          {/* Shipping — static mockup */}
          <div className="bg-white rounded-lg border border-border p-5">
            <h4 className={`${SECTION_LABEL_CLASS} mb-3`}>Shipping</h4>
            <div className="flex flex-col gap-1.5 text-sm text-muted-foreground">
              <div className="flex justify-between">
                <span>Weight</span>
                <span className="text-foreground tabular-nums">1.4 kg</span>
              </div>
              <div className="flex justify-between">
                <span>Origin</span>
                <span className="text-foreground">Oakland, CA</span>
              </div>
              <div className="flex justify-between">
                <span>Class</span>
                <span className="text-foreground">Fragile · standard</span>
              </div>
            </div>
          </div>

          {/* Category & tags card */}
          <div className="bg-white rounded-lg border border-border p-5">
            <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
              Category &amp; tags
            </h4>
            <FormField
              control={form.control}
              name="category"
              render={({ field }) => (
                <FormItem className="space-y-1">
                  <FormLabel className={FIELD_LABEL_CLASS}>Category</FormLabel>
                  <FormControl>
                    <Input
                      type="text"
                      placeholder="Ceramics"
                      className={SMALL_INPUT_CLASS}
                      style={{ height: 38 }}
                      autoComplete="off"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage className="mt-1 text-[11px]" />
                </FormItem>
              )}
            />
            {/* Tags — static mockup */}
            <p className="text-[11px] text-muted-foreground mt-3">Tags</p>
            <div className="flex flex-wrap gap-1.5 mt-1.5">
              {["hand-thrown", "stoneware", "persimmon", "small batch"].map(
                (tag) => (
                  <span
                    key={tag}
                    className="text-[11px] px-2 py-1 rounded-full bg-muted text-foreground"
                  >
                    {tag} <span className="opacity-50 ml-1">&times;</span>
                  </span>
                ),
              )}
              <span className="text-[11px] px-2 py-1 rounded-full text-muted-foreground">
                + add
              </span>
            </div>
          </div>

          {/* Listing health — static mockup */}
          <div className="bg-canvas-parchment rounded-lg p-5">
            <div className="flex items-center justify-between mb-2">
              <span className="text-[13.5px] font-semibold text-foreground">
                Listing health
              </span>
              <span className="text-[13.5px] font-semibold tabular-nums text-good">
                92
              </span>
            </div>
            <div className="h-1.5 bg-black/[0.06] rounded-full mb-2.5 overflow-hidden">
              <div className="h-full bg-good rounded-full w-[92%]" />
            </div>
            <p className="text-[11px] text-muted-foreground">
              3 photos · clear title · price set · description over 100 chars.
              Add 1 more photo to reach 100.
            </p>
          </div>
        </div>
      </form>
    </Form>
  );
}
