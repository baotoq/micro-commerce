import NextAuth from "next-auth";
import type { JWT } from "next-auth/jwt";
import Keycloak from "next-auth/providers/keycloak";

// Keycloak reads AUTH_KEYCLOAK_ID, AUTH_KEYCLOAK_SECRET and AUTH_KEYCLOAK_ISSUER (set by the AppHost).
const oidcEndpoint = (name: "token" | "logout") =>
  `${process.env.AUTH_KEYCLOAK_ISSUER}/protocol/openid-connect/${name}`;

const clientCredentials = () => ({
  client_id: process.env.AUTH_KEYCLOAK_ID!,
  client_secret: process.env.AUTH_KEYCLOAK_SECRET!,
});

// Keycloak access tokens are short-lived; trade the refresh token for a new one shortly before expiry.
async function refreshAccessToken(token: JWT): Promise<JWT> {
  try {
    const response = await fetch(oidcEndpoint("token"), {
      method: "POST",
      body: new URLSearchParams({
        ...clientCredentials(),
        grant_type: "refresh_token",
        refresh_token: token.refreshToken!,
      }),
    });
    const tokens = await response.json();
    if (!response.ok) throw tokens;

    return {
      ...token,
      accessToken: tokens.access_token,
      expiresAt: Math.floor(Date.now() / 1000) + tokens.expires_in,
      refreshToken: tokens.refresh_token ?? token.refreshToken,
      error: undefined,
    };
  } catch (error) {
    console.error("Refreshing the Keycloak access token failed", error);
    return { ...token, error: "RefreshTokenError" };
  }
}

export const { handlers, auth, signIn, signOut } = NextAuth({
  providers: [Keycloak],
  callbacks: {
    async jwt({ token, account }) {
      if (account) {
        return {
          ...token,
          accessToken: account.access_token,
          expiresAt: account.expires_at,
          refreshToken: account.refresh_token,
        };
      }
      if (!token.expiresAt || Date.now() < (token.expiresAt - 30) * 1000) return token;
      if (!token.refreshToken) return { ...token, error: "RefreshTokenError" };
      return refreshAccessToken(token);
    },
    // The access token is exposed so server code and the /api proxy can call the API as the user.
    async session({ session, token }) {
      session.accessToken = token.accessToken;
      session.error = token.error;
      return session;
    },
  },
  events: {
    // Also end the Keycloak session, otherwise the next sign-in would silently reuse it.
    async signOut(message) {
      const refreshToken = "token" in message ? message.token?.refreshToken : undefined;
      if (!refreshToken) return;
      await fetch(oidcEndpoint("logout"), {
        method: "POST",
        body: new URLSearchParams({ ...clientCredentials(), refresh_token: refreshToken }),
      }).catch((error) => console.error("Ending the Keycloak session failed", error));
    },
  },
});

declare module "next-auth" {
  interface Session {
    accessToken?: string;
    error?: "RefreshTokenError";
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken?: string;
    expiresAt?: number;
    refreshToken?: string;
    error?: "RefreshTokenError";
  }
}
