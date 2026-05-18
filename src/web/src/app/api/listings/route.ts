import { NextResponse } from "next/server";
import { fetchProducts, type ProductQuery } from "@/lib/catalog/api";
import type { ListingStatus } from "@/lib/seller/types";

const DEFAULT_LIMIT = 9;
const STATUS_VALUES: readonly ListingStatus[] = [
  "active",
  "low",
  "out",
  "draft",
];

function toPositiveInt(raw: string | null, fallback: number): number {
  const n = Number(raw ?? "");
  if (!Number.isFinite(n) || n < 1) return fallback;
  return Math.floor(n);
}

function parseStatus(raw: string | null): ListingStatus | undefined {
  return STATUS_VALUES.find((s) => s === raw);
}

export async function GET(req: Request) {
  const url = new URL(req.url);
  const page = toPositiveInt(url.searchParams.get("page"), 1);
  const limit = toPositiveInt(url.searchParams.get("limit"), DEFAULT_LIMIT);
  const status = parseStatus(url.searchParams.get("status"));
  const query: ProductQuery = { page, limit };
  if (status) query.status = status;
  const data = await fetchProducts(query);
  return NextResponse.json(data);
}
