import { queryOptions } from "@tanstack/react-query";
import { z } from "zod";
import { apiFetch } from "@/lib/api";

export type Product = {
  id: string;
  name: string;
  price: number;
  createdAt: string;
};

export const productsQueryOptions = queryOptions({
  queryKey: ["products"],
  queryFn: () => apiFetch<Product[]>("/products"),
});

// Mirrors CreateProductValidator in the API; the API stays the source of truth.
export const createProductSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required")
    .max(200, "Name must be 200 characters or fewer"),
  price: z.number({ error: "Price is required" }).min(0, "Price can't be negative"),
});

export type CreateProductInput = z.infer<typeof createProductSchema>;

export function createProduct(input: CreateProductInput) {
  return apiFetch<Product>("/products", {
    method: "POST",
    body: JSON.stringify(input),
  });
}
