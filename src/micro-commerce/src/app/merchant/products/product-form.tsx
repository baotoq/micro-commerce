"use client";

import { type AnyFieldApi, useForm } from "@tanstack/react-form";
import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { type ProductFormValues, type ProductInput, productSchema, variantsFor } from "@/lib/catalog";
import { toSubmitErrors } from "@/lib/form-errors";

export type LeafCategory = { id: string; path: string };

const maxOptions = 2;

const emptyProduct: ProductFormValues = {
  name: "",
  description: "",
  categoryId: "",
  options: [],
  variants: [{ optionValues: [], price: Number.NaN, stock: Number.NaN, weightGrams: Number.NaN }],
};

// The form fields API validation failures can land on; the rest become a form-level error.
function fieldNamesOf({ options, variants }: ProductFormValues) {
  return [
    "name",
    "description",
    "categoryId",
    ...options.flatMap((_, i) => [`options[${i}].name`, `options[${i}].values`]),
    ...variants.flatMap((_, i) => [`variants[${i}].price`, `variants[${i}].stock`, `variants[${i}].weightGrams`]),
  ];
}

function invalid(field: AnyFieldApi) {
  return field.state.meta.isTouched && !field.state.meta.isValid;
}

function TextField({ field, label, placeholder }: { field: AnyFieldApi; label: string; placeholder?: string }) {
  const isInvalid = invalid(field);

  return (
    <Field data-invalid={isInvalid}>
      <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
      <Input
        id={field.name}
        name={field.name}
        placeholder={placeholder}
        value={field.state.value as string}
        onBlur={field.handleBlur}
        onChange={(e) => field.handleChange(e.target.value)}
        aria-invalid={isInvalid}
      />
      {isInvalid && <FieldError errors={field.state.meta.errors} />}
    </Field>
  );
}

function NumberField({ field, label, hideLabel = false }: { field: AnyFieldApi; label: string; hideLabel?: boolean }) {
  const isInvalid = invalid(field);
  const value = field.state.value as number;

  return (
    <Field data-invalid={isInvalid}>
      <FieldLabel htmlFor={field.name} className={hideLabel ? "sr-only" : undefined}>
        {label}
      </FieldLabel>
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

// A Product: its details, a leaf Category, up to two options, and a Variant for every combination of the
// options' values (a single default Variant without options), each with its own price, stock and weight.
// Used both to create a Product and to edit one.
export function ProductForm({
  categories,
  defaultValues = emptyProduct,
  submitLabel,
  onSubmit,
  onCancel,
}: {
  categories: LeafCategory[];
  defaultValues?: ProductFormValues;
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
          return toSubmitErrors(error, fieldNamesOf(value));
        }
      },
    },
  });

  // The Variant grid follows the options: a row per combination, keeping what was entered for the rest.
  const syncVariants = () =>
    form.setFieldValue("variants", variantsFor(form.state.values.options, form.state.values.variants));

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        void form.handleSubmit();
      }}
    >
      <FieldGroup>
        <form.Field name="name">{(field) => <TextField field={field} label="Name" />}</form.Field>

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

        <form.Field name="options" mode="array">
          {(optionsField) => (
            <FieldSet>
              <FieldLegend>Options</FieldLegend>
              <FieldDescription>
                Up to two, such as Colour or Size. Every combination of their values is a Variant with its own
                price, stock and weight.
              </FieldDescription>
              {optionsField.state.value.map((_, i) => (
                <div key={i} className="grid gap-3 sm:grid-cols-[1fr_2fr_auto] sm:items-start">
                  <form.Field name={`options[${i}].name`}>
                    {(field) => <TextField field={field} label={`Option ${i + 1} name`} placeholder="Colour" />}
                  </form.Field>
                  <form.Field name={`options[${i}].values`} listeners={{ onChange: syncVariants }}>
                    {(field) => (
                      <TextField field={field} label={`Option ${i + 1} values`} placeholder="Red, Blue" />
                    )}
                  </form.Field>
                  <Button
                    type="button"
                    variant="outline"
                    className="sm:mt-6"
                    aria-label={`Remove option ${i + 1}`}
                    onClick={() => {
                      optionsField.removeValue(i);
                      syncVariants();
                    }}
                  >
                    Remove
                  </Button>
                </div>
              ))}
              {optionsField.state.value.length < maxOptions && (
                <Button
                  type="button"
                  variant="outline"
                  className="self-start"
                  onClick={() => {
                    optionsField.pushValue({ name: "", values: "" });
                    syncVariants();
                  }}
                >
                  Add option
                </Button>
              )}
            </FieldSet>
          )}
        </form.Field>

        <form.Subscribe selector={(state) => state.values.options.map((option) => option.name.trim())}>
          {(optionNames) => (
            <form.Field name="variants">
              {(variantsField) =>
                optionNames.length === 0 ? (
                  <FieldSet>
                    <FieldLegend>Price, stock and weight</FieldLegend>
                    <form.Field name="variants[0].price">
                      {(field) => <NumberField field={field} label="Price (VND)" />}
                    </form.Field>
                    <form.Field name="variants[0].stock">
                      {(field) => <NumberField field={field} label="Stock" />}
                    </form.Field>
                    <form.Field name="variants[0].weightGrams">
                      {(field) => <NumberField field={field} label="Weight (grams)" />}
                    </form.Field>
                  </FieldSet>
                ) : (
                  <FieldSet>
                    <FieldLegend>Variants</FieldLegend>
                    {variantsField.state.value.length === 0 ? (
                      <FieldDescription>Give each option at least one value to set prices.</FieldDescription>
                    ) : (
                      <table className="w-full text-sm">
                        <thead className="text-left text-muted-foreground">
                          <tr>
                            {optionNames.map((name, i) => (
                              <th key={i} className="py-2 pr-3 font-normal">
                                {name || `Option ${i + 1}`}
                              </th>
                            ))}
                            <th className="py-2 pr-3 font-normal">Price (VND)</th>
                            <th className="py-2 pr-3 font-normal">Stock</th>
                            <th className="py-2 font-normal">Weight (grams)</th>
                          </tr>
                        </thead>
                        <tbody>
                          {variantsField.state.value.map((variant, i) => {
                            const label = variant.optionValues.join(" / ");
                            return (
                              <tr key={label} className="align-top">
                                {variant.optionValues.map((value, j) => (
                                  <td key={j} className="py-2 pr-3 leading-8">
                                    {value}
                                  </td>
                                ))}
                                <td className="py-2 pr-3">
                                  <form.Field name={`variants[${i}].price`}>
                                    {(field) => <NumberField field={field} label={`Price (VND) of ${label}`} hideLabel />}
                                  </form.Field>
                                </td>
                                <td className="py-2 pr-3">
                                  <form.Field name={`variants[${i}].stock`}>
                                    {(field) => <NumberField field={field} label={`Stock of ${label}`} hideLabel />}
                                  </form.Field>
                                </td>
                                <td className="py-2">
                                  <form.Field name={`variants[${i}].weightGrams`}>
                                    {(field) => (
                                      <NumberField field={field} label={`Weight (grams) of ${label}`} hideLabel />
                                    )}
                                  </form.Field>
                                </td>
                              </tr>
                            );
                          })}
                        </tbody>
                      </table>
                    )}
                  </FieldSet>
                )
              }
            </form.Field>
          )}
        </form.Subscribe>

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
