import NextAuth from "next-auth";
import Keycloak from "next-auth/providers/keycloak";

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [
    Keycloak({
      clientId: process.env.KEYCLOAK_CLIENT_ID!,
      clientSecret: process.env.KEYCLOAK_CLIENT_SECRET!,
      issuer: process.env.KEYCLOAK_ISSUER,
    }),
  ],
  callbacks: {
    async jwt({ token, account }) {
      // Persist the access_token from the provider to the token
      if (account) {
        token.accessToken = account.access_token;
        token.idToken = account.id_token;
        token.refreshToken = account.refresh_token;
        token.expiresAt = account.expires_at;
        token.isNewLogin = true;  // Flag for cart merge
      } else {
        token.isNewLogin = false;
      }
      return token;
    },
    async session({ session, token }) {
      // Send access token to the client for API calls
      session.accessToken = token.accessToken as string;
      session.isNewLogin = token.isNewLogin as boolean;
      return session;
    },
  },
});

