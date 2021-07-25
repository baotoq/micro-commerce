import NextAuth, { InitOptions } from "next-auth";
import Providers from "next-auth/providers";
import { NextApiRequest, NextApiResponse } from "next-auth/_utils";

const options: InitOptions = {
  providers: [
    Providers.IdentityServer4({
      id: "identity-server4",
      name: "IdentityServer4",
      scope: "openid profile email catalog-api",
      domain: process.env.AUTHORITY_DOMAIN,
      clientId: "react-web",
      clientSecret: "secret",
    }),
  ],
  callbacks: {
    async jwt(token, user, account, profile, isNewUser) {
      // Initial sign in
      console.log(account);
      if (account && user) {
        return {
          accessToken: account.accessToken,
          user,
        };
      }

      // Return previous token if the access token has not expired yet
      return token;
    },
    async session(session, token) {
      session.user = token
      return session;
    },
  },
};

export default (req: NextApiRequest, res: NextApiResponse) => NextAuth(req, res, options);
