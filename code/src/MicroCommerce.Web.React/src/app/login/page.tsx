"use client";

import { AuthPage } from "@components/auth-page";
import { useIsAuthenticated } from "@refinedev/core";
import { redirect } from "next/navigation";

export default function Login() {
  const { data } = useIsAuthenticated();

  if (data?.authenticated) {
    redirect(data?.redirectTo || "/");
  }

  return <AuthPage type="login" />;
}
