import { auth } from "@/auth";

export default auth((req) => {
  // Add any route protection logic here
  // For example, to protect all routes under /dashboard:
  // if (!req.auth && req.nextUrl.pathname.startsWith("/dashboard")) {
  //   const newUrl = new URL("/api/auth/signin", req.nextUrl.origin);
  //   return Response.redirect(newUrl);
  // }
});

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico).*)"],
};

