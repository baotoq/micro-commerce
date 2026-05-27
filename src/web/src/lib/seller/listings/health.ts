// Pure scoring of a listing draft. No I/O, no async. Mirrors PRD §3.
// Accepts either string-based form values (RHF holds raw inputs) or
// already-coerced numbers — used by both the listing-health card and unit tests.

export type ListingHealthInput = {
  name?: string;
  description?: string | null;
  category?: string;
  price?: string | number;
  inventory?: string | number;
  weight?: string | number;
  origin?: string;
  tags?: readonly string[];
  photoUrls?: readonly string[];
};

export type ListingHealth = {
  score: number;
  reasons: string[];
};

const ORIGIN_REGEX = /^[A-Za-z .'-]{2,40}, [A-Z]{2}$/;

function toNumber(value: string | number | undefined | null): number {
  if (typeof value === "number") return Number.isFinite(value) ? value : 0;
  if (value == null) return 0;
  const trimmed = value.trim();
  if (trimmed === "") return 0;
  const n = Number(trimmed);
  return Number.isFinite(n) ? n : 0;
}

export function computeListingHealth(
  values: ListingHealthInput,
): ListingHealth {
  let score = 0;
  const reasons: string[] = [];

  const name = (values.name ?? "").trim();
  if (name.length >= 3) score += 15;
  else reasons.push("Add a longer name");

  const description = (values.description ?? "").trim();
  if (description.length >= 100) score += 15;
  else reasons.push("Add a description over 100 chars");

  const photos = values.photoUrls ?? [];
  if (photos.length >= 3) {
    score += 20;
  } else {
    score += photos.length * 5;
    reasons.push("Add 3+ photos");
  }

  const tags = values.tags ?? [];
  if (tags.length >= 3) score += 10;
  else reasons.push("Add 3+ tags");

  if (toNumber(values.price) > 0) score += 10;
  else reasons.push("Set a price");

  if (toNumber(values.inventory) >= 1) score += 10;
  else reasons.push("Add inventory");

  if ((values.category ?? "").trim().length > 0) score += 10;
  else reasons.push("Choose a category");

  if (toNumber(values.weight) > 0) score += 5;
  else reasons.push("Set shipping weight");

  if (ORIGIN_REGEX.test((values.origin ?? "").trim())) score += 5;
  else reasons.push("Add origin (City, ST)");

  if (score < 0) score = 0;
  if (score > 100) score = 100;

  return { score, reasons };
}
