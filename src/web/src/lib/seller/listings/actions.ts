"use server";

import { updateTag } from "next/cache";
import { createProduct, deleteProduct, updateProduct } from "@/lib/catalog/api";
import { productInputSchema, productUpdateSchema } from "./schema";

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

// FormData stores multiple values per key for arrays (tags, photoUrls). We
// reshape into a plain object the Zod schema can parse: scalars stay scalar,
// known array keys come back as arrays.
const ARRAY_KEYS = new Set(["tags", "photoUrls"]);

function formDataToInput(formData: FormData): Record<string, unknown> {
  const out: Record<string, unknown> = {};
  for (const key of new Set(Array.from(formData.keys()))) {
    const values = formData.getAll(key).filter((v) => typeof v === "string");
    if (ARRAY_KEYS.has(key)) {
      out[key] = values as string[];
    } else if (values.length > 0) {
      out[key] = values[0];
    }
  }
  return out;
}

export async function createListingAction(
  formData: FormData,
): Promise<ActionResult> {
  // TODO(auth): requireSeller() — see audit/auth-followup.md
  const parsed = productInputSchema.safeParse(formDataToInput(formData));
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
    updateTag("listings");
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
  // TODO(auth): requireSeller() — see audit/auth-followup.md
  // The legacy edit form doesn't render weight/origin — productUpdateSchema
  // keeps those optional with backend-aligned defaults so existing edits don't
  // start failing now that the create wizard has expanded the field set.
  const parsed = productUpdateSchema.safeParse(formDataToInput(formData));
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
    updateTag("listings");
    return { ok: true, sku: listing.sku };
  } catch (err) {
    const message =
      err instanceof Error ? err.message : "Failed to update listing";
    return { ok: false, error: message };
  }
}

export async function deleteListingAction(sku: string): Promise<ActionResult> {
  // TODO(auth): requireSeller() — see audit/auth-followup.md
  try {
    const deleted = await deleteProduct(sku);
    if (!deleted) {
      return { ok: false, error: "Listing not found." };
    }
    updateTag("listings");
    return { ok: true, sku };
  } catch (err) {
    const message =
      err instanceof Error ? err.message : "Failed to delete listing";
    return { ok: false, error: message };
  }
}
