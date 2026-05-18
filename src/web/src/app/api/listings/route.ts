import { NextResponse } from "next/server";
import { fetchProducts } from "@/lib/catalog/api";

const DEFAULT_LIMIT = 9;

function toPositiveInt(raw: string | null, fallback: number): number {
  const n = Number(raw ?? "");
  if (!Number.isFinite(n) || n < 1) return fallback;
  return Math.floor(n);
}

export async function GET(req: Request) {
  const url = new URL(req.url);
  const page = toPositiveInt(url.searchParams.get("page"), 1);
  const limit = toPositiveInt(url.searchParams.get("limit"), DEFAULT_LIMIT);
  const data = await fetchProducts({ page, limit });
  return NextResponse.json(data);
}
