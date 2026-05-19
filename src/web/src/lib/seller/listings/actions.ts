"use server";

import { revalidatePath } from "next/cache";
import { createProduct, deleteProduct, updateProduct } from "@/lib/catalog/api";
import { productInputSchema } from "./schema";

export type ActionResult =
  | { ok: true; sku: string }
  | {
      ok: false;
      error: string;
      fieldErrors?: Record<string, string[]>;
      values?: Record<string, string>;
    };

// `<form action={serverAction}>` resets uncontrolled inputs after every submit,
// so on validation failure we round-trip the raw entries back to the client and
// let the form re-seed `defaultValue` from them — otherwise the user's typed
// fields go blank when any single field fails validation.
function rawFormValues(formData: FormData): Record<string, string> {
  const values: Record<string, string> = {};
  for (const [key, value] of formData.entries()) {
    if (typeof value === "string") values[key] = value;
  }
  return values;
}

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
    return {
      ok: false,
      error: "Invalid input",
      fieldErrors,
      values: rawFormValues(formData),
    };
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
    return {
      ok: false,
      error: "Invalid input",
      fieldErrors,
      values: rawFormValues(formData),
    };
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
