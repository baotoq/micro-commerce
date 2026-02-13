import "next-auth";

declare module "next-auth" {
  interface Session {
    accessToken?: string;
    isNewLogin?: boolean;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken?: string;
    idToken?: string;
    refreshToken?: string;
    expiresAt?: number;
    isNewLogin?: boolean;
  }
}

