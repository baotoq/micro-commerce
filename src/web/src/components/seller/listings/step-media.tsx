"use client";

import { type Control, Controller } from "react-hook-form";
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import type {
  ProductFormInput,
  ProductFormOutput,
} from "@/lib/seller/listings/schema";
import { PhotoUploader } from "./photo-uploader";
import { TagsInput } from "./tags-input";

const SECTION_LABEL_CLASS =
  "text-[11px] font-semibold uppercase tracking-wider text-muted-foreground";

type Props = {
  control: Control<ProductFormInput, unknown, ProductFormOutput>;
};

export function StepMedia({ control }: Props) {
  return (
    <div className="flex flex-col gap-5">
      <div className="bg-white rounded-lg border border-border p-5">
        <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
          Photos
        </h4>
        <FormField
          control={control}
          name="photoUrls"
          render={({ field }) => (
            <FormItem className="gap-2">
              <FormLabel className={SECTION_LABEL_CLASS}>Upload</FormLabel>
              <FormControl>
                <PhotoUploader
                  value={field.value ?? []}
                  onChange={field.onChange}
                />
              </FormControl>
              <FormMessage className="mt-1 text-[11px]" />
            </FormItem>
          )}
        />
      </div>

      <div className="bg-white rounded-lg border border-border p-5">
        <h4 className="text-[13.5px] font-semibold text-foreground mb-3">
          Tags
        </h4>
        <Controller
          control={control}
          name="tags"
          render={({ field, fieldState }) => (
            <div className="gap-1">
              <p className={SECTION_LABEL_CLASS}>Tags</p>
              <TagsInput value={field.value ?? []} onChange={field.onChange} />
              {fieldState.error?.message && (
                <p className="mt-1 text-[11px] text-destructive">
                  {fieldState.error.message}
                </p>
              )}
            </div>
          )}
        />
      </div>
    </div>
  );
}
