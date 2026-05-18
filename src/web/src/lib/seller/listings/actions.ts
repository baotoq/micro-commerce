"use server";

import { revalidatePath } from "next/cache";
import { createProduct, deleteProduct, updateProduct } from "@/lib/catalog/api";
import { productInputSchema } from "./schema";

export type ActionResult =
  | { ok: true; sku: string }
  | { ok: false; error: string; fieldErrors?: Record<string, string[]> };

export async function createListingAction(
  formData: FormData,
): Promise<ActionResult> {
  const parsed = productInputSchema.safeParse(
    Object.fromEntries(formData.entries()),
  );
  if (!parsed.success) {
    const fieldErrors: Record<string, string[]> = {};
    for (const [field, issues] of Object.entries(
      parsed.error.flatten().fieldErrors,
    )) {
      fieldErrors[field] = issues ?? [];
    }
    return { ok: false, error: "Invalid input", fieldErrors };
  }

  try {
    const listing = await createProduct(parsed.data);
    revalidatePath("/seller/listings");
    return { ok: true, sku: listing.sku };
  } catch (err) {
    const message =
      err instanceof Error ? err.message : "Failed to create listing";
    if (message.includes("already exists")) {
      return { ok: false, error: `A listing with that SKU already exists.` };
    }
    return { ok: false, error: message };
  }
}

export async function updateListingAction(
  sku: string,
  formData: FormData,
): Promise<ActionResult> {
  const parsed = productInputSchema.safeParse(
    Object.fromEntries(formData.entries()),
  );
  if (!parsed.success) {
    const fieldErrors: Record<string, string[]> = {};
    for (const [field, issues] of Object.entries(
      parsed.error.flatten().fieldErrors,
    )) {
      fieldErrors[field] = issues ?? [];
    }
    return { ok: false, error: "Invalid input", fieldErrors };
  }

  const { sku: _sku, ...rest } = parsed.data;
  try {
    const listing = await updateProduct(sku, rest);
    if (listing === null) {
      return { ok: false, error: "Listing not found." };
    }
    revalidatePath("/seller/listings");
    revalidatePath(`/seller/listings/${sku}/edit`);
    return { ok: true, sku: listing.sku };
  } catch (err) {
    const message =
      err instanceof Error ? err.message : "Failed to update listing";
    return { ok: false, error: message };
  }
}

export async function deleteListingAction(sku: string): Promise<ActionResult> {
  try {
    const deleted = await deleteProduct(sku);
    if (!deleted) {
      return { ok: false, error: "Listing not found." };
    }
    revalidatePath("/seller/listings");
    revalidatePath(`/seller/listings/${sku}/edit`);
    return { ok: true, sku };
  } catch (err) {
    const message =
      err instanceof Error ? err.message : "Failed to delete listing";
    return { ok: false, error: message };
  }
}
