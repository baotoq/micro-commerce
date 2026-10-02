import { redirect } from "next/navigation";
import { cache } from "react";
import { auth } from "@/auth";
import { getMe, type Me, type Merchant } from "@/lib/accounts";

// Server-only. Cached per request, so the header and the page share one /me call.
export const getCurrentAccount = cache(async (): Promise<Me | null> => {
  const session = await auth();
  if (!session?.accessToken || session.error) return null;
  // An expired or rejected token just means "not signed in" here; signing in again fixes it.
  return getMe(session.accessToken).catch(() => null);
});

export function signInUrl(callbackUrl: string) {
  return `/api/auth/signin?callbackUrl=${encodeURIComponent(callbackUrl)}`;
}

// Guard for Merchant Centre pages: they only render for an Account that owns a Merchant.
// It's for navigation only; the API refuses /merchant/* requests from anyone else regardless.
export async function requireMerchant(): Promise<Merchant> {
  const me = await getCurrentAccount();
  if (!me) redirect(signInUrl("/merchant"));
  if (!me.merchant) redirect("/open-shop");
  return me.merchant;
}
