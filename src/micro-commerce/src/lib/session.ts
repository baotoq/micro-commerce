import { notFound, redirect } from "next/navigation";
import { cache } from "react";
import { auth } from "@/auth";
import { getMe, type Me, type Merchant } from "@/lib/accounts";

// Server-only: the signed-in Account's access token, for server components that call the API as it.
export const getAccessToken = cache(async (): Promise<string | null> => {
  const session = await auth();
  return session?.accessToken && !session.error ? session.accessToken : null;
});

// Server-only. Cached per request, so the header and the page share one /me call.
export const getCurrentAccount = cache(async (): Promise<Me | null> => {
  const accessToken = await getAccessToken();
  if (!accessToken) return null;
  // An expired or rejected token just means "not signed in" here; signing in again fixes it.
  return getMe(accessToken).catch(() => null);
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

// The Merchant Centre guard, plus the access token its pages call /merchant/* with.
export async function requireMerchantSession(): Promise<{ merchant: Merchant; accessToken: string }> {
  const merchant = await requireMerchant();
  // requireMerchant only lets a signed-in Account through, so there is a token.
  return { merchant, accessToken: (await getAccessToken())! };
}

// Guard for operator pages (/ops): they only render for a Platform Operator; anyone else gets a 404.
// It's for navigation only; the API refuses /ops/* requests from anyone else regardless.
export async function requirePlatformOperator(): Promise<Me> {
  const me = await getCurrentAccount();
  if (!me) redirect(signInUrl("/ops"));
  if (!me.isPlatformOperator) notFound();
  return me;
}
