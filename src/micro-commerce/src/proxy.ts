import { NextResponse } from "next/server";
import { auth } from "@/auth";

// Runs on every page and API request so Auth.js can persist a refreshed session cookie.
// Forwards /api/* (except Auth.js's own /api/auth/*) to the backend, attaching the signed-in
// Account's access token. Done here rather than in next.config.ts rewrites because those are fixed
// at build time, while API_URL is only known at runtime (Aspire / container env).
export const proxy = auth((request) => {
  const { pathname, search } = request.nextUrl;
  if (!pathname.startsWith("/api/") || pathname.startsWith("/api/auth/")) return NextResponse.next();

  const apiUrl = process.env.API_URL;
  if (!apiUrl) {
    return NextResponse.json({ title: "API_URL is not configured" }, { status: 500 });
  }

  const headers = new Headers(request.headers);
  headers.delete("authorization");
  const accessToken = request.auth?.accessToken;
  if (accessToken) headers.set("authorization", `Bearer ${accessToken}`);

  return NextResponse.rewrite(new URL(pathname.replace(/^\/api/, "") + search, apiUrl), {
    request: { headers },
  });
});

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
};
