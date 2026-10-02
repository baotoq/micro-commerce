"use client";

import { type AnyFieldApi, useForm } from "@tanstack/react-form";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldGroup, FieldLabel, FieldLegend, FieldSet } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { type ShopInput, shopSchema } from "@/lib/accounts";
import { toSubmitErrors } from "@/lib/form-errors";

const fieldNames = [
  "shopName",
  "description",
  "pickupAddress.street",
  "pickupAddress.ward",
  "pickupAddress.district",
  "pickupAddress.province",
] as const;

const emptyShop: ShopInput = {
  shopName: "",
  description: "",
  pickupAddress: { street: "", ward: "", district: "", province: "" },
};

function TextField({
  field,
  label,
  multiline = false,
}: {
  field: AnyFieldApi;
  label: string;
  multiline?: boolean;
}) {
  const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid;
  const props = {
    id: field.name,
    name: field.name,
    value: field.state.value as string,
    onBlur: field.handleBlur,
    "aria-invalid": isInvalid,
  };

  return (
    <Field data-invalid={isInvalid}>
      <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
      {multiline ? (
        <Textarea {...props} onChange={(e) => field.handleChange(e.target.value)} />
      ) : (
        <Input {...props} onChange={(e) => field.handleChange(e.target.value)} />
      )}
      {isInvalid && <FieldError errors={field.state.meta.errors} />}
    </Field>
  );
}

// The Shop profile form, used both to open a Merchant and to edit its Shop.
export function ShopForm({
  defaultValues = emptyShop,
  showDescription = false,
  submitLabel,
  onSubmit,
  onCancel,
}: {
  defaultValues?: ShopInput;
  showDescription?: boolean;
  submitLabel: string;
  onSubmit: (input: ShopInput) => Promise<void>;
  onCancel?: () => void;
}) {
  const form = useForm({
    defaultValues,
    validators: {
      onChange: shopSchema,
      onSubmitAsync: async ({ value }) => {
        try {
          await onSubmit(shopSchema.parse(value));
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
        <form.Field name="shopName">{(field) => <TextField field={field} label="Shop name" />}</form.Field>

        {showDescription && (
          <form.Field name="description">
            {(field) => <TextField field={field} label="Description" multiline />}
          </form.Field>
        )}

        <FieldSet>
          <FieldLegend>Pickup Address</FieldLegend>
          <form.Field name="pickupAddress.street">
            {(field) => <TextField field={field} label="Street" />}
          </form.Field>
          <form.Field name="pickupAddress.ward">{(field) => <TextField field={field} label="Ward" />}</form.Field>
          <form.Field name="pickupAddress.district">
            {(field) => <TextField field={field} label="District" />}
          </form.Field>
          <form.Field name="pickupAddress.province">
            {(field) => <TextField field={field} label="Province" />}
          </form.Field>
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
              {onCancel && (
                <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
                  Cancel
                </Button>
              )}
            </Field>
          )}
        </form.Subscribe>
      </FieldGroup>
    </form>
  );
}
