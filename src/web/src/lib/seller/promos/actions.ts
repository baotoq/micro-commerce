"use server";

import { updateTag } from "next/cache";
import {
  activatePromotion,
  createPromotion,
  deletePromotion,
  endPromotion,
  updatePromotion,
} from "@/lib/catalog/promotions";
import { promotionInputSchema } from "./schema";

export type ActionResult =
  | { ok: true; sku: string }
  | {
      ok: false;
      error: string;
      fieldErrors?: Record<string, string[]>;
      values?: Record<string, string>;
    };

function rawFormValues(formData: FormData): Record<string, string> {
  const values: Record<string, string> = {};
  for (const [key, value] of formData.entries()) {
    if (typeof value === "string") values[key] = value;
  }
  return values;
}

function formDataToInput(formData: FormData): Record<string, unknown> {
  const out: Record<string, unknown> = {};
  for (const key of new Set(Array.from(formData.keys()))) {
    const values = formData.getAll(key).filter((v) => typeof v === "string");
    if (values.length > 0) {
      out[key] = values[0];
    }
  }
  return out;
}

export async function createPromoAction(
  formData: FormData,
): Promise<ActionResult> {
  const parsed = promotionInputSchema.safeParse(formDataToInput(formData));
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
    const promo = await createPromotion(parsed.data);
    updateTag("promotions");
    return { ok: true, sku: promo.code };
  } catch (err) {
    const message =
      err instanceof Error ? err.message : "Failed to create promotion";
    if (message === "PROMOTION_CODE_DUPLICATE") {
      return { ok: false, error: `A promotion with that code already exists.` };
    }
    return { ok: false, error: message };
  }
}

export async function updatePromoAction(
  code: string,
  formData: FormData,
): Promise<ActionResult> {
  const parsed = promotionInputSchema.safeParse(formDataToInput(formData));
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

  const { activateImmediately: _activate, ...rest } = parsed.data;
  try {
    const promo = await updatePromotion(code, {
      ...rest,
      code,
      activateImmediately: false,
    });
    if (promo === null) {
      return { ok: false, error: "Promotion not found." };
    }
    updateTag("promotions");
    return { ok: true, sku: promo.code };
  } catch (err) {
    const message =
      err instanceof Error ? err.message : "Failed to update promotion";
    return { ok: false, error: message };
  }
}

export async function activatePromoAction(code: string): Promise<ActionResult> {
  try {
    const promo = await activatePromotion(code);
    if (promo === null) {
      return { ok: false, error: "Promotion not found." };
    }
    updateTag("promotions");
    return { ok: true, sku: promo.code };
  } catch (err) {
    const message =
      err instanceof Error ? err.message : "Failed to activate promotion";
    return { ok: false, error: message };
  }
}

export async function endPromoAction(code: string): Promise<ActionResult> {
  try {
    const promo = await endPromotion(code);
    if (promo === null) {
      return { ok: false, error: "Promotion not found." };
    }
    updateTag("promotions");
    return { ok: true, sku: promo.code };
  } catch (err) {
    const message =
      err instanceof Error ? err.message : "Failed to end promotion";
    return { ok: false, error: message };
  }
}

export async function deletePromoAction(code: string): Promise<ActionResult> {
  try {
    const deleted = await deletePromotion(code);
    if (!deleted) {
      return { ok: false, error: "Promotion not found." };
    }
    updateTag("promotions");
    return { ok: true, sku: code };
  } catch (err) {
    const message =
      err instanceof Error ? err.message : "Failed to delete promotion";
    return { ok: false, error: message };
  }
}

// `<form action>` requires a void-returning server action. These thin wrappers
// adapt the row actions (which return ActionResult) for direct form binding via
// `.bind(null, code)`, so submitting gets proper server-action revalidation
// (a plain client closure calling the action did not refresh the table).
export async function activatePromoFormAction(code: string): Promise<void> {
  await activatePromoAction(code);
}

export async function endPromoFormAction(code: string): Promise<void> {
  await endPromoAction(code);
}

export async function deletePromoFormAction(code: string): Promise<void> {
  await deletePromoAction(code);
}
