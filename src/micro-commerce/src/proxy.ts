import { NextResponse, type NextRequest } from "next/server";

// Forwards /api/* to the backend. Done here rather than in next.config.ts rewrites because
// those are fixed at build time, while API_URL is only known at runtime (Aspire / container env).
export function proxy(request: NextRequest) {
  const apiUrl = process.env.API_URL;
  if (!apiUrl) {
    return NextResponse.json({ title: "API_URL is not configured" }, { status: 500 });
  }

  const { pathname, search } = request.nextUrl;
  return NextResponse.rewrite(new URL(pathname.replace(/^\/api/, "") + search, apiUrl));
}

export const config = {
  matcher: "/api/:path*",
};
