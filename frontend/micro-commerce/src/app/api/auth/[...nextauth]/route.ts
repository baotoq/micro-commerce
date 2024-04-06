import axios from "axios";
import NextAuth, { AuthOptions } from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";

const authOptions: AuthOptions = {
  pages: {
    signIn: "/auth/signin",
  },
  providers: [
    CredentialsProvider({
      name: "Credentials",
      credentials: {
        username: { label: "Username", type: "text" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials, req) {
        if (credentials) {
          const res = await axios.post("http://localhost:5010/login", {
            email: credentials.username,
            password: credentials.password,
          });
          const config = {
            headers: {
              Authorization: "Bearer " + res.data.accessToken,
            },
          };
          // If no error and we have user data, return it
          if (res.data && res.status === 200) {
            const res = await axios.get(
              "http://localhost:5010/manage/info",
              config
            );

            console.log("Login successfully", res.data);

            return {
              id: res.data.email,
              email: res.data.email,
              name: res.data.email,
            };
          }
        }

        // Return null if user data could not be retrieved
        return null;
      },
    }),
  ],
};

const handler = NextAuth(authOptions);

export { handler as GET, handler as POST };
