import { z } from "zod";
import { apiFetch } from "@/lib/api";

// Mirrors CategoryResponse in the API.
export type Category = {
  id: string;
  name: string;
  parentId: string | null;
};

// Mirrors CategoryNodeResponse in the API: a Category and its descendants, children ordered by name.
export type CategoryNode = {
  id: string;
  name: string;
  children: CategoryNode[];
};

// Anonymous, so it works from the server and the browser alike.
export function getCategoryTree() {
  return apiFetch<CategoryNode[]>("/storefront/categories");
}

// Mirrors CreateCategoryValidator / RenameCategoryValidator in the API; the API stays the source of truth.
export const categoryNameSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required")
    .max(100, "Name must be 100 characters or fewer"),
});

export type CategoryNameInput = z.infer<typeof categoryNameSchema>;

// Browser-side, through the /api proxy, as the signed-in Platform Operator.
export function createCategory({ name }: CategoryNameInput, parentId: string | null = null) {
  return apiFetch<Category>("/ops/categories", {
    method: "POST",
    body: JSON.stringify({ name, parentId }),
  });
}

export function renameCategory(id: string, { name }: CategoryNameInput) {
  return apiFetch<Category>(`/ops/categories/${id}`, {
    method: "PUT",
    body: JSON.stringify({ name }),
  });
}

export function deleteCategory(id: string) {
  return apiFetch<void>(`/ops/categories/${id}`, { method: "DELETE" });
}

// Every Category without children, with its path from the top ("Fashion › Men › T-Shirts"), in tree order.
// Products can only be listed in one of these.
export function leafCategories(tree: CategoryNode[], parents: string[] = []): { id: string; path: string }[] {
  return tree.flatMap((node) =>
    node.children.length === 0
      ? [{ id: node.id, path: [...parents, node.name].join(" › ") }]
      : leafCategories(node.children, [...parents, node.name]),
  );
}

// Mirrors ProductStatus in the API.
export type ProductStatus = "Draft" | "Published";

// Mirrors VariantResponse in the API; price is whole VND, weight is in grams.
export type Variant = {
  id: string;
  price: number;
  stock: number;
  weightGrams: number;
};

// Mirrors ProductResponse in the API: a Product as its Merchant sees it.
export type Product = {
  id: string;
  name: string;
  description: string | null;
  categoryId: string;
  status: ProductStatus;
  variants: Variant[];
  createdAt: string;
};

// Mirrors ProductSummaryResponse in the API: a row of the Merchant's Product list.
export type ProductSummary = {
  id: string;
  name: string;
  categoryId: string;
  status: ProductStatus;
  variantCount: number;
  minPrice: number;
  maxPrice: number;
  totalStock: number;
  createdAt: string;
};

const vnd = new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" });

export function formatPrice(amount: number) {
  return vnd.format(amount);
}

// Server-side: the caller passes the session's access token.
export function listMerchantProducts(accessToken: string) {
  return apiFetch<ProductSummary[]>("/merchant/products", {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}

export function getMerchantProduct(accessToken: string, id: string) {
  return apiFetch<Product>(`/merchant/products/${encodeURIComponent(id)}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}

const wholeNumber = (label: string) =>
  z.number({ error: `${label} is required` }).int(`${label} must be a whole number`);

// Mirrors ProductDetailsValidator in the API; the API stays the source of truth.
export const productSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required")
    .max(200, "Name must be 200 characters or fewer"),
  description: z.string().trim().max(5000, "Description must be 5000 characters or fewer"),
  categoryId: z.string().min(1, "Category is required"),
  price: wholeNumber("Price").positive("Price must be more than 0"),
  stock: wholeNumber("Stock").min(0, "Stock can't be negative"),
  weightGrams: wholeNumber("Weight").positive("Weight must be more than 0"),
});

export type ProductInput = z.infer<typeof productSchema>;

const productBody = ({ description, ...input }: ProductInput) =>
  JSON.stringify({ ...input, description: description || null });

// Browser-side, through the /api proxy, as the signed-in Merchant.
export function createProduct(input: ProductInput) {
  return apiFetch<Product>("/merchant/products", { method: "POST", body: productBody(input) });
}

export function updateProduct(id: string, input: ProductInput) {
  return apiFetch<Product>(`/merchant/products/${id}`, { method: "PUT", body: productBody(input) });
}

export function publishProduct(id: string) {
  return apiFetch<Product>(`/merchant/products/${id}/publish`, { method: "POST" });
}

export function unpublishProduct(id: string) {
  return apiFetch<Product>(`/merchant/products/${id}/unpublish`, { method: "POST" });
}
