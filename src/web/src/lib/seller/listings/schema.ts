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
  price: z.coerce
    .number({ error: "Price must be a number" })
    .min(0, "Price must be non-negative"),
  inventory: z.coerce
    .number({ error: "Inventory must be a number" })
    .int("Inventory must be an integer")
    .min(0, "Inventory must be non-negative"),
  status: z.enum(["active", "low", "out", "draft"]),
});

export type ProductFormInput = z.infer<typeof productInputSchema>;
