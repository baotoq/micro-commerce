import "server-only";
import { forbidden, redirect } from "next/navigation";
import { auth } from "@/lib/auth/config";

const SIGN_IN_PATH = "/api/auth/signin";

/**
 * Resolve the Keycloak access token for server-side API writes. Redirects to
 * the sign-in flow when the caller is unauthenticated, when the session has no
 * access token, or when refresh-token rotation has failed
 * (`session.error === "RefreshTokenError"`). Never returns an empty string.
 */
export async function getAccessToken(): Promise<string> {
  const session = await auth();
  if (
    !session ||
    !session.accessToken ||
    session.error === "RefreshTokenError"
  ) {
    redirect(SIGN_IN_PATH);
  }
  return session.accessToken;
}

/**
 * Guard for seller-only server actions and route handlers. Redirects
 * unauthenticated callers to sign-in and `forbidden()`s authenticated callers
 * who lack the `seller` role. Returns the validated session on success.
 */
export async function requireSeller() {
  const session = await auth();
  if (!session) {
    redirect(SIGN_IN_PATH);
  }
  if (!session.roles?.includes("seller")) {
    forbidden();
  }
  return session;
}
