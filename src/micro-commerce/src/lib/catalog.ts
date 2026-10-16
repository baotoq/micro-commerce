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
