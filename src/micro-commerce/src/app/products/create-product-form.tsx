"use client";

import { useForm } from "@tanstack/react-form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { ApiError } from "@/lib/api";
import {
  createProduct,
  createProductSchema,
  productsQueryOptions,
} from "@/lib/products";

// Maps API validation failures onto the matching fields; anything else becomes a form-level error.
function toSubmitErrors(error: unknown) {
  if (!(error instanceof ApiError)) {
    return { form: "Something went wrong. Please try again." };
  }

  const fieldErrors = error.problem?.errors ?? [];
  if (fieldErrors.length === 0) return { form: error.message };

  return {
    fields: Object.fromEntries(fieldErrors.map((e) => [e.name, { message: e.reason }])),
  };
}

export function CreateProductForm() {
  const queryClient = useQueryClient();
  const { mutateAsync } = useMutation({
    mutationFn: createProduct,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: productsQueryOptions.queryKey }),
  });

  const form = useForm({
    defaultValues: { name: "", price: 0 },
    validators: {
      onChange: createProductSchema,
      onSubmitAsync: async ({ value }) => {
        try {
          await mutateAsync(createProductSchema.parse(value));
          return null;
        } catch (error) {
          return toSubmitErrors(error);
        }
      },
    },
    onSubmit: ({ formApi }) => formApi.reset(),
  });

  return (
    <form
      className="mb-8"
      onSubmit={(e) => {
        e.preventDefault();
        void form.handleSubmit();
      }}
    >
      <FieldGroup>
        <form.Field name="name">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid;
            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Name</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(e) => field.handleChange(e.target.value)}
                  aria-invalid={isInvalid}
                />
                {isInvalid && <FieldError errors={field.state.meta.errors} />}
              </Field>
            );
          }}
        </form.Field>

        <form.Field name="price">
          {(field) => {
            const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid;
            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Price</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  type="number"
                  inputMode="decimal"
                  step="0.01"
                  min={0}
                  value={Number.isNaN(field.state.value) ? "" : field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(e) => field.handleChange(e.target.valueAsNumber)}
                  aria-invalid={isInvalid}
                />
                {isInvalid && <FieldError errors={field.state.meta.errors} />}
              </Field>
            );
          }}
        </form.Field>

        <form.Subscribe selector={(state) => state.errorMap.onSubmit}>
          {(formError) =>
            typeof formError === "string" && <FieldError>{formError}</FieldError>
          }
        </form.Subscribe>

        <form.Subscribe selector={(state) => state.isSubmitting}>
          {(isSubmitting) => (
            <Field orientation="horizontal">
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? "Adding…" : "Add product"}
              </Button>
            </Field>
          )}
        </form.Subscribe>
      </FieldGroup>
    </form>
  );
}
