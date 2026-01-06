import { getApiBaseUrl } from "@/lib/config";
import { NextResponse } from "next/server";

export async function GET() {
  return NextResponse.json({
    apiBaseUrl: getApiBaseUrl(),
  });
}

