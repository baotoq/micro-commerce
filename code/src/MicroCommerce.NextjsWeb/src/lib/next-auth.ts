import axios from "axios";
import NextAuth, { AuthOptions, getServerSession as getServerSessionBase } from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";

export const authOptions: AuthOptions = {
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
          const api = process.env.services__apiservice__http__0;

          const resToken = await axios.post(`${api}/login`, {
            email: credentials.username,
            password: credentials.password,
          });
          const config = {
            headers: {
              Authorization: "Bearer " + resToken.data.accessToken,
            },
          };

          console.log("Login successfully", resToken.data);

          // If no error and we have user data, return it
          if (resToken.data && resToken.status === 200) {
            const resUserInfo = await axios.get(`${api}/manage/info`, config);

            console.log("Get info successfully", resUserInfo.data);

            return {
              id: resUserInfo.data.email,
              email: resUserInfo.data.email,
              name: resUserInfo.data.email,
              accessToken: resToken.data.accessToken,
            };
          }
        }

        // Return null if user data could not be retrieved
        return null;
      },
    }),
  ],
  callbacks: {
    async jwt({ token, user }) {
      return { ...token, ...user };
    },
    async session({ session, token, user }) {
      session.user.id = token.id as string;
      session.accessToken = token.accessToken as string;

      return session;
    },
  },
};

export const getServerSession = () => getServerSessionBase(authOptions);
