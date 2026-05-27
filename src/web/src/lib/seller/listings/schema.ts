import { z } from "zod";

const TAG_REGEX = /^[a-z0-9-]{2,24}$/;
const ORIGIN_REGEX = /^[A-Za-z .'-]{2,40}, [A-Z]{2}$/;

// Common shapes broken out for reuse in the per-step schemas. They mirror the
// fields each step controls in the wizard (see PRD §1). The whole-form schema
// then combines them and adds the cross-field "active requires inventory ≥ 1
// AND photoUrls.length ≥ 1" gate.

const skuField = z
  .string()
  .trim()
  .min(1, "SKU is required")
  .max(64)
  .transform((v) => v.toUpperCase());

const nameField = z.string().trim().min(1, "Name is required").max(120);

const categoryField = z.string().trim().min(1, "Category is required").max(80);

// description is optional (max 2000) but always trimmed
const descriptionField = z.string().trim().max(2000).optional().default("");

const priceField = z
  .string()
  .trim()
  .min(1, "Price is required")
  .transform((v) => Number(v))
  .refine((n) => !Number.isNaN(n), "Price must be a number")
  .refine((n) => n >= 0, "Price must be non-negative");

const inventoryField = z
  .string()
  .trim()
  .min(1, "Inventory is required")
  .transform((v) => Number(v))
  .refine((n) => !Number.isNaN(n), "Inventory must be a number")
  .refine((n) => Number.isInteger(n), "Inventory must be an integer")
  .refine((n) => n >= 0, "Inventory must be non-negative");

const statusField = z.enum(["active", "low", "out", "draft"]);

const weightField = z
  .string()
  .trim()
  .min(1, "Weight is required")
  .transform((v) => Number(v))
  .refine((n) => !Number.isNaN(n), "Weight must be a number")
  .refine((n) => n > 0, "Weight must be greater than 0")
  .refine((n) => n <= 100, "Weight must be 100 kg or less")
  // precision(6,2) — round-trip through a 2-decimal string to enforce.
  .refine(
    (n) => Math.round(n * 100) / 100 === n,
    "Weight must have at most 2 decimal places",
  );

const originField = z
  .string()
  .trim()
  .min(1, "Origin is required")
  .regex(ORIGIN_REGEX, "Origin must be 'City, ST' (e.g. Portland, OR)");

const tagsField = z
  .array(z.string().regex(TAG_REGEX, "Tags must be lowercase kebab, 2–24 chars"))
  .max(8, "Up to 8 tags")
  // Dedupe in transform — UX-friendlier than rejecting a duplicate. The order
  // of first occurrence is preserved so the user's intent stays intact.
  .transform((arr) => Array.from(new Set(arr)))
  .optional()
  .default([]);

const photoUrlsField = z
  .array(z.string().min(1))
  .max(6, "Up to 6 photos")
  .optional()
  .default([]);

// Per-step schemas: each owns the fields the wizard step exposes. Used by
// validateStep() to gate the "Next" button per PRD §1.

export const step1Schema = z.object({
  sku: skuField,
  name: nameField,
  category: categoryField,
  description: descriptionField,
});

export const step2Schema = z.object({
  price: priceField,
  inventory: inventoryField,
  status: statusField,
  weight: weightField,
  origin: originField,
});

export const step3Schema = z.object({
  photoUrls: photoUrlsField,
  tags: tagsField,
});

// Whole-form schema. The cross-field gate from PRD §2 / AC-12 is enforced via
// superRefine so the error targets the `status` path (the wizard surfaces it
// on step 2 next to the status select).
export const productInputSchema = z
  .object({
    sku: skuField,
    name: nameField,
    category: categoryField,
    description: descriptionField,
    price: priceField,
    inventory: inventoryField,
    status: statusField,
    weight: weightField,
    origin: originField,
    tags: tagsField,
    photoUrls: photoUrlsField,
  })
  .superRefine((values, ctx) => {
    if (values.status === "active") {
      if (values.inventory < 1) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["status"],
          message: "Active listings need at least 1 in inventory",
        });
      }
      if (values.photoUrls.length < 1) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["status"],
          message: "Active listings need at least 1 photo",
        });
      }
    }
  });

export type ProductFormInput = z.input<typeof productInputSchema>;
export type ProductFormOutput = z.output<typeof productInputSchema>;

export type WizardStep = 1 | 2 | 3;

// Per-step validator the wizard "Next" button consults. Returning `ok` plus
// the keyed step schema's parse result keeps the call site terse and lets the
// caller surface field-level errors on the active step if it wants.
export function validateStep(
  step: WizardStep,
  values: Record<string, unknown>,
): { ok: boolean } {
  const schema =
    step === 1 ? step1Schema : step === 2 ? step2Schema : step3Schema;
  // pick the fields this step owns out of the form values so unrelated fields
  // (e.g. step 3 stuff while gating step 1) don't trigger false negatives.
  const keys = Object.keys(schema.shape);
  const subset: Record<string, unknown> = {};
  for (const k of keys) subset[k] = values[k];
  return { ok: schema.safeParse(subset).success };
}
