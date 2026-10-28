"use client";

import { type AnyFieldApi, useForm } from "@tanstack/react-form";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldGroup, FieldLabel, FieldLegend, FieldSet } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { type ProductInput, productSchema } from "@/lib/catalog";
import { toSubmitErrors } from "@/lib/form-errors";

export type LeafCategory = { id: string; path: string };

const fieldNames = ["name", "description", "categoryId", "price", "stock", "weightGrams"] as const;

const emptyProduct: ProductInput = {
  name: "",
  description: "",
  categoryId: "",
  price: Number.NaN,
  stock: Number.NaN,
  weightGrams: Number.NaN,
};

function invalid(field: AnyFieldApi) {
  return field.state.meta.isTouched && !field.state.meta.isValid;
}

function NumberField({ field, label }: { field: AnyFieldApi; label: string }) {
  const isInvalid = invalid(field);
  const value = field.state.value as number;

  return (
    <Field data-invalid={isInvalid}>
      <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
      <Input
        id={field.name}
        name={field.name}
        type="number"
        inputMode="numeric"
        step={1}
        min={0}
        value={Number.isNaN(value) ? "" : value}
        onBlur={field.handleBlur}
        onChange={(e) => field.handleChange(e.target.valueAsNumber)}
        aria-invalid={isInvalid}
      />
      {isInvalid && <FieldError errors={field.state.meta.errors} />}
    </Field>
  );
}

// A Product without options: its details, a leaf Category and its single default Variant's price, stock
// and weight. Used both to create a Product and to edit one.
export function ProductForm({
  categories,
  defaultValues = emptyProduct,
  submitLabel,
  onSubmit,
  onCancel,
}: {
  categories: LeafCategory[];
  defaultValues?: ProductInput;
  submitLabel: string;
  onSubmit: (input: ProductInput) => Promise<void>;
  onCancel: () => void;
}) {
  const form = useForm({
    defaultValues,
    validators: {
      onChange: productSchema,
      onSubmitAsync: async ({ value }) => {
        try {
          await onSubmit(productSchema.parse(value));
          return null;
        } catch (error) {
          return toSubmitErrors(error, fieldNames);
        }
      },
    },
  });

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        void form.handleSubmit();
      }}
    >
      <FieldGroup>
        <form.Field name="name">
          {(field) => (
            <Field data-invalid={invalid(field)}>
              <FieldLabel htmlFor={field.name}>Name</FieldLabel>
              <Input
                id={field.name}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(e) => field.handleChange(e.target.value)}
                aria-invalid={invalid(field)}
              />
              {invalid(field) && <FieldError errors={field.state.meta.errors} />}
            </Field>
          )}
        </form.Field>

        <form.Field name="description">
          {(field) => (
            <Field data-invalid={invalid(field)}>
              <FieldLabel htmlFor={field.name}>Description</FieldLabel>
              <Textarea
                id={field.name}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(e) => field.handleChange(e.target.value)}
                aria-invalid={invalid(field)}
              />
              {invalid(field) && <FieldError errors={field.state.meta.errors} />}
            </Field>
          )}
        </form.Field>

        <form.Field name="categoryId">
          {(field) => (
            <Field data-invalid={invalid(field)}>
              <FieldLabel htmlFor={field.name}>Category</FieldLabel>
              <select
                id={field.name}
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(e) => field.handleChange(e.target.value)}
                aria-invalid={invalid(field)}
                className="h-8 w-full rounded-lg border border-input bg-transparent px-2.5 text-base outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 aria-invalid:border-destructive md:text-sm dark:bg-input/30"
              >
                <option value="">Choose a Category</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.path}
                  </option>
                ))}
              </select>
              {invalid(field) && <FieldError errors={field.state.meta.errors} />}
            </Field>
          )}
        </form.Field>

        <FieldSet>
          <FieldLegend>Price, stock and weight</FieldLegend>
          <form.Field name="price">{(field) => <NumberField field={field} label="Price (VND)" />}</form.Field>
          <form.Field name="stock">{(field) => <NumberField field={field} label="Stock" />}</form.Field>
          <form.Field name="weightGrams">{(field) => <NumberField field={field} label="Weight (grams)" />}</form.Field>
        </FieldSet>

        <form.Subscribe selector={(state) => state.errorMap.onSubmit}>
          {(formError) => typeof formError === "string" && <FieldError>{formError}</FieldError>}
        </form.Subscribe>

        <form.Subscribe selector={(state) => state.isSubmitting}>
          {(isSubmitting) => (
            <Field orientation="horizontal">
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? "Saving…" : submitLabel}
              </Button>
              <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
                Cancel
              </Button>
            </Field>
          )}
        </form.Subscribe>
      </FieldGroup>
    </form>
  );
}
