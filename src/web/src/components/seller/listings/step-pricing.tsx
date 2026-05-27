"use client";

import type { Control } from "react-hook-form";
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { ProductFormInput } from "@/lib/seller/listings/schema";

const STATUS_OPTIONS = [
  { value: "active", label: "Active" },
  { value: "draft", label: "Draft" },
  { value: "low", label: "Low" },
  { value: "out", label: "Out" },
] as const;

const NUMBER_INPUT_CLASS =
  "h-auto border-0 bg-transparent p-0 text-[16px] font-semibold tabular-nums text-foreground focus-visible:ring-0";

const FIELD_LABEL_CLASS = "text-[11px] text-muted-foreground font-normal";

const SECTION_LABEL_CLASS =
  "text-[11px] font-semibold uppercase tracking-wider text-muted-foreground";

type Props = {
  control: Control<ProductFormInput>;
};

export function StepPricing({ control }: Props) {
  return (
    <div className="flex flex-col gap-5">
      <div className="bg-white rounded-lg border border-border p-5">
        <FormField
          control={control}
          name="status"
          render={({ field }) => (
            <FormItem>
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
                        STATUS_OPTIONS.find((o) => o.value === value)?.label ??
                        "Select status"
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

      <div className="bg-white rounded-lg border border-border p-5">
        <h4 className={`${SECTION_LABEL_CLASS} mb-3`}>Pricing</h4>
        <FormField
          control={control}
          name="price"
          render={({ field }) => (
            <FormItem className="gap-1">
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
      </div>

      <div className="bg-white rounded-lg border border-border p-5">
        <h4 className={`${SECTION_LABEL_CLASS} mb-3`}>Inventory</h4>
        <FormField
          control={control}
          name="inventory"
          render={({ field }) => (
            <FormItem className="gap-1">
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
      </div>

      <div className="bg-white rounded-lg border border-border p-5">
        <h4 className={`${SECTION_LABEL_CLASS} mb-3`}>Shipping</h4>
        <div className="grid grid-cols-2 gap-3">
          <FormField
            control={control}
            name="weight"
            render={({ field }) => (
              <FormItem className="gap-1">
                <FormLabel className={FIELD_LABEL_CLASS}>Weight (kg)</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min="0"
                    step="0.01"
                    placeholder="0.00"
                    className="rounded-md border border-border px-3 text-sm text-foreground focus-visible:ring-0"
                    style={{ height: 38 }}
                    {...field}
                    value={field.value == null ? "" : String(field.value)}
                  />
                </FormControl>
                <FormMessage className="mt-1 text-[11px]" />
              </FormItem>
            )}
          />

          <FormField
            control={control}
            name="origin"
            render={({ field }) => (
              <FormItem className="gap-1">
                <FormLabel className={FIELD_LABEL_CLASS}>Origin</FormLabel>
                <FormControl>
                  <Input
                    type="text"
                    placeholder="Portland, OR"
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
    </div>
  );
}
