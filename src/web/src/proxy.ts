import { NextResponse } from "next/server";
import { auth } from "@/lib/auth/config";

// Next 16 Proxy (replaces the deprecated `middleware.ts`). This is
// defense-in-depth only — the authoritative enforcement is `requireSeller()`
// in server actions and the JWT-bearer policy on the Catalog API. Here we just
// bounce unauthenticated visitors off the `/seller/*` UI toward the sign-in
// flow so they never see a half-rendered dashboard.
export const proxy = auth((req) => {
  if (!req.auth && req.nextUrl.pathname.startsWith("/seller")) {
    const signInUrl = new URL("/api/auth/signin", req.nextUrl);
    signInUrl.searchParams.set(
      "callbackUrl",
      `${req.nextUrl.pathname}${req.nextUrl.search}`,
    );
    return NextResponse.redirect(signInUrl);
  }
  return NextResponse.next();
});

export const config = {
  matcher: ["/seller/:path*"],
};
