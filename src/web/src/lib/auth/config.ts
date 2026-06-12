import NextAuth, { type NextAuthConfig } from "next-auth";
import Keycloak from "next-auth/providers/keycloak";

// Framework-only auth configuration. No React imports here — this module is
// consumed by route handlers, server actions, the proxy, and Server
// Components. Auth.js v5 infers the Keycloak provider's clientId/clientSecret/
// issuer from the `AUTH_KEYCLOAK_ID`/`AUTH_KEYCLOAK_SECRET`/`AUTH_KEYCLOAK_ISSUER`
// env vars, and reads `AUTH_SECRET` for cookie encryption.

/** Refresh ~60s before the access token actually expires to avoid mid-request 401s. */
const REFRESH_SKEW_MS = 60_000;

/**
 * Decode the top-level `roles` claim from a Keycloak access token. The realm's
 * `oidc-usermodel-realm-role-mapper` flattens realm roles into a top-level
 * `roles` array on the access token (see the realm import). We decode the JWT
 * payload once on sign-in rather than parsing nested `realm_access.roles`.
 */
function rolesFromAccessToken(accessToken: string | undefined): string[] {
  if (!accessToken) return [];
  try {
    const payload = accessToken.split(".")[1];
    if (!payload) return [];
    const json = Buffer.from(payload, "base64").toString("utf8");
    const claims = JSON.parse(json) as { roles?: unknown };
    return Array.isArray(claims.roles)
      ? claims.roles.filter((r): r is string => typeof r === "string")
      : [];
  } catch {
    return [];
  }
}

export const authConfig = {
  providers: [Keycloak],
  session: { strategy: "jwt" },
  callbacks: {
    async jwt({ token, account }) {
      // 1. Initial sign-in: persist tokens + decode roles once.
      if (account) {
        return {
          ...token,
          access_token: account.access_token,
          expires_at: account.expires_at,
          refresh_token: account.refresh_token,
          roles: rolesFromAccessToken(account.access_token),
        };
      }

      // 2. Subsequent calls while the access token is still fresh.
      if (
        typeof token.expires_at === "number" &&
        Date.now() < token.expires_at * 1000 - REFRESH_SKEW_MS
      ) {
        return token;
      }

      // 3. Access token expired (or within the skew window): rotate it.
      if (!token.refresh_token) {
        return { ...token, error: "RefreshTokenError" as const };
      }

      try {
        const response = await fetch(
          `${process.env.AUTH_KEYCLOAK_ISSUER}/protocol/openid-connect/token`,
          {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({
              client_id: process.env.AUTH_KEYCLOAK_ID ?? "",
              client_secret: process.env.AUTH_KEYCLOAK_SECRET ?? "",
              grant_type: "refresh_token",
              refresh_token: token.refresh_token,
            }),
          },
        );

        const tokensOrError = await response.json();
        if (!response.ok) throw tokensOrError;

        const newTokens = tokensOrError as {
          access_token: string;
          expires_in: number;
          refresh_token?: string;
        };

        return {
          ...token,
          access_token: newTokens.access_token,
          expires_at: Math.floor(Date.now() / 1000 + newTokens.expires_in),
          // Keycloak rotates refresh tokens, but preserve the old one if a new
          // one isn't returned.
          refresh_token: newTokens.refresh_token ?? token.refresh_token,
          roles: rolesFromAccessToken(newTokens.access_token),
          error: undefined,
        };
      } catch (error) {
        console.error("Error refreshing access_token", error);
        return { ...token, error: "RefreshTokenError" as const };
      }
    },
    async session({ session, token }) {
      session.accessToken = token.access_token;
      session.roles = Array.isArray(token.roles) ? token.roles : [];
      session.error = token.error;
      return session;
    },
    authorized({ auth }) {
      // Defense-in-depth gate consulted by the proxy wrapper; real enforcement
      // lives in server actions + the Catalog API.
      return !!auth;
    },
  },
} satisfies NextAuthConfig;

export const { handlers, auth, signIn, signOut } = NextAuth(authConfig);

declare module "next-auth" {
  interface Session {
    accessToken?: string;
    roles?: string[];
    error?: "RefreshTokenError";
  }
}

declare module "@auth/core/jwt" {
  interface JWT {
    access_token?: string;
    expires_at?: number;
    refresh_token?: string;
    roles?: string[];
    error?: "RefreshTokenError";
  }
}
