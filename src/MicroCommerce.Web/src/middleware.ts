import { auth } from "@/auth";

export default auth((req) => {
  // Protect account routes - require authentication
  if (!req.auth && req.nextUrl.pathname.startsWith("/account")) {
    const signInUrl = new URL("/api/auth/signin", req.nextUrl.origin);
    signInUrl.searchParams.set("callbackUrl", req.nextUrl.pathname);
    return Response.redirect(signInUrl);
  }
});

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico).*)"],
};

