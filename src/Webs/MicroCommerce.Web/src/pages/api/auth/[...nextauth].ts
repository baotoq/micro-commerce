import NextAuth from "next-auth";
import Providers from "next-auth/providers";

export default NextAuth({
  providers: [
    Providers.IdentityServer4({
      id: "identity-server4",
      name: "IdentityServer4",
      scope: "openid profile catalog.api",
      domain: "localhost:15001",
      clientId: "react-web",
      clientSecret: "secret",
    }),
  ],
});
