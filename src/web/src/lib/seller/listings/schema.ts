import { z } from "zod";

export const productInputSchema = z.object({
  sku: z
    .string()
    .trim()
    .min(1, "SKU is required")
    .max(64)
    .transform((v) => v.toUpperCase()),
  name: z.string().trim().min(1, "Name is required").max(120),
  category: z.string().trim().min(1, "Category is required").max(80),
  // Check for empty *before* converting — `Number("")` is 0, which would
  // silently satisfy `>= 0` and hide the required-field state.
  price: z
    .string()
    .trim()
    .min(1, "Price is required")
    .transform((v) => Number(v))
    .refine((n) => !Number.isNaN(n), "Price must be a number")
    .refine((n) => n >= 0, "Price must be non-negative"),
  inventory: z
    .string()
    .trim()
    .min(1, "Inventory is required")
    .transform((v) => Number(v))
    .refine((n) => !Number.isNaN(n), "Inventory must be a number")
    .refine((n) => Number.isInteger(n), "Inventory must be an integer")
    .refine((n) => n >= 0, "Inventory must be non-negative"),
  status: z.enum(["active", "low", "out", "draft"]),
});

export type ProductFormInput = z.infer<typeof productInputSchema>;
