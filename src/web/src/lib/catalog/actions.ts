"use server";

import { updateTag } from "next/cache";
import type { Listing, ListingStatus } from "@/lib/seller/listings/types";
import { createProduct, deleteProduct, updateProduct } from "./api";

const STATUSES: ListingStatus[] = ["active", "low", "out", "draft"];

function parseStatus(raw: FormDataEntryValue | null): ListingStatus {
  const v = String(raw ?? "").toLowerCase() as ListingStatus;
  if (!STATUSES.includes(v)) throw new Error(`Invalid status: ${raw}`);
  return v;
}

function parseRequired(raw: FormDataEntryValue | null, field: string): string {
  const v = String(raw ?? "").trim();
  if (!v) throw new Error(`${field} is required.`);
  return v;
}

function parseNumber(
  raw: FormDataEntryValue | null,
  field: string,
  min: number,
): number {
  const n = Number(raw);
  if (!Number.isFinite(n) || n < min)
    throw new Error(`${field} must be >= ${min}.`);
  return n;
}

export async function createProductAction(
  formData: FormData,
): Promise<Listing> {
  // TODO(auth): requireSeller() — see audit/auth-followup.md
  const input = {
    sku: parseRequired(formData.get("sku"), "SKU"),
    name: parseRequired(formData.get("name"), "Name"),
    category: parseRequired(formData.get("category"), "Category"),
    price: parseNumber(formData.get("price"), "Price", 0),
    inventory: parseNumber(formData.get("inventory"), "Inventory", 0),
    status: parseStatus(formData.get("status")),
  };
  const created = await createProduct(input);
  updateTag("listings");
  return created;
}

export async function updateProductAction(
  sku: string,
  formData: FormData,
): Promise<Listing | null> {
  // TODO(auth): requireSeller() — see audit/auth-followup.md
  const input = {
    name: parseRequired(formData.get("name"), "Name"),
    category: parseRequired(formData.get("category"), "Category"),
    price: parseNumber(formData.get("price"), "Price", 0),
    inventory: parseNumber(formData.get("inventory"), "Inventory", 0),
    status: parseStatus(formData.get("status")),
  };
  const updated = await updateProduct(sku, input);
  updateTag("listings");
  return updated;
}

export async function deleteProductAction(sku: string): Promise<boolean> {
  // TODO(auth): requireSeller() — see audit/auth-followup.md
  const deleted = await deleteProduct(sku);
  updateTag("listings");
  return deleted;
}
